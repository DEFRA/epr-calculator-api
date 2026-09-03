namespace EPR.CommonDataService.DataApi.Alignment;

public sealed record ProducerErrorDetectionResult
{
    /// <summary>
    ///     Every error/warning row, including holding-company roll-ups - the full set to persist.
    /// </summary>
    public required IReadOnlyList<ProducerCalculationError> Errors { get; init; }

    /// <summary>
    ///     Org/subsidiary keys with a hard (non-warning) error - these should be excluded from
    ///     downstream alignment. Deliberately excludes holding-company roll-ups (which would otherwise
    ///     incorrectly exclude a holding-level POM row for a producer whose error is subsidiary-scoped).
    /// </summary>
    public required IReadOnlySet<(int OrganisationId, string? SubsidiaryId)> UnmatchedKeys { get; init; }
}

public interface IProducerErrorDetector
{
    /// <summary>
    ///     Runs every error/warning rule against the (pre-dedup) organisation and POM populations, and
    ///     rolls up a holding-company-level error for any producer whose own errors are all
    ///     subsidiary-scoped.
    /// </summary>
    /// <param name="organisations">The full, non-deduped organisation population for the run.</param>
    /// <param name="poms">The full POM population for the run.</param>
    /// <param name="invoicedOrganisationIds">
    ///     Organisation ids invoiced in a previous run this financial year - obligated-error/warning rows
    ///     are only raised for a producer with no POM match if they were previously invoiced.
    /// </param>
    ProducerErrorDetectionResult Detect(
        IReadOnlyCollection<AlignmentOrganisation> organisations,
        IReadOnlyCollection<AlignmentPom> poms,
        IReadOnlyCollection<int> invoicedOrganisationIds);
}

public sealed class ProducerErrorDetector : IProducerErrorDetector
{
    private const string ObligatedStatus = "O";
    private const string ErrorStatus = "E";

    public ProducerErrorDetectionResult Detect(
        IReadOnlyCollection<AlignmentOrganisation> organisations,
        IReadOnlyCollection<AlignmentPom> poms,
        IReadOnlyCollection<int> invoicedOrganisationIds)
    {
        var obligatedErrors = HandleObligatedErrors(poms, organisations, invoicedOrganisationIds);
        var missingRegErrors = HandleMissingRegistrationData(poms, organisations);
        var obligatedWarnings = HandleObligatedWarnings(poms, organisations, invoicedOrganisationIds);
        var missingPomErrors = HandleMissingPomData(poms, organisations);

        var calcErrors = obligatedErrors
            .Concat(missingRegErrors)
            .Concat(obligatedWarnings)
            .Concat(missingPomErrors)
            .ToImmutableList();

        // Roll up a holding-company-level error for any producer whose errors are all subsidiary-scoped,
        // so the holding company itself also shows up in the error report.
        var holdingRegErrors = calcErrors
            .GroupBy(x => x.OrganisationId)
            .Where(x => !x.Any(y => string.IsNullOrEmpty(y.SubsidiaryId)))
            .Select(x => new ProducerCalculationError
            {
                OrganisationId = x.Key,
                SubsidiaryId = null,
                ErrorCode = ProducerErrorCodes.Empty,
                LeaverCode = ProducerErrorCodes.Empty,
                IsWarning = false
            })
            .ToImmutableList();

        // Warnings are kept in calculation (they still get POM data), so they're excluded from the
        // unmatched set - only hard errors exclude an org/subsidiary from alignment.
        var unmatchedKeys = calcErrors
            .Where(e => !e.IsWarning)
            .Select(e => (e.OrganisationId, e.SubsidiaryId))
            .ToHashSet();

        return new ProducerErrorDetectionResult
        {
            Errors = calcErrors.Concat(holdingRegErrors).ToImmutableList(),
            UnmatchedKeys = unmatchedKeys
        };
    }

    public static IReadOnlyList<ProducerCalculationError> HandleMissingRegistrationData(
        IReadOnlyCollection<AlignmentPom> poms,
        IReadOnlyCollection<AlignmentOrganisation> organisations)
    {
        return poms
            .DistinctBy(x => (x.OrganisationId, x.SubsidiaryId, x.SubmitterId))
            .GroupBy(x => x.OrganisationId)
            .SelectMany(group =>
            {
                var reg = organisations.Where(o => o.OrganisationId == group.Key);
                var missing = group.Any(p => !reg.Any(o => o.SubsidiaryId == p.SubsidiaryId && o.SubmitterId == p.SubmitterId));

                return missing
                    ? group.Select(p => CreateError(p.OrganisationId ?? 0, p.SubsidiaryId, ProducerErrorCodes.MissingRegistrationData, null, isWarning: false))
                    : [];
            })
            .ToList();
    }

    public static IReadOnlyList<ProducerCalculationError> HandleMissingPomData(
        IReadOnlyCollection<AlignmentPom> poms,
        IReadOnlyCollection<AlignmentOrganisation> organisations)
    {
        // Pre-compute the set of POM keys (subsidiary id, falling back to org id) so the membership
        // check below is O(1) per organisation rather than O(P) per organisation.
        var pomKeys = new HashSet<string>(poms.Count, StringComparer.Ordinal);
        foreach (var p in poms)
        {
            var key = p.SubsidiaryId ?? p.OrganisationId.ToString()!;
            pomKeys.Add(key);
        }

        return organisations
            .Where(o => o.ObligationStatus == ObligatedStatus)
            // Only raise errors for missing POM when they previously had POM data submitted to avoid loads of errors
            .Where(o => pomKeys.Contains(o.SubsidiaryId ?? o.OrganisationId.ToString()))
            .Where(o => o is not { HasH1: true, HasH2: true })
            .Select(o => CreateError(o.OrganisationId, o.SubsidiaryId, ProducerErrorCodes.MissingPOMData, o.StatusCode, isWarning: false))
            .ToList();
    }

    public static IReadOnlyList<ProducerCalculationError> HandleObligatedErrors(
        IReadOnlyCollection<AlignmentPom> poms,
        IReadOnlyCollection<AlignmentOrganisation> organisations,
        IReadOnlyCollection<int> invoicedOrganisationIds)
    {
        return organisations
            .Where(x => x.ObligationStatus == ErrorStatus)
            .Where(o => HasPomOrWasInvoiced(o, poms, invoicedOrganisationIds))
            .Select(x => CreateError(x.OrganisationId, x.SubsidiaryId, x.ErrorCode, x.StatusCode, isWarning: false))
            .ToList();
    }

    public static IReadOnlyList<ProducerCalculationError> HandleObligatedWarnings(
        IReadOnlyCollection<AlignmentPom> poms,
        IReadOnlyCollection<AlignmentOrganisation> organisations,
        IReadOnlyCollection<int> invoicedOrganisationIds)
    {
        return organisations
            .Where(x => x.ObligationStatus == ObligatedStatus && !string.IsNullOrEmpty(x.ErrorCode))
            .Where(o => HasPomOrWasInvoiced(o, poms, invoicedOrganisationIds))
            .Select(x => CreateError(x.OrganisationId, x.SubsidiaryId, x.ErrorCode, x.StatusCode, isWarning: true))
            .ToList();
    }

    private static bool HasPomOrWasInvoiced(
        AlignmentOrganisation o,
        IReadOnlyCollection<AlignmentPom> poms,
        IReadOnlyCollection<int> invoicedOrganisationIds) =>
        poms.Any(p => new { OrgId = p.OrganisationId, p.SubsidiaryId, p.SubmitterId }.Equals(new { OrgId = (int?)o.OrganisationId, o.SubsidiaryId, o.SubmitterId }))
        || invoicedOrganisationIds.Contains(o.OrganisationId);

    private static ProducerCalculationError CreateError(int orgId, string? subId, string? errorCode, string? leaverCode, bool isWarning) =>
        new()
        {
            OrganisationId = orgId,
            SubsidiaryId = subId,
            ErrorCode = errorCode ?? string.Empty,
            LeaverCode = leaverCode ?? string.Empty,
            IsWarning = isWarning
        };
}
