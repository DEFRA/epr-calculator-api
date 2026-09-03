using EPR.CommonDataService.DataApi.CommonDataApi.Entities;
using EPR.CommonDataService.DataApi.CommonDataApi.PomEligibility;

namespace EPR.Calculator.API.DataApi.UnitTests.CommonDataApi.PomEligibility;

[TestClass]
public class PomEligibilityFilterTests
{
    private readonly PomEligibilityFilter filter = new();

    private static PayCalPom Pom(int organisationId = 1, string? subsidiaryId = "SUB-1", string submitterId = "SUBMITTER-1", string submissionPeriod = "2024-P1") =>
        new()
        {
            OrganisationId = organisationId,
            SubsidiaryId = subsidiaryId,
            SubmitterId = submitterId,
            SubmissionPeriod = submissionPeriod
        };

    [TestMethod]
    public void Filter_WithBothHalvesAndRegistration_KeepsAllPoms()
    {
        var poms = new[] { Pom(submissionPeriod: "2024-P1"), Pom(submissionPeriod: "2024-P4") };

        var result = filter.Filter(poms, [1]);

        result.Count.ShouldBe(2);
    }

    [TestMethod]
    public void Filter_WithOnlyOneHalfSubmitted_DropsAllPomsForThatGroup()
    {
        var poms = new[] { Pom(submissionPeriod: "2024-P1") };

        var result = filter.Filter(poms, [1]);

        result.ShouldBeEmpty();
    }

    [TestMethod]
    public void Filter_WithNoMatchingRegistration_DropsPoms()
    {
        var poms = new[] { Pom(submissionPeriod: "2024-P1"), Pom(submissionPeriod: "2024-P4") };

        var result = filter.Filter(poms, []);

        result.ShouldBeEmpty();
    }

    [TestMethod]
    public void Filter_GateIsPerOrganisation_IgnoringSubsidiary()
    {
        // Sub-1 alone only ever submits H1; Sub-2 alone only ever submits H2. Since the gate groups by
        // (OrganisationId, SubmitterId) only - not subsidiary - the organisation as a whole satisfies
        // the H1+H2 requirement, so poms for BOTH subsidiaries survive.
        var poms = new[]
        {
            Pom(subsidiaryId: "SUB-1", submissionPeriod: "2024-P1"),
            Pom(subsidiaryId: "SUB-2", submissionPeriod: "2024-P4")
        };

        var result = filter.Filter(poms, [1]);

        result.Count.ShouldBe(2);
    }

    [TestMethod]
    public void Filter_GroupsSeparatelyByOrganisation()
    {
        var poms = new[]
        {
            Pom(organisationId: 1, submissionPeriod: "2024-P1"),
            Pom(organisationId: 1, submissionPeriod: "2024-P4"),
            Pom(organisationId: 2, submissionPeriod: "2024-P1") // org 2 never gets H2 - excluded
        };

        var result = filter.Filter(poms, [1, 2]);

        result.Count.ShouldBe(2);
        result.ShouldAllBe(p => p.OrganisationId == 1);
    }

    [TestMethod]
    public void Filter_GroupsSeparatelyBySubmitter()
    {
        var poms = new[]
        {
            Pom(submitterId: "SUBMITTER-1", submissionPeriod: "2024-P1"),
            Pom(submitterId: "SUBMITTER-1", submissionPeriod: "2024-P4"),
            Pom(submitterId: "SUBMITTER-2", submissionPeriod: "2024-P1") // different submitter, no H2 - excluded
        };

        var result = filter.Filter(poms, [1]);

        result.Count.ShouldBe(2);
        result.ShouldAllBe(p => p.SubmitterId == "SUBMITTER-1");
    }

    [TestMethod]
    public void Filter_WithMissingOrganisationId_ExcludesPom()
    {
        var poms = new[] { Pom() with { OrganisationId = null } };

        var result = filter.Filter(poms, [1]);

        result.ShouldBeEmpty();
    }

    [TestMethod]
    public void Filter_WithUnparsableSubmissionPeriod_ExcludesPom()
    {
        var poms = new[] { Pom(submissionPeriod: "not-a-period") };

        var result = filter.Filter(poms, [1]);

        result.ShouldBeEmpty();
    }
}
