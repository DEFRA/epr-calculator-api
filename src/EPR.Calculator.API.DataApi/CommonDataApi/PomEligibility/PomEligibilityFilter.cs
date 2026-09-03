using System.Globalization;
using EPR.CommonDataService.DataApi.CommonDataApi.Entities;

namespace EPR.CommonDataService.DataApi.CommonDataApi.PomEligibility;

/// <summary>
///     Filters raw POM rows down to those eligible for alignment, ported from
///     dbo.sp_GetPaycalPomData.sql's LatestAcceptedPomsWith2Period and Latest_Org_Data_Selection CTEs.
///     Operates on the whole set of POMs (and organisations) for a run at once, since eligibility depends
///     on cross-row aggregation (whether an organisation/submitter/period has submitted both halves of the
///     year) and on the separately-streamed organisation data (whether a registration exists at all).
/// </summary>
public interface IPomEligibilityFilter
{
    /// <summary>
    ///     Returns only the POMs whose organisation/submitter/submission-period-year has submitted both
    ///     H1 and H2, and whose organisation has an accepted registration (present in
    ///     <paramref name="organisationIdsWithRegistration" />).
    /// </summary>
    IReadOnlyList<PayCalPom> Filter(IReadOnlyList<PayCalPom> poms, IReadOnlyCollection<int> organisationIdsWithRegistration);
}

public sealed class PomEligibilityFilter : IPomEligibilityFilter
{
    public IReadOnlyList<PayCalPom> Filter(IReadOnlyList<PayCalPom> poms, IReadOnlyCollection<int> organisationIdsWithRegistration)
    {
        var registeredOrganisationIds = organisationIdsWithRegistration as HashSet<int> ?? organisationIdsWithRegistration.ToHashSet();

        var eligiblePeriods = poms
            .Where(p => p.OrganisationId is not null && TryParseYear(p.SubmissionPeriod, out _))
            .GroupBy(p => (p.OrganisationId!.Value, p.SubmitterId, Year: ParseYear(p.SubmissionPeriod!)))
            .Where(g => g.Any(p => IsH1(p.SubmissionPeriod!, g.Key.Year)) && g.Any(p => IsH2(p.SubmissionPeriod!, g.Key.Year)))
            .Select(g => g.Key)
            .ToHashSet();

        return poms
            .Where(p =>
                p.OrganisationId is not null &&
                registeredOrganisationIds.Contains(p.OrganisationId.Value) &&
                TryParseYear(p.SubmissionPeriod, out var year) &&
                eligiblePeriods.Contains((p.OrganisationId.Value, p.SubmitterId, year)))
            .ToList();
    }

    private static bool TryParseYear(string? submissionPeriod, out int year)
    {
        year = 0;
        return submissionPeriod is { Length: >= 4 } &&
               int.TryParse(submissionPeriod.AsSpan(0, 4), NumberStyles.Integer, CultureInfo.InvariantCulture, out year);
    }

    private static int ParseYear(string submissionPeriod)
    {
        TryParseYear(submissionPeriod, out var year);
        return year;
    }

    private static bool IsH1(string submissionPeriod, int year) =>
        (year > 2024 && submissionPeriod.EndsWith("-H1", StringComparison.Ordinal)) ||
        submissionPeriod is "2024-P1" or "2024-P2" or "2024-P3";

    private static bool IsH2(string submissionPeriod, int year) =>
        (year > 2024 && submissionPeriod.EndsWith("-H2", StringComparison.Ordinal)) ||
        submissionPeriod == "2024-P4";
}
