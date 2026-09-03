namespace EPR.CommonDataService.DataApi.Alignment;

public sealed record ProducerErrorDetectionResult
{
    /// <summary>
    ///     Every error/warning row. For a row with <see cref="ProducerCalculationError.HasPomMatch" />
    ///     false, the caller decides whether it's still worth surfacing (e.g. because the organisation
    ///     was invoiced in a previous run) - DataApi has no visibility into billing history.
    /// </summary>
    public required IReadOnlyList<ProducerCalculationError> Errors { get; init; }

    /// <summary>
    ///     Org/subsidiary keys with a hard (non-warning) error - these should be excluded from
    ///     downstream alignment, regardless of whether the caller ultimately chooses to display them.
    /// </summary>
    public required IReadOnlySet<(int OrganisationId, string? SubsidiaryId)> UnmatchedKeys { get; init; }
}

public interface IProducerErrorDetector
{
    /// <summary>
    ///     Runs every error/warning rule against the (pre-dedup) organisation and POM populations.
    ///     Doesn't decide whether a no-POM-match error/warning should be shown - that depends on billing
    ///     history DataApi doesn't have, so it's surfaced via <see cref="ProducerCalculationError.HasPomMatch" />
    ///     for the caller to decide. For the same reason, holding-company roll-ups aren't computed here
    ///     either - they depend on which of a producer's errors the caller keeps.
    /// </summary>
    /// <param name="organisations">The full, non-deduped organisation population for the run.</param>
    /// <param name="poms">The full POM population for the run.</param>
    ProducerErrorDetectionResult Detect(
        IReadOnlyCollection<AlignmentOrganisation> organisations,
        IReadOnlyCollection<AlignmentPom> poms);
}

public sealed class ProducerErrorDetector : IProducerErrorDetector
{
    private const string ObligatedStatus = "O";
    private const string ErrorStatus = "E";

    public ProducerErrorDetectionResult Detect(
        IReadOnlyCollection<AlignmentOrganisation> organisations,
        IReadOnlyCollection<AlignmentPom> poms)
    {
        var obligatedErrors = HandleObligatedErrors(poms, organisations);
        var missingRegErrors = HandleMissingRegistrationData(poms, organisations);
        var obligatedWarnings = HandleObligatedWarnings(poms, organisations);
        var missingPomErrors = HandleMissingPomData(poms, organisations);

        var calcErrors = obligatedErrors
            .Concat(missingRegErrors)
            .Concat(obligatedWarnings)
            .Concat(missingPomErrors)
            .ToImmutableList();

        // A hard error always excludes its org/subsidiary from alignment, regardless of HasPomMatch -
        // an "E"-status organisation's POM data should never enter the calculation. Warnings are kept
        // in calculation (they still get POM data), so they're excluded from the unmatched set.
        var unmatchedKeys = calcErrors
            .Where(e => !e.IsWarning)
            .Select(e => (e.OrganisationId, e.SubsidiaryId))
            .ToHashSet();

        return new ProducerErrorDetectionResult
        {
            Errors = calcErrors,
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

                // Always POM-driven by definition - there's no invoiced-history question here.
                return missing
                    ? group.Select(p => CreateError(p.OrganisationId ?? 0, p.SubsidiaryId, ProducerErrorCodes.MissingRegistrationData, null, isWarning: false, hasPomMatch: true))
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
            // Always POM-driven by definition (matched via pomKeys above).
            .Select(o => CreateError(o.OrganisationId, o.SubsidiaryId, ProducerErrorCodes.MissingPOMData, o.StatusCode, isWarning: false, hasPomMatch: true))
            .ToList();
    }

    public static IReadOnlyList<ProducerCalculationError> HandleObligatedErrors(
        IReadOnlyCollection<AlignmentPom> poms,
        IReadOnlyCollection<AlignmentOrganisation> organisations)
    {
        return organisations
            .Where(x => x.ObligationStatus == ErrorStatus)
            .Select(x => CreateError(x.OrganisationId, x.SubsidiaryId, x.ErrorCode, x.StatusCode, isWarning: false, hasPomMatch: HasPomMatch(x, poms)))
            .ToList();
    }

    public static IReadOnlyList<ProducerCalculationError> HandleObligatedWarnings(
        IReadOnlyCollection<AlignmentPom> poms,
        IReadOnlyCollection<AlignmentOrganisation> organisations)
    {
        return organisations
            .Where(x => x.ObligationStatus == ObligatedStatus && !string.IsNullOrEmpty(x.ErrorCode))
            .Select(x => CreateError(x.OrganisationId, x.SubsidiaryId, x.ErrorCode, x.StatusCode, isWarning: true, hasPomMatch: HasPomMatch(x, poms)))
            .ToList();
    }

    private static bool HasPomMatch(AlignmentOrganisation o, IReadOnlyCollection<AlignmentPom> poms) =>
        poms.Any(p => new { OrgId = p.OrganisationId, p.SubsidiaryId, p.SubmitterId }.Equals(new { OrgId = (int?)o.OrganisationId, o.SubsidiaryId, o.SubmitterId }));

    private static ProducerCalculationError CreateError(int orgId, string? subId, string? errorCode, string? leaverCode, bool isWarning, bool hasPomMatch) =>
        new()
        {
            OrganisationId = orgId,
            SubsidiaryId = subId,
            ErrorCode = errorCode ?? string.Empty,
            LeaverCode = leaverCode ?? string.Empty,
            IsWarning = isWarning,
            HasPomMatch = hasPomMatch
        };
}
