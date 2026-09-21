using System.Data;
using System.Diagnostics;
using EFCore.BulkExtensions;
using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;
using EPR.Calculator.Api.DataApi.CommonDataApi.LoadTables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Api.DataApi.CommonDataApi;

/// <summary>
///     Streams the RPD source once into <c>data_api_load_organisations</c> / <c>data_api_load_poms</c>,
///     replacing whatever was there, so a run can read the staged rows back without querying the slow
///     source again (twice, for POMs). Each table is cleared and refilled in its own transaction.
/// </summary>
internal interface ILoadTableRefresher
{
    Task RefreshAsync(int relativeYear, CancellationToken cancellationToken = default);
}

internal sealed class LoadTableRefresher(
    IStreamOrganisationsRequestHandler organisationsHandler,
    IStreamPomsRequestHandler pomsHandler,
    IDbContextFactory<DataApiLoadContext> dbContextFactory,
    IOptions<DataApiLoadOptions> options) : ILoadTableRefresher
{
    public Task RefreshAsync(int relativeYear, CancellationToken cancellationToken = default) =>
        DataApiTelemetry.TraceAsync(typeof(LoadTableRefresher), nameof(RefreshAsync),
            () => RefreshCore(relativeYear, cancellationToken));

    private async Task RefreshCore(int relativeYear, CancellationToken cancellationToken)
    {
        var loadTs = DateTime.UtcNow;

        // If either side fails the other should stop too - a half-loaded pair is worse than none.
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var ct = linkedCts.Token;

        // Each stream needs its own context (EF contexts are not thread-safe).
        await using var orgDb = await dbContextFactory.CreateDbContextAsync(ct);
        await using var pomDb = await dbContextFactory.CreateDbContextAsync(ct);

        var orgTask = FillTable(orgDb, "data_api_load_organisations", options.Value.OrganisationBatchSize,
            organisationsHandler.Handle(relativeYear, ct), o => Map(o, loadTs), ct);
        var pomTask = FillTable(pomDb, "data_api_load_poms", options.Value.PomBatchSize,
            pomsHandler.Handle(relativeYear, ct), p => Map(p, loadTs), ct);

        try
        {
            await Task.WhenAll(orgTask, pomTask);
            await orgTask.Result.CommitAsync(ct);
            await pomTask.Result.CommitAsync(ct);
        }
        catch when (!ct.IsCancellationRequested)
        {
            await linkedCts.CancelAsync();
            throw;
        }
        finally
        {
            // A transaction disposed without a commit is rolled back by EF.
            if (orgTask.IsCompletedSuccessfully) await orgTask.Result.DisposeAsync();
            if (pomTask.IsCompletedSuccessfully) await pomTask.Result.DisposeAsync();
        }
    }

    private static async Task<IDbContextTransaction> FillTable<TSource, TTarget>(
        DataApiLoadContext dbContext,
        string tableName,
        int batchSize,
        IAsyncEnumerable<TSource> source,
        Func<TSource, TTarget> map,
        CancellationToken cancellationToken)
        where TTarget : class
    {
        return await DataApiTelemetry.TraceAsync(typeof(LoadTableRefresher), $"Fill:{tableName}", async () =>
        {
            var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            try
            {
                // Note app user does not have permission to TRUNCATE.
                await dbContext.Set<TTarget>().ExecuteDeleteAsync(cancellationToken);

                var batch = new List<TTarget>(batchSize);
                long total = 0;

                await foreach (var row in source.WithCancellation(cancellationToken))
                {
                    batch.Add(map(row));
                    if (batch.Count < batchSize)
                        continue;

                    total += await Flush(dbContext, batch, cancellationToken);
                }

                total += await Flush(dbContext, batch, cancellationToken);

                Activity.Current?.SetTag("rows_loaded", total);
                return transaction;
            }
            catch
            {
                await transaction.DisposeAsync();
                throw;
            }
        });
    }

    private static async Task<int> Flush<T>(DataApiLoadContext dbContext, List<T> batch, CancellationToken cancellationToken)
        where T : class
    {
        if (batch.Count == 0)
            return 0;

        await dbContext.BulkInsertAsync(batch, cancellationToken: cancellationToken);
        var count = batch.Count;
        batch.Clear();
        return count;
    }

    private static LoadOrganisation Map(PayCalOrganisation o, DateTime loadTs) => new()
    {
        OrganisationId = o.OrganisationId,
        SubsidiaryId = o.SubsidiaryId,
        SubmitterId = o.SubmitterId,
        OrganisationName = o.OrganisationName,
        TradingName = o.TradingName,
        StatusCode = o.StatusCode,
        LeaverDate = o.LeaverDate,
        JoinerDate = o.JoinerDate,
        RegulatorStatus = o.RegulatorStatus,
        ObligationStatus = o.ObligationStatus,
        NumDaysObligated = o.NumDaysObligated,
        ErrorCode = o.ErrorCode,
        SubmissionPeriodYear = o.SubmissionPeriodYear,
        HasH1 = o.HasH1,
        HasH2 = o.HasH2,
        FileName = o.FileName,
        IsResubmission = o.IsResubmission,
        CreatedDateTime = o.CreatedDateTime,
        LoadTs = loadTs
    };

    private static LoadPom Map(PayCalPom p, DateTime loadTs) => new()
    {
        OrganisationId = p.OrganisationId,
        SubsidiaryId = p.SubsidiaryId,
        SubmitterId = p.SubmitterId,
        SubmissionPeriod = p.SubmissionPeriod,
        SubmissionPeriodDescription = p.SubmissionPeriodDescription,
        PackagingActivity = p.PackagingActivity,
        PackagingType = p.PackagingType,
        PackagingClass = p.PackagingClass,
        PackagingMaterial = p.PackagingMaterial,
        PackagingMaterialSubtype = p.PackagingMaterialSubtype,
        PackagingMaterialWeight = p.PackagingMaterialWeight,
        RamRagRating = p.RamRagRating,
        FileName = p.FileName,
        IsResubmission = p.IsResubmission,
        CreatedDateTime = p.CreatedDateTime,
        LoadTs = loadTs
    };
}
