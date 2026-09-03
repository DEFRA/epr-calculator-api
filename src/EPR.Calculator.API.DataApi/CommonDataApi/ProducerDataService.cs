using System.Diagnostics;
using EPR.CommonDataService.DataApi.AcceptedFileSelection;
using EPR.CommonDataService.DataApi.Alignment;
using EPR.CommonDataService.DataApi.CommonDataApi.Entities;
using EPR.CommonDataService.DataApi.ObligationDetermination;
using EPR.CommonDataService.DataApi.PomEligibility;

namespace EPR.CommonDataService.DataApi.CommonDataApi;

/// <summary>
///     Produces the full set of data a calculator run needs from organisation/POM data, in a single
///     call: streams the raw Synapse data, applies every business rule (file selection, obligation
///     determination, POM eligibility, period flags, error/warning detection), and aligns the result
///     into producers ready for calculation. Performs no database access outside of the Synapse
///     streams themselves - persisting the result is the caller's responsibility.
/// </summary>
public interface IProducerDataService
{
    Task<ProducerCalculationData> GetProducerData(
        int relativeYear,
        DateTimeOffset? cutOffDate,
        IReadOnlyList<string> materialCodes,
        IReadOnlyCollection<int> invoicedOrganisationIds,
        CancellationToken cancellationToken = default);
}

public sealed class ProducerDataService(
    IStreamOrganisationsRequestHandler organisationsHandler,
    IStreamPomsRequestHandler pomsHandler,
    IAcceptedFileSelector acceptedFileSelector,
    IProducerObligationDeterminer obligationDeterminer,
    IPomEligibilityFilter pomEligibilityFilter,
    IOrganisationPeriodFlagsCalculator organisationPeriodFlagsCalculator,
    IProducerErrorDetector errorDetector,
    IProducerPomAligner aligner
) : IProducerDataService
{
    private static readonly HashSet<string> ValidRagRatings = ["R", "A", "G", "R-M", "A-M", "G-M"];

    public async Task<ProducerCalculationData> GetProducerData(
        int relativeYear,
        DateTimeOffset? cutOffDate,
        IReadOnlyList<string> materialCodes,
        IReadOnlyCollection<int> invoicedOrganisationIds,
        CancellationToken cancellationToken = default)
    {
        using var activity = DataApiTelemetry.StartActivity(typeof(ProducerDataService), nameof(GetProducerData));

        // If either stream fails, both should cancel.
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var linkedCt = linkedCts.Token;

        try
        {
            var result = await GetProducerDataCore(relativeYear, cutOffDate, materialCodes, invoicedOrganisationIds, linkedCt);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (!linkedCt.IsCancellationRequested)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            await linkedCts.CancelAsync();
            throw;
        }
    }

    private async Task<ProducerCalculationData> GetProducerDataCore(
        int relativeYear,
        DateTimeOffset? cutOffDate,
        IReadOnlyList<string> materialCodes,
        IReadOnlyCollection<int> invoicedOrganisationIds,
        CancellationToken cancellationToken)
    {
        var orgsTask = StreamOrganisations(relativeYear, cutOffDate, cancellationToken);
        var pomsTask = StreamPoms(relativeYear, cutOffDate, cancellationToken);

        await Task.WhenAll(orgsTask, pomsTask);

        var rawOrganisations = orgsTask.Result;
        var rawPoms = pomsTask.Result;

        // POM eligibility (both H1 and H2 submitted, a registration exists) and each organisation's
        // own HasH1/HasH2 flags both depend on the POM stream, so they can only run once both
        // streams have finished.
        var organisationIds = rawOrganisations
            .Where(o => o.OrganisationId is not null)
            .Select(o => o.OrganisationId!.Value)
            .ToHashSet();
        var eligiblePoms = pomEligibilityFilter.Filter(rawPoms, organisationIds);
        var organisationsWithPeriodFlags = organisationPeriodFlagsCalculator.ApplyPeriodFlags(rawOrganisations, rawPoms);

        var organisations = organisationsWithPeriodFlags.Select(MapOrganisation).ToImmutableList();
        var poms = eligiblePoms.Select(MapPom).ToImmutableList();

        var detection = errorDetector.Detect(organisations, poms, invoicedOrganisationIds);

        var matchedPoms = poms
            .Where(p => !detection.UnmatchedKeys.Contains((p.OrganisationId.GetValueOrDefault(), p.SubsidiaryId)))
            .ToImmutableList();

        var dedupedOrganisations = aligner.DedupeOrganisations(organisations);
        var producers = aligner.Align(dedupedOrganisations, matchedPoms, materialCodes).ToImmutableList();

        return new ProducerCalculationData
        {
            Organisations = organisations,
            Producers = producers,
            Errors = detection.Errors
        };
    }

    private async Task<List<PayCalOrganisation>> StreamOrganisations(int relativeYear, DateTimeOffset? cutOffDate, CancellationToken cancellationToken)
    {
        var rawOrganisations = new List<PayCalOrganisation>();

        await foreach (var organisation in organisationsHandler.Handle(relativeYear, cancellationToken).WithCancellation(cancellationToken))
            rawOrganisations.Add(organisation);

        // Every candidate accepted file is streamed unfiltered - pick the winning file per
        // org/submitter/period (honouring the cut-off date) before obligation determination, which
        // needs every row for the run up front since it aggregates across rows (per producer/submission
        // period) rather than deciding a row in isolation.
        var latestOrganisations = acceptedFileSelector.SelectLatestOrganisationFiles(rawOrganisations, cutOffDate);
        return obligationDeterminer.Determine(latestOrganisations).ToList();
    }

    private async Task<List<PayCalPom>> StreamPoms(int relativeYear, DateTimeOffset? cutOffDate, CancellationToken cancellationToken)
    {
        var poms = new List<PayCalPom>();

        await foreach (var pom in pomsHandler.Handle(relativeYear, cancellationToken).WithCancellation(cancellationToken))
            poms.Add(pom);

        return acceptedFileSelector.SelectLatestPomFiles(poms, cutOffDate).ToList();
    }

    private static AlignmentOrganisation MapOrganisation(PayCalOrganisation r) => new()
    {
        OrganisationId = r.OrganisationId ?? throw new FormatException(
            $"Invalid {nameof(PayCalOrganisation)}.{nameof(PayCalOrganisation.OrganisationId)}: {r.OrganisationId}"),
        SubsidiaryId = r.SubsidiaryId,
        OrganisationName = r.OrganisationName ?? throw new FormatException(
            $"Invalid {nameof(PayCalOrganisation)}.{nameof(PayCalOrganisation.OrganisationName)}: {r.OrganisationName}"),
        TradingName = r.TradingName,
        StatusCode = r.StatusCode,
        ErrorCode = r.ErrorCode,
        JoinerDate = r.JoinerDate,
        LeaverDate = r.LeaverDate,
        ObligationStatus = r.ObligationStatus ?? throw new FormatException(
            $"Invalid {nameof(PayCalOrganisation)}.{nameof(PayCalOrganisation.ObligationStatus)}: {r.ObligationStatus}"),
        DaysObligated = r.NumDaysObligated,
        SubmitterId = Guid.TryParse(r.SubmitterId, out var guid)
            ? guid
            : throw new FormatException($"Invalid {nameof(PayCalOrganisation)}.{nameof(PayCalOrganisation.SubmitterId)}: {r.SubmitterId}"),
        HasH1 = r.HasH1,
        HasH2 = r.HasH2
    };

    private static AlignmentPom MapPom(PayCalPom r) => new()
    {
        SubmissionPeriod = r.SubmissionPeriod,
        OrganisationId = r.OrganisationId,
        SubsidiaryId = r.SubsidiaryId,
        PackagingType = r.PackagingType,
        PackagingMaterial = r.PackagingMaterial,
        PackagingMaterialWeight = r.PackagingMaterialWeight,
        RamRagRating = SafeParseRamRagRating(r),
        SubmitterId = Guid.TryParse(r.SubmitterId, out var guid)
            ? guid
            : throw new FormatException($"Invalid {nameof(PayCalPom)}.{nameof(PayCalPom.SubmitterId)}: {r.SubmitterId}")
    };

    private static string? SafeParseRamRagRating(PayCalPom pom)
    {
        if (string.IsNullOrWhiteSpace(pom.RamRagRating))
            return null;

        var trimmed = pom.RamRagRating.Trim();
        if (ValidRagRatings.Contains(trimmed))
            return trimmed;

        Activity.Current?.AddEvent(new ActivityEvent("InvalidRagRating", tags: new ActivityTagsCollection
        {
            ["OrganisationId"] = pom.OrganisationId,
            ["SubsidiaryId"] = pom.SubsidiaryId,
            ["SubmitterId"] = pom.SubmitterId,
            ["RamRagRating"] = pom.RamRagRating,
            ["PackagingMaterial"] = pom.PackagingMaterial
        }));

        return "R"; // Treat as Red when the value can't be recognised.
    }
}
