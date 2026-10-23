using EPR.Calculator.Api.DataApi;
using EPR.Calculator.Api.DataApi.AcceptedFileSelection;
using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.DataApi.UnitTests.AcceptedFileSelection;

/// <summary>
///     Validates <see cref="AcceptedFileSelector" /> against the winning-file scenarios from epr-data's
///     test_paycal_orgdata_sql.py / test_paycal_pomdata_sql.py, which exercise exactly this cut-off/
///     resubmission fallback rule, plus the regulator status rules.
/// </summary>
[TestClass]
public class AcceptedFileSelectorTests
{
    // Relative year 2026: registrations are for 2026 and POMs for 2025, and late submissions are those
    // created after 31 December 2026.
    private const int RelativeYear = 2026;

    private static readonly DateTimeOffset CutOffDate = new(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTime T0 = new(2025, 1, 1);
    private static readonly DateTime T1 = new(2025, 2, 1);
    private static readonly DateTime T2 = new(2025, 3, 1);
    private static readonly DateTime After = new(2025, 9, 1);
    private static readonly DateTime DuringRelativeYear = new(2026, 6, 1);
    private static readonly DateTime Late = new(2027, 2, 1);

    private readonly AcceptedFileSelector selector = CreateSelector();

    private static AcceptedFileSelector CreateSelector(int lateSubmissionsAfterMonth = 12, ILogger<AcceptedFileSelector>? logger = null) =>
        new(Options.Create(new CalculatorDataApiOptions { LateSubmissionsAfterMonth = lateSubmissionsAfterMonth }),
            logger ?? NullLogger<AcceptedFileSelector>.Instance);

    // Ported from epr-data's _REG_CASES (test_paycal_orgdata_sql.py), Granted/Accepted scenarios.
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
            .Select(f => Organisation(f.Marker, f.Created, f.IsResubmission, "Accepted"))
            .ToList();

        var result = selector.SelectLatestOrganisationFiles(organisations, RelativeYear, CutOffDate);

        result.Select(o => o.OrganisationName).ShouldBe([expectedWinner], caseId);
    }

    [TestMethod]
    public void SelectLatestOrganisationFiles_WithNoEligibleCandidate_ExcludesGroup()
    {
        // Only candidate is a resubmission created after the cut-off - no fallback available.
        var organisations = new[] { Organisation("Resub", After, isResubmission: true, "Accepted") };

        var result = selector.SelectLatestOrganisationFiles(organisations, RelativeYear, CutOffDate);

        result.ShouldBeEmpty();
    }

    [TestMethod]
    public void SelectLatestOrganisationFiles_WithNullCutOffDate_NoFilesExcludedByCutOff()
    {
        var organisations = new[]
        {
            Organisation("Initial", T0, isResubmission: false, "Accepted"),
            Organisation("Resub", After, isResubmission: true, "Accepted")
        };

        var result = selector.SelectLatestOrganisationFiles(organisations, RelativeYear, cutOffDate: null);

        result.Select(o => o.OrganisationName).ShouldBe(["Resub"]);
    }

    [TestMethod]
    public void SelectLatestOrganisationFiles_GroupsSeparatelyByOrganisationSubmitterAndYear()
    {
        var organisations = new[]
        {
            new PayCalOrganisation { OrganisationName = "Org Co", OrganisationId = 1, SubmitterId = "A", SubmissionPeriodYear = 2025, RegulatorStatus = "Accepted", FileName = "F1", CreatedDateTime = T0, OrganisationSize = "L" },
            new PayCalOrganisation { OrganisationName = "Org Co", OrganisationId = 2, SubmitterId = "A", SubmissionPeriodYear = 2025, RegulatorStatus = "Accepted", FileName = "F2", CreatedDateTime = T0, OrganisationSize = "L" },
            new PayCalOrganisation { OrganisationName = "Org Co", OrganisationId = 1, SubmitterId = "B", SubmissionPeriodYear = 2025, RegulatorStatus = "Accepted", FileName = "F3", CreatedDateTime = T0, OrganisationSize = "L" },
            new PayCalOrganisation { OrganisationName = "Org Co", OrganisationId = 1, SubmitterId = "A", SubmissionPeriodYear = 2026, RegulatorStatus = "Accepted", FileName = "F4", CreatedDateTime = T0, OrganisationSize = "L" }
        };

        var result = selector.SelectLatestOrganisationFiles(organisations, RelativeYear, cutOffDate: null);

        result.Count.ShouldBe(4);
    }

    // Ported from epr-data's _POM_CASES (test_paycal_pomdata_sql.py) "Accepted" scenarios.
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
            .Select(f => Pom(f.Marker, f.Created, f.IsResubmission, "Accepted"))
            .ToList();

        var result = selector.SelectLatestPomFiles(poms, RelativeYear, CutOffDate);

        result.Select(p => p.FileName).ShouldBe([expectedWinner], caseId);
    }

    [TestMethod]
    public void SelectLatestPomFiles_WinningFileKeepsAllOfItsLineItems()
    {
        var poms = new[]
        {
            new PayCalPom { OrganisationId = 1, SubmitterId = "SUBMITTER-1", SubmissionPeriod = "2025-H1", FileName = "F1", PackagingMaterial = "PL", RegulatorStatus = "Accepted", CreatedDateTime = T0 },
            new PayCalPom { OrganisationId = 1, SubmitterId = "SUBMITTER-1", SubmissionPeriod = "2025-H1", FileName = "F1", PackagingMaterial = "GL", RegulatorStatus = "Accepted", CreatedDateTime = T0 },
            new PayCalPom { OrganisationId = 1, SubmitterId = "SUBMITTER-1", SubmissionPeriod = "2025-H2", FileName = "F2", PackagingMaterial = "PL", RegulatorStatus = "Accepted", CreatedDateTime = T0 }
        };

        var result = selector.SelectLatestPomFiles(poms, RelativeYear, cutOffDate: null);

        result.Count.ShouldBe(3);
    }

    [TestMethod]
    public void SelectLatestPomFiles_ResubmissionOfOneQuarterSupersedesAnotherQuarterInTheSameHalf()
    {
        // 2024-P1/P2/P3 all count towards H1, so a resubmitted 2024-P3 file competes with - and here
        // beats - an earlier, non-resubmitted 2024-P1 file for the same organisation/submitter.
        var poms = new[]
        {
            new PayCalPom { OrganisationId = 1, SubmitterId = "SUBMITTER-1", SubmissionPeriod = "2024-P1", FileName = "P1", RegulatorStatus = "Accepted", CreatedDateTime = T0 },
            new PayCalPom { OrganisationId = 1, SubmitterId = "SUBMITTER-1", SubmissionPeriod = "2024-P3", FileName = "P3-RESUB", IsResubmission = true, RegulatorStatus = "Accepted", CreatedDateTime = T1 },
            new PayCalPom { OrganisationId = 1, SubmitterId = "SUBMITTER-1", SubmissionPeriod = "2024-P4", FileName = "P4", RegulatorStatus = "Accepted", CreatedDateTime = T0 }
        };

        var result = selector.SelectLatestPomFiles(poms, RelativeYear, cutOffDate: null);

        result.Select(p => p.FileName).ShouldBe(["P3-RESUB", "P4"]);
    }

    [TestMethod]
    [DynamicData(nameof(PomScenarios))]
    public void SelectWinningPomFileNames_PicksSameWinnerAsSelectLatestPomFiles(
        string caseId,
        (string Marker, DateTime Created, bool IsResubmission)[] files,
        string expectedWinner)
    {
        var candidates = files.Select(f => Candidate(f.Marker, f.Created, f.IsResubmission, "Accepted"));

        var winners = selector.SelectWinningPomFileNames(candidates, RelativeYear, CutOffDate);

        winners[(1, "SUBMITTER-1", "2025-P1")].ShouldBe(expectedWinner, caseId);
    }

    [TestMethod]
    public void SelectWinningPomFileNames_BothQuartersInAHalfResolveToTheSameWinner()
    {
        var candidates = new[]
        {
            new PomFileCandidate(1, "SUBMITTER-1", "2024-P1", "P1", IsResubmission: false, RegulatorStatus: "Accepted", CreatedDateTime: T0),
            new PomFileCandidate(1, "SUBMITTER-1", "2024-P3", "P3-RESUB", IsResubmission: true, RegulatorStatus: "Accepted", CreatedDateTime: T1)
        };

        var winners = selector.SelectWinningPomFileNames(candidates, RelativeYear, cutOffDate: null);

        winners[(1, "SUBMITTER-1", "2024-P1")].ShouldBe("P3-RESUB");
        winners[(1, "SUBMITTER-1", "2024-P3")].ShouldBe("P3-RESUB");
    }

    [TestMethod]
    public void SelectWinningPomFileNames_ExcludesGroupWithNoEligibleCandidate()
    {
        var candidates = new[] { Candidate("Resub", After, isResubmission: true, "Accepted") };

        var winners = selector.SelectWinningPomFileNames(candidates, RelativeYear, CutOffDate);

        winners.ShouldBeEmpty();
    }

    // A Pending file only counts when it's an original created after 31 December 2025.
    private static IEnumerable<object?[]> StatusScenarios()
    {
        (string CaseId, (string Marker, DateTime Created, bool IsResubmission, string? Status)[] Files, string? ExpectedWinner)[] cases =
        [
            ("pending_initial_late", [("INIT", Late, false, "Pending")], "INIT"),
            ("pending_initial_during_relative_year", [("INIT", DuringRelativeYear, false, "Pending")], null),
            ("pending_initial_during_year", [("INIT", T0, false, "Pending")], null),
            ("pending_resub_late_falls_back", [("INIT", T0, false, "Accepted"), ("RESUB", Late, true, "Pending")], "INIT"),
            ("pending_resub_during_year_falls_back", [("INIT", T0, false, "Accepted"), ("RESUB", T1, true, "Pending")], "INIT"),
            ("pending_initial_during_year_then_accepted_resub", [("INIT", T0, false, "Pending"), ("RESUB", T1, true, "Accepted")], "RESUB"),
            ("accepted_initial_late", [("INIT", Late, false, "Accepted")], "INIT"),
            ("granted_initial", [("INIT", T0, false, "Granted")], "INIT"),
            ("cancelled_resub_selected", [("INIT", T0, false, "Accepted"), ("RESUB", T1, true, "Cancelled")], "RESUB"),
            ("rejected_resub_falls_back", [("INIT", T0, false, "Accepted"), ("RESUB", T1, true, "Rejected")], "INIT"),
            ("refused_initial_excluded", [("INIT", Late, false, "Refused")], null),
            ("queried_initial_excluded", [("INIT", T0, false, "Queried")], null),
            ("queried_resub_falls_back", [("INIT", T0, false, "Accepted"), ("RESUB", T1, true, "Queried")], "INIT"),
            ("uploaded_resub_falls_back", [("INIT", T0, false, "Accepted"), ("RESUB", T1, true, "Uploaded")], "INIT"),
            ("missing_status_falls_back", [("INIT", T0, false, "Accepted"), ("RESUB", T1, true, null)], "INIT"),
            ("unrecognised_status_falls_back", [("INIT", T0, false, "Accepted"), ("RESUB", T1, true, "Withdrawn")], "INIT"),
        ];

        return cases.Select(c => new object?[] { c.CaseId, c.Files, c.ExpectedWinner });
    }

    [TestMethod]
    [DynamicData(nameof(StatusScenarios))]
    public void SelectLatestOrganisationFiles_SelectsByRegulatorStatus(
        string caseId,
        (string Marker, DateTime Created, bool IsResubmission, string? Status)[] files,
        string? expectedWinner)
    {
        var organisations = files.Select(f => Organisation(f.Marker, f.Created, f.IsResubmission, f.Status)).ToList();

        var result = selector.SelectLatestOrganisationFiles(organisations, RelativeYear, cutOffDate: null);

        result.Select(o => o.OrganisationName).ShouldBe(expectedWinner is null ? [] : [expectedWinner], caseId);
    }

    [TestMethod]
    [DynamicData(nameof(StatusScenarios))]
    public void SelectWinningPomFileNames_SelectsByRegulatorStatus(
        string caseId,
        (string Marker, DateTime Created, bool IsResubmission, string? Status)[] files,
        string? expectedWinner)
    {
        var candidates = files.Select(f => Candidate(f.Marker, f.Created, f.IsResubmission, f.Status));

        var winners = selector.SelectWinningPomFileNames(candidates, RelativeYear, cutOffDate: null);

        winners.GetValueOrDefault((1, "SUBMITTER-1", "2025-P1")).ShouldBe(expectedWinner, caseId);
    }

    [TestMethod]
    [DataRow(12, "2026-12-31T23:59:59", false)]
    [DataRow(12, "2027-01-01T00:00:00", true)]
    [DataRow(8, "2026-08-31T23:59:59", false)]
    [DataRow(8, "2026-09-01T00:00:00", true)]
    public void SelectWinningPomFileNames_LateSubmissionsStartAfterTheConfiguredMonth(int lateSubmissionsAfterMonth, string created, bool expectSelected)
    {
        var candidates = new[] { Candidate("INIT", DateTime.Parse(created, System.Globalization.CultureInfo.InvariantCulture), isResubmission: false, "Pending") };

        var winners = CreateSelector(lateSubmissionsAfterMonth).SelectWinningPomFileNames(candidates, RelativeYear, cutOffDate: null);

        winners.Count.ShouldBe(expectSelected ? 1 : 0);
    }

    [TestMethod]
    [DataRow(null, true)]
    [DataRow("Withdrawn", true)]
    [DataRow("Rejected", false)]
    [DataRow("Queried", false)]
    [DataRow("Accepted", false)]
    public void SelectWinningPomFileNames_WarnsOnlyAboutMissingOrUnrecognisedStatuses(string? status, bool expectWarning)
    {
        var logger = new Mock<ILogger<AcceptedFileSelector>>();
        var candidates = new[] { Candidate("INIT", T0, isResubmission: false, status) };

        CreateSelector(logger: logger.Object).SelectWinningPomFileNames(candidates, RelativeYear, cutOffDate: null);

        logger.Verify(
            l => l.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            expectWarning ? Times.Once() : Times.Never());
    }

    private static PayCalOrganisation Organisation(string marker, DateTime created, bool isResubmission, string? regulatorStatus) => new()
    {
        OrganisationId = 1,
        SubmitterId = "SUBMITTER-1",
        SubmissionPeriodYear = RelativeYear,
        RegulatorStatus = regulatorStatus,
        OrganisationName = marker,
        FileName = marker,
        CreatedDateTime = created,
        IsResubmission = isResubmission,
        OrganisationSize = "L"
    };

    private static PayCalPom Pom(string marker, DateTime created, bool isResubmission, string? regulatorStatus) => new()
    {
        OrganisationId = 1,
        SubmitterId = "SUBMITTER-1",
        SubmissionPeriod = "2025-H1",
        FileName = marker,
        CreatedDateTime = created,
        IsResubmission = isResubmission,
        RegulatorStatus = regulatorStatus
    };

    private static PomFileCandidate Candidate(string marker, DateTime created, bool isResubmission, string? regulatorStatus) =>
        new(1, "SUBMITTER-1", "2025-P1", marker, isResubmission, regulatorStatus, created);
}
