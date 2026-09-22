using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;

namespace EPR.Calculator.Api.DataApi.PomEligibility;

/// <summary>
///     Filters raw POM rows down to those eligible for alignment, ported from the eligibility CTEs
///     (both-halves-submitted and matching-registration checks) that previously lived in the Paycal POM stored procedure.
///     Operates on the whole set of POMs (and organisations) for a run at once, since eligibility depends
///     on cross-row aggregation (whether an organisation/submitter/period has submitted both halves of the
///     year) and on the separately-streamed organisation data (whether a registration exists at all).
/// </summary>
internal interface IPomEligibilityFilter
{
    /// <summary>
    ///     Returns only the POMs whose organisation/submitter/submission-period-year has submitted both
    ///     H1 and H2, and whose organisation has an accepted registration (present in
    ///     <paramref name="organisationIdsWithRegistration" />).
    /// </summary>
    IReadOnlyList<PayCalPom> Filter(IReadOnlyList<PayCalPom> poms, IReadOnlyCollection<int> organisationIdsWithRegistration);
}

internal sealed class PomEligibilityFilter : IPomEligibilityFilter
{
    public IReadOnlyList<PayCalPom> Filter(IReadOnlyList<PayCalPom> poms, IReadOnlyCollection<int> organisationIdsWithRegistration) =>
        DataApiTelemetry.Trace(typeof(PomEligibilityFilter), nameof(Filter), () =>
        {
            var registeredOrganisationIds = organisationIdsWithRegistration as HashSet<int> ?? organisationIdsWithRegistration.ToHashSet();

            // Parse each POM's submission period once, not once per group-key lookup - this runs over
            // every POM row for the run.
            var eligiblePeriods = poms
                .Select(p => (Pom: p, Year: ParseYear(p.SubmissionPeriod)))
                .Where(x => x.Year is not null)
                .GroupBy(x => (x.Pom.OrganisationId, x.Pom.SubmitterId, Year: x.Year!.Value))
                .Where(g =>
                    g.Any(x => SubmissionPeriodClassification.IsH1(x.Pom.SubmissionPeriod!, g.Key.Year)) &&
                    g.Any(x => SubmissionPeriodClassification.IsH2(x.Pom.SubmissionPeriod!, g.Key.Year)))
                .Select(g => g.Key)
                .ToHashSet();

            return poms
                .Where(p =>
                    registeredOrganisationIds.Contains(p.OrganisationId) &&
                    SubmissionPeriodClassification.TryParseYear(p.SubmissionPeriod, out var year) &&
                    eligiblePeriods.Contains((p.OrganisationId, p.SubmitterId, year)))
                .ToList();
        });

    private static int? ParseYear(string? submissionPeriod) =>
        SubmissionPeriodClassification.TryParseYear(submissionPeriod, out var year) ? year : null;
}
