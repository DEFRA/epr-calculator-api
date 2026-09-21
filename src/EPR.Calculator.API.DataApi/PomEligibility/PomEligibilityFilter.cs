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

            var eligiblePeriods = poms
                .Where(p => SubmissionPeriodClassification.TryParseYear(p.SubmissionPeriod, out _))
                .GroupBy(p => (p.OrganisationId, p.SubmitterId, Year: ParseYear(p.SubmissionPeriod!)))
                .Where(g =>
                    g.Any(p => SubmissionPeriodClassification.IsH1(p.SubmissionPeriod!, g.Key.Year)) &&
                    g.Any(p => SubmissionPeriodClassification.IsH2(p.SubmissionPeriod!, g.Key.Year)))
                .Select(g => g.Key)
                .ToHashSet();

            return poms
                .Where(p =>
                    registeredOrganisationIds.Contains(p.OrganisationId) &&
                    SubmissionPeriodClassification.TryParseYear(p.SubmissionPeriod, out var year) &&
                    eligiblePeriods.Contains((p.OrganisationId, p.SubmitterId, year)))
                .ToList();
        });

    private static int ParseYear(string submissionPeriod)
    {
        SubmissionPeriodClassification.TryParseYear(submissionPeriod, out var year);
        return year;
    }
}
