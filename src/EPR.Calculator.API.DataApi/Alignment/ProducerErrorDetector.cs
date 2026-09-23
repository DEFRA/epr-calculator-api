using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;
using EPR.Calculator.Api.DataApi.Models;
using EPR.Calculator.Api.DataApi.PomEligibility;

namespace EPR.Calculator.Api.DataApi.Alignment;

/// <summary>
///     A <see cref="ProducerCalculationError" /> keyed by the org/subsidiary it was raised against - the
///     detector's flat output, folded into <see cref="ProducerRecord" />s by the caller.
/// </summary>
internal sealed record OrganisationCalculationError
{
    public required int OrganisationId { get; init; }
    public string? SubsidiaryId { get; init; }
    public required ProducerCalculationError Error { get; init; }
}

internal sealed record ProducerErrorDetectionResult
{
    /// <summary>
    ///     Every error/warning row. For a row with <see cref="ProducerCalculationError.HasPomMatch" />
    ///     false, the caller decides whether it's still worth surfacing (e.g. because the organisation
    ///     was invoiced in a previous run) - DataApi has no visibility into billing history.
    /// </summary>
    public required IReadOnlyList<OrganisationCalculationError> Errors { get; init; }

    /// <summary>
    ///     Org/subsidiary keys with a hard (non-warning) error - these should be excluded from
    ///     downstream alignment, regardless of whether the caller ultimately chooses to display them.
    /// </summary>
    public required IReadOnlySet<(int OrganisationId, string? SubsidiaryId)> UnmatchedKeys { get; init; }
}

internal interface IProducerErrorDetector
{
    /// <summary>
    ///     Runs every error/warning rule against the (pre-dedupe) organisation and POM populations.
    ///     Doesn't decide whether a no-POM-match error/warning should be shown - that depends on billing
    ///     history DataApi doesn't have, so it's surfaced via <see cref="ProducerCalculationError.HasPomMatch" />
    ///     for the caller to decide. For the same reason, holding-company roll-ups aren't computed here
    ///     either - they depend on which of a producer's errors the caller keeps.
    /// </summary>
    /// <param name="organisations">The full, non-deduped organisation population for the run.</param>
    /// <param name="poms">The full POM population for the run.</param>
    ProducerErrorDetectionResult Detect(
        IReadOnlyCollection<FlaggedOrganisation> organisations,
        IReadOnlyCollection<PayCalPom> poms);
}

internal sealed class ProducerErrorDetector : IProducerErrorDetector
{
    private const string ObligatedStatus = "O";
    private const string ErrorStatus = "E";

    public ProducerErrorDetectionResult Detect(
        IReadOnlyCollection<FlaggedOrganisation> organisations,
        IReadOnlyCollection<PayCalPom> poms)
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
            .Where(e => !e.Error.IsWarning)
            .Select(e => (e.OrganisationId, e.SubsidiaryId))
            .ToHashSet();

        return new ProducerErrorDetectionResult
        {
            Errors = calcErrors,
            UnmatchedKeys = unmatchedKeys
        };
    }

    public static IReadOnlyList<OrganisationCalculationError> HandleMissingRegistrationData(
        IReadOnlyCollection<PayCalPom> poms,
        IReadOnlyCollection<FlaggedOrganisation> organisations)
    {
        ArgumentNullException.ThrowIfNull(poms);
        ArgumentNullException.ThrowIfNull(organisations);

        var registrationKeys = organisations
            .Select(o => ((int?)o.Org.OrganisationId, o.Org.SubsidiaryId, o.Org.SubmitterId))
            .ToHashSet();

        return poms
            .DistinctBy(x => (x.OrganisationId, x.SubsidiaryId, x.SubmitterId))
            .GroupBy(x => x.OrganisationId)
            .SelectMany(group =>
            {
                var missing = group.Any(p => !registrationKeys.Contains((p.OrganisationId, p.SubsidiaryId, p.SubmitterId)));

                // Always POM-driven by definition - there's no invoiced-history question here.
                return missing
                    ? group.Select(p => CreateError(p.OrganisationId, p.SubsidiaryId, "Missing Registration Data", null, isWarning: false, hasPomMatch: true))
                    : [];
            })
            .ToList();
    }

    public static IReadOnlyList<OrganisationCalculationError> HandleMissingPomData(
        IReadOnlyCollection<PayCalPom> poms,
        IReadOnlyCollection<FlaggedOrganisation> organisations)
    {
        // Pre-compute the set of POM keys (subsidiary id, falling back to org id) so the membership
        // check below is O(1) per organisation rather than O(P) per organisation.
        var pomKeys = new HashSet<string>(poms.Count, StringComparer.Ordinal);
        foreach (var p in poms)
        {
            var key = p.SubsidiaryId ?? p.OrganisationId.ToString();
            pomKeys.Add(key);
        }

        return organisations
            .Where(o => o.ObligationStatus == ObligatedStatus)
            // Only raise errors for missing POM when they previously had POM data submitted to avoid loads of errors
            .Where(o => pomKeys.Contains(o.Org.SubsidiaryId ?? o.Org.OrganisationId.ToString()))
            .Where(o => o is not { HasH1: true, HasH2: true })
            // Always POM-driven by definition (matched via pomKeys above).
            .Select(o => CreateError(o.Org.OrganisationId, o.Org.SubsidiaryId, "Missing POM Data", o.Org.StatusCode, isWarning: false, hasPomMatch: true))
            .ToList();
    }

    public static IReadOnlyList<OrganisationCalculationError> HandleObligatedErrors(
        IReadOnlyCollection<PayCalPom> poms,
        IReadOnlyCollection<FlaggedOrganisation> organisations)
    {
        var pomKeys = BuildPomKeys(poms);

        return organisations
            .Where(x => x.ObligationStatus == ErrorStatus)
            .Select(x => CreateError(x.Org.OrganisationId, x.Org.SubsidiaryId, x.ErrorCode, x.Org.StatusCode, isWarning: false,
                hasPomMatch: pomKeys.Contains((x.Org.OrganisationId, x.Org.SubsidiaryId, x.Org.SubmitterId))))
            .ToList();
    }

    public static IReadOnlyList<OrganisationCalculationError> HandleObligatedWarnings(
        IReadOnlyCollection<PayCalPom> poms,
        IReadOnlyCollection<FlaggedOrganisation> organisations)
    {
        var pomKeys = BuildPomKeys(poms);

        return organisations
            .Where(x => x.ObligationStatus == ObligatedStatus && !string.IsNullOrEmpty(x.ErrorCode))
            .Select(x => CreateError(x.Org.OrganisationId, x.Org.SubsidiaryId, x.ErrorCode, x.Org.StatusCode, isWarning: true,
                hasPomMatch: pomKeys.Contains((x.Org.OrganisationId, x.Org.SubsidiaryId, x.Org.SubmitterId))))
            .ToList();
    }

    private static HashSet<(int OrganisationId, string? SubsidiaryId, string? SubmitterId)> BuildPomKeys(
        IReadOnlyCollection<PayCalPom> poms) =>
        poms.Select(p => (p.OrganisationId, p.SubsidiaryId, p.SubmitterId)).ToHashSet();

    private static OrganisationCalculationError CreateError(int orgId, string? subId, string? errorCode, string? leaverCode, bool isWarning, bool hasPomMatch) =>
        new()
        {
            OrganisationId = orgId,
            SubsidiaryId = subId,
            Error = new ProducerCalculationError
            {
                ErrorCode = errorCode ?? string.Empty,
                LeaverCode = leaverCode ?? string.Empty,
                IsWarning = isWarning,
                HasPomMatch = hasPomMatch
            }
        };
}
