using System.Diagnostics;
using EPR.Calculator.Api.DataApi.AcceptedFileSelection;
using EPR.Calculator.Api.DataApi.Alignment;
using EPR.Calculator.Api.DataApi.CommonDataApi;
using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;
using EPR.Calculator.Api.DataApi.CommonDataApi.LoadTables;
using EPR.Calculator.Api.DataApi.Models;
using EPR.Calculator.Api.DataApi.ObligationDetermination;
using EPR.Calculator.Api.DataApi.PomEligibility;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Api.DataApi.Services;

/// <summary>
///     Produces the full set of data a calculator run needs from organisation/POM data, in a single
///     call: streams the raw Synapse data, applies every business rule (file selection, obligation
///     determination, POM eligibility, period flags, error/warning detection), and aligns the result
///     into producers ready for calculation. Performs no database access outside of the Synapse
///     streams themselves - persisting the result is the caller's responsibility.
/// </summary>
public interface IProducerDataService
{
    Task<IReadOnlyList<ProducerRecord>> GetProducerData(
        int relativeYear,
        DateTimeOffset? cutOffDate,
        IReadOnlyList<string> materialCodes,
        CancellationToken cancellationToken = default);
}

internal sealed class ProducerDataService(
    IPayCalDataSource dataSource,
    IAcceptedFileSelector acceptedFileSelector,
    IProducerObligationDeterminer obligationDeterminer,
    IPomEligibilityFilter pomEligibilityFilter,
    IOrganisationPeriodFlagsCalculator organisationPeriodFlagsCalculator,
    IProducerErrorDetector errorDetector,
    IProducerPomAligner aligner,
    ILoadTableRefresher loadTableRefresher,
    IOptions<DataApiLoadOptions> loadOptions
) : IProducerDataService
{
    private const string ErrorStatus = "E";
    private const string HouseholdDrinksContainersType = "HDC";
    private const string GlassMaterial = "GL";
    private static readonly HashSet<string> ReportablePackagingTypes = ["HH", "CW", "PB"];
    private static readonly HashSet<string> ValidRagRatings = ["R", "A", "G", "R-M", "A-M", "G-M"];

    public async Task<IReadOnlyList<ProducerRecord>> GetProducerData(
        int relativeYear,
        DateTimeOffset? cutOffDate,
        IReadOnlyList<string> materialCodes,
        CancellationToken cancellationToken = default)
    {
        using var activity = DataApiTelemetry.StartActivity(typeof(ProducerDataService), nameof(GetProducerData));
        var allocatedBefore = DataApiTelemetry.CaptureMemoryMetrics ? GC.GetTotalAllocatedBytes() : 0;

        // If either stream fails, both should cancel.
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var linkedCt = linkedCts.Token;

        try
        {
            var result = await GetProducerDataCore(relativeYear, cutOffDate, materialCodes, linkedCt);
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
        finally
        {
            if (DataApiTelemetry.CaptureMemoryMetrics)
            {
                activity?.SetTag("allocated_bytes", GC.GetTotalAllocatedBytes() - allocatedBefore);
                activity?.SetTag("heap_bytes", GC.GetTotalMemory(forceFullCollection: false));
            }
        }
    }

    private async Task<IReadOnlyList<ProducerRecord>> GetProducerDataCore(
        int relativeYear,
        DateTimeOffset? cutOffDate,
        IReadOnlyList<string> materialCodes,
        CancellationToken cancellationToken)
    {
        // When the load-table stage is on, pull the RPD source once into data_api_load_* first; the
        // streams below then read those tables (dataSource is LoadTableDataSource). Off: they read RPD.
        if (loadOptions.Value.Enabled)
            await loadTableRefresher.RefreshAsync(relativeYear, cancellationToken);

        var orgsTask = StreamOrganisations(relativeYear, cutOffDate, cancellationToken);
        var pomsTask = StreamPoms(relativeYear, cutOffDate, cancellationToken);

        await Task.WhenAll(orgsTask, pomsTask);

        var rawOrganisations = orgsTask.Result;
        var rawPoms = pomsTask.Result;

        // POM eligibility (both H1 and H2 submitted, a registration exists) and each organisation's
        // own HasH1/HasH2 flags both depend on the POM stream, so they can only run once both
        // streams have finished.
        //
        // Cancelled registrations are excluded here - the obligation stream has to keep them (they
        // drive "Not Obligated"), but sp_GetPaycalPomData's registration gate was Granted/Accepted
        // only. The org stream only ever carries Granted/Accepted/Cancelled, so "not Cancelled" is
        // equivalent and also tolerates fixtures that leave RegulatorStatus unset.
        var registeredOrganisationIds = rawOrganisations
            .Where(o => o.RegulatorStatus is not "Cancelled")
            .Select(o => o.OrganisationId)
            .ToHashSet();
        var eligiblePoms = pomEligibilityFilter.Filter(rawPoms, registeredOrganisationIds);
        var organisationsWithPeriodFlags = organisationPeriodFlagsCalculator.ApplyPeriodFlags(rawOrganisations, rawPoms);

        var organisations = organisationsWithPeriodFlags.Select(ValidateOrganisation).ToImmutableList();
        // sp_GetPaycalPomData applied the reportable-packaging filter upstream of every consumer,
        // error detection included - not just alignment.
        var poms = eligiblePoms
            .Where(p => IsReportablePackaging(p))
            .Select(NormalisePom)
            .ToImmutableList();

        var detection = errorDetector.Detect(organisations, poms);

        var matchedPoms = poms
            .Where(p => !detection.UnmatchedKeys.Contains((p.OrganisationId, p.SubsidiaryId)))
            .ToImmutableList();

        var dedupedOrganisations = aligner.DedupeOrganisations(organisations);
        var aligned = aligner.Align(dedupedOrganisations, matchedPoms, materialCodes).ToImmutableList();

        return MergeProducerRecords(dedupedOrganisations, aligned, detection.Errors);
    }

    /// <summary>
    ///     Combines the aligner's per-obligated-organisation output with detection's error/warning
    ///     results into the final per-organisation record set. Three sources feed the result:
    ///     obligated organisations (from <paramref name="aligned" />, keyed by (OrganisationId,
    ///     SubsidiaryId) since detection doesn't distinguish submitters), "E"-status organisations
    ///     (which the aligner never looks at, since it only iterates obligated ones), and "orphan"
    ///     errors - almost always "Missing Registration Data", which is
    ///     POM-driven and can reference an org/subsidiary combo with no exact registration match.
    /// </summary>
    private static IReadOnlyList<ProducerRecord> MergeProducerRecords(
        IReadOnlyList<PayCalOrganisation> dedupedOrganisations,
        IReadOnlyList<ProducerRecord> aligned,
        IReadOnlyList<OrganisationCalculationError> errors)
    {
        var alignedByKey = aligned.ToLookup(r => (r.OrganisationId, r.SubsidiaryId));
        var errorsByKey = errors.ToLookup(e => (e.OrganisationId, e.SubsidiaryId));

        var records = new List<ProducerRecord>(aligned.Count);
        var coveredKeys = new HashSet<(int OrganisationId, string? SubsidiaryId)>();

        foreach (var key in alignedByKey.Select(g => g.Key))
        {
            coveredKeys.Add(key);
            var (hardErrors, warnings) = SplitErrors(errorsByKey[key]);

            foreach (var record in alignedByKey[key])
                records.Add(record with { Errors = hardErrors, Warnings = warnings });
        }

        // "E"-status organisations: never obligated, so the aligner never produces a record for them.
        foreach (var organisation in dedupedOrganisations.Where(o => o.ObligationStatus == ErrorStatus))
        {
            var key = (organisation.OrganisationId, organisation.SubsidiaryId);
            coveredKeys.Add(key);
            var (hardErrors, warnings) = SplitErrors(errorsByKey[key]);
            records.Add(ToProducerRecord(organisation, hardErrors, warnings));
        }

        // Orphan errors: a key with no aligned or "E"-status row at all. Borrow identity fields from
        // any other row sharing the OrganisationId (e.g. a POM submitted under a subsidiary/submitter
        // combo that doesn't match any registration); fall back to an empty identity in the rare case
        // no registration exists for the organisation at all - mirroring today's behaviour, where such
        // an organisation never gets a CalculatorRunOrganisation snapshot either.
        foreach (var key in errorsByKey.Select(g => g.Key))
        {
            if (!coveredKeys.Add(key))
                continue;

            var (hardErrors, warnings) = SplitErrors(errorsByKey[key]);
            var anyOrganisationRow = dedupedOrganisations.FirstOrDefault(o => o.OrganisationId == key.OrganisationId);

            records.Add(anyOrganisationRow is not null
                ? ToProducerRecord(anyOrganisationRow with { SubsidiaryId = key.SubsidiaryId, SubmitterId = null }, hardErrors, warnings)
                : ToOrphanProducerRecord(key, hardErrors, warnings));
        }

        return records;
    }

    private static (IReadOnlyList<ProducerCalculationError> HardErrors, IReadOnlyList<ProducerCalculationError> Warnings) SplitErrors(
        IEnumerable<OrganisationCalculationError> errors)
    {
        var list = errors.Select(e => e.Error).ToImmutableList();
        return (list.Where(e => !e.IsWarning).ToImmutableList(), list.Where(e => e.IsWarning).ToImmutableList());
    }

    private static ProducerRecord ToProducerRecord(
        PayCalOrganisation organisation,
        IReadOnlyList<ProducerCalculationError> errors,
        IReadOnlyList<ProducerCalculationError> warnings) => new()
    {
        OrganisationId = organisation.OrganisationId,
        SubsidiaryId = organisation.SubsidiaryId,
        ProducerName = organisation.OrganisationName,
        TradingName = organisation.TradingName,
        DaysObligated = organisation.NumDaysObligated,
        JoinerDate = organisation.JoinerDate,
        LeaverDate = organisation.LeaverDate,
        StatusCode = organisation.StatusCode,
        ErrorCode = organisation.ErrorCode,
        Errors = errors,
        Warnings = warnings,
        ReportedMaterials = []
    };

    private static ProducerRecord ToOrphanProducerRecord(
        (int OrganisationId, string? SubsidiaryId) key,
        IReadOnlyList<ProducerCalculationError> errors,
        IReadOnlyList<ProducerCalculationError> warnings) => new()
    {
        OrganisationId = key.OrganisationId,
        SubsidiaryId = key.SubsidiaryId,
        ProducerName = string.Empty,
        Errors = errors,
        Warnings = warnings,
        ReportedMaterials = []
    };

    private async Task<List<PayCalOrganisation>> StreamOrganisations(int relativeYear, DateTimeOffset? cutOffDate, CancellationToken cancellationToken)
    {
        var rawOrganisations = await DataApiTelemetry.TraceAsync(typeof(ProducerDataService), "BufferOrganisationStream", async () =>
        {
            var buffer = new List<PayCalOrganisation>();
            await foreach (var organisation in dataSource.StreamOrganisations(relativeYear, cancellationToken).WithCancellation(cancellationToken))
                buffer.Add(organisation);
            return buffer;
        });

        // Every candidate accepted file is streamed unfiltered - pick the winning file per
        // org/submitter/period (honouring the cut-off date) before obligation determination, which
        // needs every row for the run up front since it aggregates across rows (per producer/submission
        // period) rather than deciding a row in isolation.
        var latestOrganisations = acceptedFileSelector.SelectLatestOrganisationFiles(rawOrganisations, cutOffDate);
        return obligationDeterminer.Determine(latestOrganisations).ToList();
    }

    private async Task<List<PayCalPom>> StreamPoms(int relativeYear, DateTimeOffset? cutOffDate, CancellationToken cancellationToken)
    {
        // The raw POM stream is one row per line item and mostly superseded resubmission files; buffering
        // all of it costs ~1 GB+ at production volume. Instead take two passes: the first keeps only the
        // per-file metadata and picks the winning file per org/submitter/period; the second re-streams and
        // buffers only the winners' line items.
        var winningFileNames = await DataApiTelemetry.TraceAsync(typeof(ProducerDataService), "SelectPomFiles", async () =>
        {
            var candidates = new Dictionary<(int?, string?, string?, string?), PomFileCandidate>();
            await foreach (var pom in dataSource.StreamPoms(relativeYear, cancellationToken).WithCancellation(cancellationToken))
                candidates.TryAdd(
                    (pom.OrganisationId, pom.SubmitterId, pom.SubmissionPeriod, pom.FileName),
                    new PomFileCandidate(pom.OrganisationId, pom.SubmitterId, pom.SubmissionPeriod, pom.FileName, pom.IsResubmission, pom.CreatedDateTime));

            return acceptedFileSelector.SelectWinningPomFileNames(candidates.Values, cutOffDate);
        });

        return await DataApiTelemetry.TraceAsync(typeof(ProducerDataService), "BufferPomStream", async () =>
        {
            var buffer = new List<PayCalPom>();
            await foreach (var pom in dataSource.StreamPoms(relativeYear, cancellationToken).WithCancellation(cancellationToken))
                if (winningFileNames.TryGetValue((pom.OrganisationId, pom.SubmitterId, pom.SubmissionPeriod), out var winner) &&
                    pom.FileName == winner)
                    buffer.Add(pom);
            return buffer;
        });
    }

    // Downstream stages rely on these being present and well-formed, so reject a bad row up front
    // rather than letting them null-check every field.
    private static PayCalOrganisation ValidateOrganisation(PayCalOrganisation r)
    {
        if (r.ObligationStatus is null)
            throw Invalid(nameof(PayCalOrganisation), nameof(r.ObligationStatus), r.ObligationStatus);
        if (!Guid.TryParse(r.SubmitterId, out _))
            throw Invalid(nameof(PayCalOrganisation), nameof(r.SubmitterId), r.SubmitterId);

        return r;
    }

    /// <summary>
    ///     The reportable-packaging rule from <c>sp_GetPaycalPomData</c>'s final WHERE clause: household,
    ///     consumer waste and public bin count regardless of material; household drinks containers count
    ///     only for glass. Applied upstream of both error detection and alignment, matching where the
    ///     stored procedure applied it.
    /// </summary>
    private static bool IsReportablePackaging(PayCalPom pom) =>
        pom.PackagingType is not null &&
        (ReportablePackagingTypes.Contains(pom.PackagingType) ||
         (pom.PackagingType == HouseholdDrinksContainersType && pom.PackagingMaterial == GlassMaterial));

    private static PayCalPom NormalisePom(PayCalPom r)
    {
        if (!Guid.TryParse(r.SubmitterId, out _))
            throw Invalid(nameof(PayCalPom), nameof(r.SubmitterId), r.SubmitterId);

        return r with { RamRagRating = SafeParseRamRagRating(r) };
    }

    private static FormatException Invalid(string entity, string property, object? value) =>
        new($"Invalid {entity}.{property}: {value}");

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
