using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;
using EPR.Calculator.Api.DataApi.ObligationDetermination;

namespace EPR.Calculator.Api.DataApi.PomEligibility;

/// <summary>
///     Computes each organisation/subsidiary's HasH1/HasH2 flags (whether it submitted a POM for each
///     half of the prior year), ported from dbo.sp_GetPaycalOrgData.sql's organisation_period_flags CTE.
///     Unlike the run-wide H1+H2 gate in <see cref="IPomEligibilityFilter" /> (which groups by
///     organisation/submitter only), this groups by organisation/subsidiary/submitter, matching the
///     per-subsidiary flags previously computed in SQL.
/// </summary>
internal interface IOrganisationPeriodFlagsCalculator
{
    /// <summary>
    ///     Returns the given organisations with HasH1/HasH2 set from the POM stream. Row identity and
    ///     count are preserved 1:1.
    /// </summary>
    IReadOnlyList<FlaggedOrganisation> ApplyPeriodFlags(IReadOnlyList<DeterminedOrganisation> organisations, IReadOnlyList<PayCalPom> poms);
}

internal sealed class OrganisationPeriodFlagsCalculator : IOrganisationPeriodFlagsCalculator
{
    public IReadOnlyList<FlaggedOrganisation> ApplyPeriodFlags(IReadOnlyList<DeterminedOrganisation> organisations, IReadOnlyList<PayCalPom> poms) =>
        DataApiTelemetry.Trace(typeof(OrganisationPeriodFlagsCalculator), nameof(ApplyPeriodFlags), () =>
        {
            // Parse each POM's submission period once, not once per IsH1/IsH2 check - this runs over
            // every POM row for the run.
            var flagsByOrgSubSubmitter = poms
                .Select(p => (Pom: p, Year: ParseYear(p.SubmissionPeriod)))
                .Where(x => x.Year is not null)
                .GroupBy(x => (x.Pom.OrganisationId, x.Pom.SubsidiaryId, x.Pom.SubmitterId))
                .ToDictionary(
                    g => g.Key,
                    g => (
                        HasH1: g.Any(x => SubmissionPeriodClassification.IsH1(x.Pom.SubmissionPeriod!, x.Year!.Value)),
                        HasH2: g.Any(x => SubmissionPeriodClassification.IsH2(x.Pom.SubmissionPeriod!, x.Year!.Value))));

            return organisations
                .Select(o =>
                {
                    var flags = flagsByOrgSubSubmitter.GetValueOrDefault((o.Org.OrganisationId, o.Org.SubsidiaryId, o.Org.SubmitterId));

                    return new FlaggedOrganisation
                    {
                        Org = o.Org,
                        ObligationStatus = o.ObligationStatus,
                        NumDaysObligated = o.NumDaysObligated,
                        ErrorCode = o.ErrorCode,
                        HasH1 = flags.HasH1,
                        HasH2 = flags.HasH2
                    };
                })
                .ToList();
        });

    private static int? ParseYear(string? submissionPeriod) =>
        SubmissionPeriodClassification.TryParseYear(submissionPeriod, out var year) ? year : null;
}
