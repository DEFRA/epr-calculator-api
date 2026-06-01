using System.Data;
using System.Diagnostics.CodeAnalysis;
using EFCore.BulkExtensions;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Options;
using EPR.Calculator.Service.Function.Services.CommonDataApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Service.Function.Services.DataLoading;

/// <summary>
///     Loads POM and Organisation data by streaming from the Common Data API and bulk-inserting into the database.
/// </summary>
[ExcludeFromCodeCoverage(Justification =
    "The bulk insert/transaction behaviour here is not compatible with SQLite or InMemory databases.")]
public class CommonDataApiLoader(
    IOptions<CommonDataApiLoaderOptions> options,
    IDbContextFactory<ApplicationDBContext> dbContextFactory,
    ICommonDataApiClient httpClient,
    TimeProvider timeProvider,
    ITelemetryClient telemetry,
    ILogger<CommonDataApiLoader> logger)
    : IDataLoader
{
    /// <inheritdoc />
    public async Task LoadData(RelativeYear relativeYear,
        CancellationToken cancellationToken = default)
    {
        var opts = options.Value;

        if (!opts.Enabled)
        {
            logger.LogInformation("Disabled, skipping load");
            return;
        }

        await telemetry.TrackDuration("DataStream", () => Run(relativeYear, timeProvider.GetUtcNow(), cancellationToken));
    }

    private async Task<(long totalPoms, long totalOrgs)> Run(
        RelativeYear relativeYear, DateTimeOffset loadTime, CancellationToken cancellationToken)
    {
        // If either stream fails, both should cancel.
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var linkedCt = linkedCts.Token;
        var (pomStream, orgStream) = await GetStreams(relativeYear, loadTime, linkedCt);

        try
        {
            return await UpdateDatabase(pomStream, orgStream, linkedCt);
        }
        catch when (!linkedCt.IsCancellationRequested)
        {
            await linkedCts.CancelAsync();
            throw;
        }
        finally
        {
            await pomStream.Enumerator.DisposeAsync();
            await orgStream.Enumerator.DisposeAsync();
        }
    }

    private async Task<(InitialisedStream<PomData> pomStream, InitialisedStream<OrganisationData> orgStream)>
        GetStreams(RelativeYear relativeYear, DateTimeOffset loadTime, CancellationToken linkedCt)
    {
        var pomStream = httpClient.StreamPoms(relativeYear, linkedCt)
            .Select(CommonDataApiLoaderMapper.MapPom(loadTime))
            .Chunk(options.Value.PomBatchSize)
            .GetAsyncEnumerator(linkedCt);

        var orgStream = httpClient.StreamOrganisations(relativeYear, linkedCt)
            .Select(CommonDataApiLoaderMapper.MapOrganisation(loadTime))
            .Chunk(options.Value.OrganisationBatchSize)
            .GetAsyncEnumerator(linkedCt);

        try
        {
            // Await the arrival of the first element in both streams before proceeding.
            // Avoids holding locks during network delays, since both are (currently) highly variable.
            var pomStarted = TrackStreamDelay(pomStream);
            var orgStarted = TrackStreamDelay(orgStream);

            await Task.WhenAll(pomStarted, orgStarted);
            return (new InitialisedStream<PomData>(pomStream, !pomStarted.Result),
                new InitialisedStream<OrganisationData>(orgStream, !orgStarted.Result));
        }
        catch
        {
            await pomStream.DisposeAsync();
            await orgStream.DisposeAsync();
            throw;
        }

        Task<bool> TrackStreamDelay<T>(IAsyncEnumerator<IEnumerable<T>> stream) =>
            telemetry.TrackDuration($"{typeof(T).Name}StreamDelayMs", () => stream.MoveNextAsync().AsTask());
    }

    private async Task<(long totalPoms, long totalOrgs)> UpdateDatabase(InitialisedStream<PomData> pomStream,
        InitialisedStream<OrganisationData> orgStream, CancellationToken linkedCt)
    {
        // Each stream needs its own DbContext as it is not thread-safe.
        await using var pomDb = await dbContextFactory.CreateDbContextAsync(linkedCt);
        await using var orgDb = await dbContextFactory.CreateDbContextAsync(linkedCt);

        var pomsInserted = BulkInsert(pomDb, pomStream, linkedCt);
        var orgsInserted = BulkInsert(orgDb, orgStream, linkedCt);

        try
        {
            await Task.WhenAll(pomsInserted, orgsInserted);

            var (pomTxn, totalPoms) = pomsInserted.Result;
            var (orgTxn, totalOrgs) = orgsInserted.Result;

            // Note that if orgTxn throws we'll have already committed pomTxn and end up with a mixed state...
            await pomTxn.CommitAsync(linkedCt);
            await orgTxn.CommitAsync(linkedCt);

            return (totalPoms, totalOrgs);
        }
        finally
        {
            // Handles scenarios where one or both BulkInsert tasks have errored.
            // Note that transactions are rolled back by EF if they are disposed without being committed.
            if (pomsInserted.IsCompletedSuccessfully) await pomsInserted.Result.transaction.DisposeAsync();
            if (orgsInserted.IsCompletedSuccessfully) await orgsInserted.Result.transaction.DisposeAsync();
        }
    }

    private async Task<(IDbContextTransaction transaction, long total)> BulkInsert<TEntity>(
        ApplicationDBContext dbContext,
        InitialisedStream<TEntity> stream,
        CancellationToken ct)
        where TEntity : class
    {
        return await logger.LogDuration(async () =>
        {
            var txn = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
            long total = 0;
            var batchNumber = 0;

            try
            {
                // Clears all existing data - the underlying tables are only used for this data loading process.
                await dbContext.Set<TEntity>().ExecuteDeleteAsync(ct);

                if (stream.IsEmpty)
                {
                    logger.LogWarning("{StreamType}: Stream ended without receiving any records",
                        typeof(TEntity).Name);

                    return (txn, total);
                }

                do
                {
                    var batch = stream.Enumerator.Current;
                    await dbContext.BulkInsertAsync(batch, cancellationToken: ct);
                    total += batch.Count;
                    batchNumber++;

                    logger.LogTrace("{StreamType}: Inserted {Count} entities for batch {BatchNumber}",
                        typeof(TEntity).Name, batch.Count, batchNumber);
                } while (await stream.Enumerator.MoveNextAsync());

                logger.LogDebug("{StreamType}: Inserted {Total} total entities",
                    typeof(TEntity).Name, total);

                return (txn, total);
            }
            catch
            {
                await txn.DisposeAsync();
                throw;
            }
        }, typeof(TEntity).Name);
    }

    private sealed record InitialisedStream<T>(IAsyncEnumerator<IList<T>> Enumerator, bool IsEmpty);
}
