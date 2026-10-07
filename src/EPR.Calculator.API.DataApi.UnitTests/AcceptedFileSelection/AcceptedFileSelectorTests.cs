using EPR.Calculator.Api.DataApi.AcceptedFileSelection;
using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;

namespace EPR.Calculator.API.DataApi.UnitTests.AcceptedFileSelection;

/// <summary>
///     Validates <see cref="AcceptedFileSelector" /> against the winning-file scenarios from epr-data's
///     test_paycal_orgdata_sql.py / test_paycal_pomdata_sql.py, which exercise exactly this cut-off/
///     resubmission fallback rule. Only the Granted/Accepted scenarios are ported: Pending is excluded by
///     a SQL-side status filter upstream of this class entirely, and Cancelled reaches this selector but
///     wins in exactly the same way an Accepted file would - regulator status plays no part in picking a
///     winner here, it only affects the obligation status computed afterwards by
///     ProducerObligationDeterminer (see its own tests) - so a Cancelled case would just duplicate an
///     existing Accepted one.
/// </summary>
[TestClass]
public class AcceptedFileSelectorTests
{
    private static readonly DateTimeOffset CutOffDate = new(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTime T0 = new(2025, 1, 1);
    private static readonly DateTime T1 = new(2025, 2, 1);
    private static readonly DateTime T2 = new(2025, 3, 1);
    private static readonly DateTime After = new(2025, 9, 1);

    private readonly AcceptedFileSelector selector = new();

    // Ported from epr-data's _REG_CASES (test_paycal_orgdata_sql.py), Granted/Accepted scenarios only -
    // see the class summary for why Pending/Cancelled aren't duplicated here.
    private static IEnumerable<object[]> OrganisationScenarios()
    {
        (string CaseId, (string Marker, DateTime Created, bool IsResubmission)[] Files, string ExpectedWinner)[] cases =
        [
            ("01_initial_before", [("Initial", T0, false)], "Initial"),
            ("02_initial_after", [("Initial", After, false)], "Initial"),
            ("05_resub_before", [("Initial", T0, false), ("Resub", T1, true)], "Resub"),
            ("06_resub_after", [("Initial", T0, false), ("Resub", After, true)], "Initial"),
            ("09_resub2_before", [("Initial", T0, false), ("Resub1", T1, true), ("Resub2", T2, true)], "Resub2"),
            ("10_resub2_after", [("Initial", T0, false), ("Resub1", T1, true), ("Resub2", After, true)], "Resub1"),
        ];

        return cases.Select(c => new object[] { c.CaseId, c.Files, c.ExpectedWinner });
    }

    [TestMethod]
    [DynamicData(nameof(OrganisationScenarios))]
    public void SelectLatestOrganisationFiles_MatchesReferenceScenario(
        string caseId,
        (string Marker, DateTime Created, bool IsResubmission)[] files,
        string expectedWinner)
    {
        var organisations = files
            .Select(f => new PayCalOrganisation
            {
                OrganisationId = 1,
                SubmitterId = "SUBMITTER-1",
                SubmissionPeriodYear = 2025,
                RegulatorStatus = "Accepted",
                OrganisationName = f.Marker,
                FileName = f.Marker,
                CreatedDateTime = f.Created,
                IsResubmission = f.IsResubmission
            })
            .ToList();

        var result = selector.SelectLatestOrganisationFiles(organisations, CutOffDate);

        result.Select(o => o.OrganisationName).ShouldBe([expectedWinner], caseId);
    }

    [TestMethod]
    public void SelectLatestOrganisationFiles_WithNoEligibleCandidate_ExcludesGroup()
    {
        // Only candidate is a resubmission created after the cut-off - no fallback available.
        var organisations = new[]
        {
            new PayCalOrganisation
            {
                OrganisationId = 1,
                OrganisationName = "Org Co",
                SubmitterId = "SUBMITTER-1",
                SubmissionPeriodYear = 2025,
                RegulatorStatus = "Accepted",
                FileName = "Resub",
                CreatedDateTime = After,
                IsResubmission = true
            }
        };

        var result = selector.SelectLatestOrganisationFiles(organisations, CutOffDate);

        result.ShouldBeEmpty();
    }

    [TestMethod]
    public void SelectLatestOrganisationFiles_WithNullCutOffDate_NoFilesExcludedByCutOff()
    {
        var organisations = new[]
        {
            new PayCalOrganisation
            {
                OrganisationId = 1,
                SubmitterId = "SUBMITTER-1",
                SubmissionPeriodYear = 2025,
                RegulatorStatus = "Accepted",
                OrganisationName = "Initial",
                FileName = "Initial",
                CreatedDateTime = T0,
                IsResubmission = false
            },
            new PayCalOrganisation
            {
                OrganisationId = 1,
                SubmitterId = "SUBMITTER-1",
                SubmissionPeriodYear = 2025,
                RegulatorStatus = "Accepted",
                OrganisationName = "Resub",
                FileName = "Resub",
                CreatedDateTime = After,
                IsResubmission = true
            }
        };

        var result = selector.SelectLatestOrganisationFiles(organisations, cutOffDate: null);

        result.Select(o => o.OrganisationName).ShouldBe(["Resub"]);
    }

    [TestMethod]
    public void SelectLatestOrganisationFiles_GroupsSeparatelyByOrganisationSubmitterAndYear()
    {
        var organisations = new[]
        {
            new PayCalOrganisation { OrganisationName = "Org Co", OrganisationId = 1, SubmitterId = "A", SubmissionPeriodYear = 2025, RegulatorStatus = "Accepted", FileName = "F1", CreatedDateTime = T0 },
            new PayCalOrganisation { OrganisationName = "Org Co", OrganisationId = 2, SubmitterId = "A", SubmissionPeriodYear = 2025, RegulatorStatus = "Accepted", FileName = "F2", CreatedDateTime = T0 },
            new PayCalOrganisation { OrganisationName = "Org Co", OrganisationId = 1, SubmitterId = "B", SubmissionPeriodYear = 2025, RegulatorStatus = "Accepted", FileName = "F3", CreatedDateTime = T0 },
            new PayCalOrganisation { OrganisationName = "Org Co", OrganisationId = 1, SubmitterId = "A", SubmissionPeriodYear = 2026, RegulatorStatus = "Accepted", FileName = "F4", CreatedDateTime = T0 }
        };

        var result = selector.SelectLatestOrganisationFiles(organisations, cutOffDate: null);

        result.Count.ShouldBe(4);
    }

    // Ported from epr-data's _POM_CASES (test_paycal_pomdata_sql.py) "Accepted" scenarios - the only ones
    // relevant here, since non-Accepted POMs never reach this selector (SQL still filters
    // Regulator_Status = 'Accepted' before streaming).
    private static IEnumerable<object[]> PomScenarios()
    {
        (string CaseId, (string Marker, DateTime Created, bool IsResubmission)[] Files, string ExpectedWinner)[] cases =
        [
            ("01_initial_before", [("INIT", T0, false)], "INIT"),
            ("02_initial_after", [("INIT", After, false)], "INIT"),
            ("05_resub_before", [("INIT", T0, false), ("RESUB", T1, true)], "RESUB"),
            ("06_resub_after", [("INIT", T0, false), ("RESUB", After, true)], "INIT"),
            ("09_resub2_before", [("INIT", T0, false), ("RESUB1", T1, true), ("RESUB2", T2, true)], "RESUB2"),
            ("10_resub2_after", [("INIT", T0, false), ("RESUB1", T1, true), ("RESUB2", After, true)], "RESUB1"),
        ];

        return cases.Select(c => new object[] { c.CaseId, c.Files, c.ExpectedWinner });
    }

    [TestMethod]
    [DynamicData(nameof(PomScenarios))]
    public void SelectLatestPomFiles_MatchesReferenceScenario(
        string caseId,
        (string Marker, DateTime Created, bool IsResubmission)[] files,
        string expectedWinner)
    {
        var poms = files
            .Select(f => new PayCalPom
            {
                OrganisationId = 1,
                SubmitterId = "SUBMITTER-1",
                SubmissionPeriod = "2025-H1",
                PackagingMaterialSubtype = f.Marker,
                FileName = f.Marker,
                CreatedDateTime = f.Created,
                IsResubmission = f.IsResubmission
            })
            .ToList();

        var result = selector.SelectLatestPomFiles(poms, CutOffDate);

        result.Select(p => p.PackagingMaterialSubtype).ShouldBe([expectedWinner], caseId);
    }

    [TestMethod]
    public void SelectLatestPomFiles_WinningFileKeepsAllOfItsLineItems()
    {
        var poms = new[]
        {
            new PayCalPom { OrganisationId = 1, SubmitterId = "SUBMITTER-1", SubmissionPeriod = "2025-H1", FileName = "F1", PackagingMaterial = "PL", CreatedDateTime = T0 },
            new PayCalPom { OrganisationId = 1, SubmitterId = "SUBMITTER-1", SubmissionPeriod = "2025-H1", FileName = "F1", PackagingMaterial = "GL", CreatedDateTime = T0 },
            new PayCalPom { OrganisationId = 1, SubmitterId = "SUBMITTER-1", SubmissionPeriod = "2025-H2", FileName = "F2", PackagingMaterial = "PL", CreatedDateTime = T0 }
        };

        var result = selector.SelectLatestPomFiles(poms, cutOffDate: null);

        result.Count.ShouldBe(3);
    }

    [TestMethod]
    public void SelectLatestPomFiles_ResubmissionOfOneQuarterSupersedesAnotherQuarterInTheSameHalf()
    {
        // 2024-P1/P2/P3 all count towards H1, so a resubmitted 2024-P3 file competes with - and here
        // beats - an earlier, non-resubmitted 2024-P1 file for the same organisation/submitter.
        var poms = new[]
        {
            new PayCalPom { OrganisationId = 1, SubmitterId = "SUBMITTER-1", SubmissionPeriod = "2024-P1", FileName = "P1", CreatedDateTime = T0 },
            new PayCalPom { OrganisationId = 1, SubmitterId = "SUBMITTER-1", SubmissionPeriod = "2024-P3", FileName = "P3-RESUB", IsResubmission = true, CreatedDateTime = T1 },
            new PayCalPom { OrganisationId = 1, SubmitterId = "SUBMITTER-1", SubmissionPeriod = "2024-P4", FileName = "P4", CreatedDateTime = T0 }
        };

        var result = selector.SelectLatestPomFiles(poms, cutOffDate: null);

        result.Select(p => p.FileName).ShouldBe(["P3-RESUB", "P4"]);
    }

    [TestMethod]
    [DynamicData(nameof(PomScenarios))]
    public void SelectWinningPomFileNames_PicksSameWinnerAsSelectLatestPomFiles(
        string caseId,
        (string Marker, DateTime Created, bool IsResubmission)[] files,
        string expectedWinner)
    {
        var candidates = files.Select(f => new PomFileCandidate(
            OrganisationId: 1,
            SubmitterId: "SUBMITTER-1",
            SubmissionPeriod: "2025-H1",
            FileName: f.Marker,
            IsResubmission: f.IsResubmission,
            CreatedDateTime: f.Created));

        var winners = selector.SelectWinningPomFileNames(candidates, CutOffDate);

        winners[(1, "SUBMITTER-1", "2025-H1")].ShouldBe(expectedWinner, caseId);
    }

    [TestMethod]
    public void SelectWinningPomFileNames_BothQuartersInAHalfResolveToTheSameWinner()
    {
        var candidates = new[]
        {
            new PomFileCandidate(1, "SUBMITTER-1", "2024-P1", "P1", IsResubmission: false, CreatedDateTime: T0),
            new PomFileCandidate(1, "SUBMITTER-1", "2024-P3", "P3-RESUB", IsResubmission: true, CreatedDateTime: T1)
        };

        var winners = selector.SelectWinningPomFileNames(candidates, cutOffDate: null);

        winners[(1, "SUBMITTER-1", "2024-P1")].ShouldBe("P3-RESUB");
        winners[(1, "SUBMITTER-1", "2024-P3")].ShouldBe("P3-RESUB");
    }

    [TestMethod]
    public void SelectWinningPomFileNames_ExcludesGroupWithNoEligibleCandidate()
    {
        var candidates = new[]
        {
            new PomFileCandidate(1, "SUBMITTER-1", "2025-H1", "Resub", IsResubmission: true, CreatedDateTime: After)
        };

        var winners = selector.SelectWinningPomFileNames(candidates, CutOffDate);

        winners.ShouldBeEmpty();
    }
}
