using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Options;
using EPR.Calculator.API.Data.DataModels;
using EPR.CommonDataService.DataApi.CommonDataApi;
using EPR.CommonDataService.DataApi.AcceptedFileSelection;
using EPR.CommonDataService.DataApi.Alignment;
using EPR.CommonDataService.DataApi.CommonDataApi.Entities;
using EPR.CommonDataService.DataApi.ObligationDetermination;
using EPR.CommonDataService.DataApi.PomEligibility;
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
    IAcceptedFileSelector acceptedFileSelector,
    IProducerObligationDeterminer obligationDeterminer,
    IPomEligibilityFilter pomEligibilityFilter,
    IOrganisationPeriodFlagsCalculator organisationPeriodFlagsCalculator,
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

            var organisations = orgsTask.Result;
            var poms = pomsTask.Result;

            logger.LogTrace("Streamed {TotalOrgs} organisations and {TotalPoms} POMs", organisations.Count, poms.Count);

            // POM eligibility (both H1 and H2 submitted, a registration exists) and each organisation's
            // own HasH1/HasH2 flags both depend on the POM stream, so they can only run once both
            // streams have finished.
            var organisationIds = organisations
                .Where(o => o.OrganisationId is not null)
                .Select(o => o.OrganisationId!.Value)
                .ToHashSet();
            var eligiblePoms = pomEligibilityFilter.Filter(poms, organisationIds);
            var organisationsWithPeriodFlags = organisationPeriodFlagsCalculator.ApplyPeriodFlags(organisations, poms);

            var orgMapper = CommonDataApiLoaderMapper.MapOrganisation();
            var pomMapper = CommonDataApiLoaderMapper.MapPom(logger);

            return (organisationsWithPeriodFlags.Select(orgMapper).ToList(), eligiblePoms.Select(pomMapper).ToList());
        }
        catch when (!linkedCt.IsCancellationRequested)
        {
            await linkedCts.CancelAsync();
            throw;
        }
    }

    private async Task<List<PayCalOrganisation>> StreamOrganisations(int relativeYear, DateTimeOffset? cutOffDate, CancellationToken cancellationToken)
    {
        await using var enumerator = organisationsHandler.Handle(relativeYear, cancellationToken).GetAsyncEnumerator(cancellationToken);

        var hasFirst = await telemetry.Metric(Metrics.OrgStreamDelay, () => enumerator.MoveNextAsync().AsTask(), StreamDelayThreshold, nameof(Metrics.OrgStreamDelay));

        var rawOrganisations = new List<PayCalOrganisation>();

        while (hasFirst)
        {
            rawOrganisations.Add(enumerator.Current);
            hasFirst = await enumerator.MoveNextAsync();
        }

        // Every candidate accepted file is streamed unfiltered - pick the winning file per
        // org/submitter/period (honouring the cut-off date) before obligation determination, which
        // needs every row for the run up front since it aggregates across rows (per producer/submission
        // period) rather than deciding a row in isolation.
        var latestOrganisations = acceptedFileSelector.SelectLatestOrganisationFiles(rawOrganisations, cutOffDate);
        return obligationDeterminer.Determine(latestOrganisations).ToList();
    }

    private async Task<List<PayCalPom>> StreamPoms(int relativeYear, DateTimeOffset? cutOffDate, CancellationToken cancellationToken)
    {
        await using var enumerator = pomsHandler.Handle(relativeYear, cancellationToken).GetAsyncEnumerator(cancellationToken);

        var hasFirst = await telemetry.Metric(Metrics.PomStreamDelay, () => enumerator.MoveNextAsync().AsTask(), StreamDelayThreshold, nameof(Metrics.PomStreamDelay));

        var poms = new List<PayCalPom>();

        while (hasFirst)
        {
            poms.Add(enumerator.Current);
            hasFirst = await enumerator.MoveNextAsync();
        }

        return acceptedFileSelector.SelectLatestPomFiles(poms, cutOffDate).ToList();
    }
}
