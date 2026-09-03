using EPR.CommonDataService.DataApi.AcceptedFileSelection;
using EPR.CommonDataService.DataApi.CommonDataApi.Entities;

namespace EPR.Calculator.API.DataApi.UnitTests.AcceptedFileSelection;

/// <summary>
///     Validates <see cref="AcceptedFileSelector" /> against the winning-file scenarios from epr-data's
///     test_paycal_orgdata_sql.py / test_paycal_pomdata_sql.py, which exercise exactly this cut-off/
///     resubmission fallback rule. Only the scenarios where the candidate file's regulator status would
///     have survived the (unchanged) SQL status filter are ported here - a file rejected by regulator
///     status (e.g. Pending registrations, non-Accepted POMs) never reaches this selector, so those
///     scenarios remain covered by the still-SQL-side status filtering, not by this class.
/// </summary>
[TestClass]
public class AcceptedFileSelectorTests
{
    private static readonly DateTimeOffset CutOffDate = new(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T0 = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T1 = new(2025, 2, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T2 = new(2025, 3, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset After = new(2025, 9, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly AcceptedFileSelector selector = new();

    // Ported from epr-data's _REG_CASES (test_paycal_orgdata_sql.py), restricted to the Granted/Accepted/
    // Cancelled scenarios - the file-selection rule is identical for all three since regulator-status
    // filtering already happened upstream in SQL by the time rows reach this selector.
    private static IEnumerable<object[]> OrganisationScenarios()
    {
        (string CaseId, (string Marker, DateTimeOffset Created, bool IsResubmission)[] Files, string ExpectedWinner)[] cases =
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
        (string Marker, DateTimeOffset Created, bool IsResubmission)[] files,
        string expectedWinner)
    {
        var organisations = files
            .Select(f => new PayCalOrganisation
            {
                OrganisationId = 1,
                SubmitterId = "SUBMITTER-1",
                SubmissionPeriodYear = 2025,
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
                SubmitterId = "SUBMITTER-1",
                SubmissionPeriodYear = 2025,
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
            new PayCalOrganisation { OrganisationId = 1, SubmitterId = "A", SubmissionPeriodYear = 2025, FileName = "F1", CreatedDateTime = T0 },
            new PayCalOrganisation { OrganisationId = 2, SubmitterId = "A", SubmissionPeriodYear = 2025, FileName = "F2", CreatedDateTime = T0 },
            new PayCalOrganisation { OrganisationId = 1, SubmitterId = "B", SubmissionPeriodYear = 2025, FileName = "F3", CreatedDateTime = T0 },
            new PayCalOrganisation { OrganisationId = 1, SubmitterId = "A", SubmissionPeriodYear = 2026, FileName = "F4", CreatedDateTime = T0 }
        };

        var result = selector.SelectLatestOrganisationFiles(organisations, cutOffDate: null);

        result.Count.ShouldBe(4);
    }

    // Ported from epr-data's _POM_CASES (test_paycal_pomdata_sql.py) "Accepted" scenarios - the only ones
    // relevant here, since non-Accepted POMs never reach this selector (SQL still filters
    // Regulator_Status = 'Accepted' before streaming).
    private static IEnumerable<object[]> PomScenarios()
    {
        (string CaseId, (string Marker, DateTimeOffset Created, bool IsResubmission)[] Files, string ExpectedWinner)[] cases =
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
        (string Marker, DateTimeOffset Created, bool IsResubmission)[] files,
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
}
