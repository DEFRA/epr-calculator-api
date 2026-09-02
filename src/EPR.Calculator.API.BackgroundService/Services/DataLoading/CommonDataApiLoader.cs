using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Options;
using EPR.Calculator.API.Data.DataModels;
using EPR.CommonDataService.DataApi.CommonDataApi;
using EPR.CommonDataService.DataApi.CommonDataApi.Alignment;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.BackgroundService.Services.DataLoading;

/// <summary>
///     Loads POM and Organisation data required for a calculator run.
/// </summary>
public interface IDataLoader
{
    /// <summary>
    ///     Loads data for the specified calculator run.
    /// </summary>
    /// <param name="runContext">The context of the calculator run containing relevant data and parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    ///     The organisations and POMs streamed for this run. Empty lists if the loader is disabled.
    /// </returns>
    Task<(IReadOnlyList<CalculatorRunOrganisation> Organisations, IReadOnlyList<AlignmentPom> Poms)> LoadData(
        CalculatorRunContext runContext, CancellationToken cancellationToken = default);
}

/// <summary>
///     Loads POM and Organisation data by streaming it from the Common Data API into memory. Performs
///     no database access - persisting the data is the caller's responsibility.
/// </summary>
public class CommonDataApiLoader(
    IOptions<CommonDataApiLoaderOptions> options,
    IStreamOrganisationsRequestHandler organisationsHandler,
    IStreamPomsRequestHandler pomsHandler,
    ILogger<CommonDataApiLoader> logger,
    ITelemetry<CommonDataApiLoader> telemetry
) : IDataLoader
{
    private static readonly TimeSpan StreamDelayThreshold = TimeSpan.FromMinutes(5);
    private static readonly (IReadOnlyList<CalculatorRunOrganisation>, IReadOnlyList<AlignmentPom>) Empty = ([], []);

    /// <inheritdoc />
    public async Task<(IReadOnlyList<CalculatorRunOrganisation> Organisations, IReadOnlyList<AlignmentPom> Poms)> LoadData(
        CalculatorRunContext runContext, CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Disabled, skipping load");
            return Empty;
        }

        return await LoadDataCore(runContext, cancellationToken);
    }

    [ActivityTrace]
    private async Task<(IReadOnlyList<CalculatorRunOrganisation>, IReadOnlyList<AlignmentPom>)> LoadDataCore(
        RunContext runContext, CancellationToken cancellationToken)
    {
        var cutOffDate = runContext.DefaultParameters.CutOffDate is { } d
            ? new DateTimeOffset(DateTime.SpecifyKind(d, DateTimeKind.Utc))
            : (DateTimeOffset?)null;

        // If either stream fails, both should cancel.
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var linkedCt = linkedCts.Token;

        try
        {
            var orgsTask = StreamOrganisations(runContext.RelativeYear, cutOffDate, linkedCt);
            var pomsTask = StreamPoms(runContext.RelativeYear, cutOffDate, linkedCt);

            await Task.WhenAll(orgsTask, pomsTask);

            logger.LogTrace("Streamed {TotalOrgs} organisations and {TotalPoms} POMs", orgsTask.Result.Count, pomsTask.Result.Count);

            return (orgsTask.Result, pomsTask.Result);
        }
        catch when (!linkedCt.IsCancellationRequested)
        {
            await linkedCts.CancelAsync();
            throw;
        }
    }

    private Task<List<CalculatorRunOrganisation>> StreamOrganisations(int relativeYear, DateTimeOffset? cutOffDate, CancellationToken cancellationToken) =>
        telemetry.Activity(async () =>
        {
            var mapper = CommonDataApiLoaderMapper.MapOrganisation();

            await using var enumerator = organisationsHandler.Handle(relativeYear, cutOffDate).GetAsyncEnumerator(cancellationToken);

            var hasFirst = await telemetry.Metric(Metrics.OrgStreamDelay, () => enumerator.MoveNextAsync().AsTask(), StreamDelayThreshold, nameof(Metrics.OrgStreamDelay));

            var organisations = new List<CalculatorRunOrganisation>();

            while (hasFirst)
            {
                organisations.Add(mapper(enumerator.Current));
                hasFirst = await enumerator.MoveNextAsync();
            }

            return organisations;
        }, null, "OrgStream");

    private Task<List<AlignmentPom>> StreamPoms(int relativeYear, DateTimeOffset? cutOffDate, CancellationToken cancellationToken) =>
        telemetry.Activity(async () =>
        {
            var mapper = CommonDataApiLoaderMapper.MapPom(logger);

            await using var enumerator = pomsHandler.Handle(relativeYear, cutOffDate).GetAsyncEnumerator(cancellationToken);

            var hasFirst = await telemetry.Metric(Metrics.PomStreamDelay, () => enumerator.MoveNextAsync().AsTask(), StreamDelayThreshold, nameof(Metrics.PomStreamDelay));

            var poms = new List<AlignmentPom>();

            while (hasFirst)
            {
                poms.Add(mapper(enumerator.Current));
                hasFirst = await enumerator.MoveNextAsync();
            }

            return poms;
        }, null, "PomStream");
}
