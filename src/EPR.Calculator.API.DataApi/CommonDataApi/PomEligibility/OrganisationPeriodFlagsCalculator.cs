using EPR.CommonDataService.DataApi.CommonDataApi.Entities;

namespace EPR.CommonDataService.DataApi.CommonDataApi.PomEligibility;

/// <summary>
///     Computes each organisation/subsidiary's HasH1/HasH2 flags (whether it submitted a POM for each
///     half of the prior year), ported from dbo.sp_GetPaycalOrgData.sql's organisation_period_flags CTE.
///     Unlike the run-wide H1+H2 gate in <see cref="IPomEligibilityFilter" /> (which groups by
///     organisation/submitter only), this groups by organisation/subsidiary/submitter, matching the
///     per-subsidiary flags previously computed in SQL.
/// </summary>
public interface IOrganisationPeriodFlagsCalculator
{
    /// <summary>
    ///     Returns the given organisations with HasH1/HasH2 set from the POM stream. Row identity and
    ///     count are preserved 1:1.
    /// </summary>
    IReadOnlyList<PayCalOrganisation> ApplyPeriodFlags(IReadOnlyList<PayCalOrganisation> organisations, IReadOnlyList<PayCalPom> poms);
}

public sealed class OrganisationPeriodFlagsCalculator : IOrganisationPeriodFlagsCalculator
{
    public IReadOnlyList<PayCalOrganisation> ApplyPeriodFlags(IReadOnlyList<PayCalOrganisation> organisations, IReadOnlyList<PayCalPom> poms)
    {
        var flagsByOrgSubSubmitter = poms
            .Where(p => p.OrganisationId is not null && SubmissionPeriodClassification.TryParseYear(p.SubmissionPeriod, out _))
            .GroupBy(p => (p.OrganisationId!.Value, p.SubsidiaryId, p.SubmitterId))
            .ToDictionary(
                g => g.Key,
                g => (
                    HasH1: g.Any(p => IsH1(p.SubmissionPeriod!)),
                    HasH2: g.Any(p => IsH2(p.SubmissionPeriod!))));

        return organisations
            .Select(o =>
            {
                if (o.OrganisationId is null ||
                    !flagsByOrgSubSubmitter.TryGetValue((o.OrganisationId.Value, o.SubsidiaryId, o.SubmitterId), out var flags))
                {
                    return o with { HasH1 = false, HasH2 = false };
                }

                return o with { HasH1 = flags.HasH1, HasH2 = flags.HasH2 };
            })
            .ToList();
    }

    private static bool IsH1(string submissionPeriod)
    {
        SubmissionPeriodClassification.TryParseYear(submissionPeriod, out var year);
        return SubmissionPeriodClassification.IsH1(submissionPeriod, year);
    }

    private static bool IsH2(string submissionPeriod)
    {
        SubmissionPeriodClassification.TryParseYear(submissionPeriod, out var year);
        return SubmissionPeriodClassification.IsH2(submissionPeriod, year);
    }
}
