using EPR.CommonDataService.DataApi.CommonDataApi.Entities;
using EPR.CommonDataService.DataApi.CommonDataApi.PomEligibility;

namespace EPR.Calculator.API.DataApi.UnitTests.CommonDataApi.PomEligibility;

[TestClass]
public class OrganisationPeriodFlagsCalculatorTests
{
    private readonly OrganisationPeriodFlagsCalculator calculator = new();

    private static PayCalOrganisation Organisation(int organisationId = 1, string? subsidiaryId = "SUB-1", string submitterId = "SUBMITTER-1") =>
        new()
        {
            OrganisationId = organisationId,
            SubsidiaryId = subsidiaryId,
            SubmitterId = submitterId,
            OrganisationName = "Org Co"
        };

    private static PayCalPom Pom(int organisationId = 1, string? subsidiaryId = "SUB-1", string submitterId = "SUBMITTER-1", string submissionPeriod = "2024-P1") =>
        new()
        {
            OrganisationId = organisationId,
            SubsidiaryId = subsidiaryId,
            SubmitterId = submitterId,
            SubmissionPeriod = submissionPeriod
        };

    [TestMethod]
    public void ApplyPeriodFlags_WithBothHalvesSubmitted_SetsBothFlagsTrue()
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom(submissionPeriod: "2024-P1"), Pom(submissionPeriod: "2024-P4") };

        var result = calculator.ApplyPeriodFlags(organisations, poms);

        result[0].HasH1.ShouldBeTrue();
        result[0].HasH2.ShouldBeTrue();
    }

    [TestMethod]
    public void ApplyPeriodFlags_WithOnlyH1Submitted_SetsOnlyHasH1()
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom(submissionPeriod: "2024-P1") };

        var result = calculator.ApplyPeriodFlags(organisations, poms);

        result[0].HasH1.ShouldBeTrue();
        result[0].HasH2.ShouldBeFalse();
    }

    [TestMethod]
    public void ApplyPeriodFlags_WithNoMatchingPoms_SetsBothFlagsFalse()
    {
        var organisations = new[] { Organisation() };
        var poms = Array.Empty<PayCalPom>();

        var result = calculator.ApplyPeriodFlags(organisations, poms);

        result[0].HasH1.ShouldBeFalse();
        result[0].HasH2.ShouldBeFalse();
    }

    [TestMethod]
    public void ApplyPeriodFlags_IsPerSubsidiary_NotPerOrganisation()
    {
        // Sub-1 submits both halves; Sub-2 (same org/submitter) submits neither - each subsidiary's
        // own flags must not leak into the other's, unlike the run-wide H1+H2 eligibility gate.
        var organisations = new[]
        {
            Organisation(subsidiaryId: "SUB-1"),
            Organisation(subsidiaryId: "SUB-2")
        };
        var poms = new[]
        {
            Pom(subsidiaryId: "SUB-1", submissionPeriod: "2024-P1"),
            Pom(subsidiaryId: "SUB-1", submissionPeriod: "2024-P4")
        };

        var result = calculator.ApplyPeriodFlags(organisations, poms);

        var sub1 = result.Single(o => o.SubsidiaryId == "SUB-1");
        var sub2 = result.Single(o => o.SubsidiaryId == "SUB-2");

        sub1.HasH1.ShouldBeTrue();
        sub1.HasH2.ShouldBeTrue();
        sub2.HasH1.ShouldBeFalse();
        sub2.HasH2.ShouldBeFalse();
    }

    [TestMethod]
    public void ApplyPeriodFlags_WithDifferentSubmitter_DoesNotMatch()
    {
        var organisations = new[] { Organisation(submitterId: "SUBMITTER-1") };
        var poms = new[] { Pom(submitterId: "SUBMITTER-2", submissionPeriod: "2024-P1") };

        var result = calculator.ApplyPeriodFlags(organisations, poms);

        result[0].HasH1.ShouldBeFalse();
        result[0].HasH2.ShouldBeFalse();
    }

    [TestMethod]
    [DataRow("2024-P1", true, false)]
    [DataRow("2024-P2", true, false)]
    [DataRow("2024-P3", true, false)]
    [DataRow("2024-P4", false, true)]
    [DataRow("2025-H1", true, false)]
    [DataRow("2025-H2", false, true)]
    public void ApplyPeriodFlags_ClassifiesSubmissionPeriodsCorrectly(string submissionPeriod, bool expectedH1, bool expectedH2)
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom(submissionPeriod: submissionPeriod) };

        var result = calculator.ApplyPeriodFlags(organisations, poms);

        result[0].HasH1.ShouldBe(expectedH1);
        result[0].HasH2.ShouldBe(expectedH2);
    }

    [TestMethod]
    public void ApplyPeriodFlags_PreservesRowCountAndOtherFields()
    {
        var organisations = new[] { Organisation() with { ObligationStatus = "O", TradingName = "Trading Co" } };

        var result = calculator.ApplyPeriodFlags(organisations, []);

        result.Count.ShouldBe(1);
        result[0].ObligationStatus.ShouldBe("O");
        result[0].TradingName.ShouldBe("Trading Co");
    }
}
