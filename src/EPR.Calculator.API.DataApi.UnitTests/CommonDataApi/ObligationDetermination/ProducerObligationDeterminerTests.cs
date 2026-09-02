using EPR.CommonDataService.DataApi.CommonDataApi.Entities;

namespace EPR.CommonDataService.DataApi.CommonDataApi.ObligationDetermination.UnitTests;

/// <summary>
///     Validates <see cref="ProducerObligationDeterminer" /> against the shared business-rule scenarios
///     from epr-data's epr-obligation-determination test suite (see
///     <see cref="ObligationTestCaseLoader" />), the same CSV used to validate the SQL/PySpark
///     reference implementations this C# port replaces.
/// </summary>
[TestClass]
public class ProducerObligationDeterminerTests
{
    private static readonly string CsvPath = Path.Combine(AppContext.BaseDirectory, "TestData", "myc-obligation-determination-test-cases.csv");

    private static IEnumerable<object[]> Scenarios() =>
        ObligationTestCaseLoader.Load(CsvPath).Select(c => new object[] { c });

    [TestMethod]
    [DynamicData(nameof(Scenarios))]
    public void Determine_MatchesCsvScenario(ObligationTestCaseLoader.TestCase testCase)
    {
        // Arrange
        var determiner = new ProducerObligationDeterminer();

        var input = testCase.Records
            .Select(r => new PayCalOrganisation
            {
                OrganisationId = r.OrganisationId,
                SubsidiaryId = r.SubsidiaryId,
                SubmitterId = r.ComplianceScheme ?? $"EXT{r.OrganisationId}",
                OrganisationName = r.OrganisationName,
                TradingName = r.OrganisationName,
                StatusCode = r.StatusCode,
                JoinerDate = r.JoinerDate,
                RegulatorStatus = "Accepted",
                SubmissionPeriodYear = r.SubmissionPeriodYear
            })
            .ToList();

        // Act
        var results = determiner.Determine(input);

        // Assert
        foreach (var expected in testCase.Records)
        {
            var context = $"{testCase.SubTitle}: org {expected.OrganisationId}/{expected.SubsidiaryId ?? "-"}";
            var expectedSubmitterId = expected.ComplianceScheme ?? $"EXT{expected.OrganisationId}";
            var actual = results.Single(o =>
                o.OrganisationId == expected.OrganisationId &&
                o.SubsidiaryId == expected.SubsidiaryId &&
                o.SubmitterId == expectedSubmitterId &&
                o.SubmissionPeriodYear == expected.SubmissionPeriodYear);

            actual.ObligationStatus.ShouldBe(expected.ExpectedObligationStatus, context);
            actual.NumDaysObligated.ShouldBe(expected.ExpectedNumDaysObligated, context);
            actual.ErrorCode.ShouldBe(expected.ExpectedErrorCode, context);
        }
    }

    // ─────────────────────────── Cancelled registrations (not covered by the CSV) ───────────────────────────

    /// <summary>
    ///     A single Cancelled registration is deliberately kept (not filtered out upstream) and treated as
    ///     a raw Not-Obligated status, per epr-data's test_paycal_orgdata_sql.py registration-selection
    ///     tests. With no sibling registration for the same producer/period, this lands on the same
    ///     "solo not-obligated" branch as scenario 19.5 in the CSV (a lone Not-Obligated registration is
    ///     an error, not silently Not Obligated).
    /// </summary>
    [TestMethod]
    public void Determine_WhenRegulatorStatusCancelledAlone_IsError()
    {
        // Arrange
        var determiner = new ProducerObligationDeterminer();
        var input = new List<PayCalOrganisation>
        {
            new()
            {
                OrganisationId = 900001,
                OrganisationName = "Cancelled Co",
                SubmitterId = "EXT900001",
                StatusCode = null,
                RegulatorStatus = "Cancelled",
                SubmissionPeriodYear = 2024
            }
        };

        // Act
        var results = determiner.Determine(input);

        // Assert
        results.Single().ObligationStatus.ShouldBe("E");
        results.Single().ErrorCode.ShouldBe("Not Obligated");
    }

    /// <summary>
    ///     A Cancelled registration alongside a genuine Obligated one for the same producer/period is a
    ///     Not-Obligated + Obligated combination (pivot: Obligated=1, Blank=0) - the single obligated
    ///     registration wins outright, with the cancelled row demoted to Not Obligated.
    /// </summary>
    [TestMethod]
    public void Determine_WhenCancelledAlongsideObligated_ObligatedWins()
    {
        // Arrange
        var determiner = new ProducerObligationDeterminer();
        var input = new List<PayCalOrganisation>
        {
            new()
            {
                OrganisationId = 900002,
                SubsidiaryId = "900003",
                OrganisationName = "Old Registration",
                SubmitterId = "EXT900002",
                StatusCode = "01",
                RegulatorStatus = "Cancelled",
                SubmissionPeriodYear = 2024
            },
            new()
            {
                OrganisationId = 900004,
                SubsidiaryId = "900003",
                OrganisationName = "New Registration",
                SubmitterId = "EXT900004",
                StatusCode = "01",
                RegulatorStatus = "Accepted",
                SubmissionPeriodYear = 2024
            }
        };

        // Act
        var results = determiner.Determine(input);

        // Assert
        results.Single(o => o.OrganisationId == 900002).ObligationStatus.ShouldBe("N");
        results.Single(o => o.OrganisationId == 900004).ObligationStatus.ShouldBe("O");
    }
}
