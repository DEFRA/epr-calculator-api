using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Services;

[TestClass]
public class ReportedProducerServiceTests : TestsFor<ReportedProducerService>
{
    [TestMethod]
    public async Task ShouldReturnProducers_ForGivenRunId()
    {
        // Arrange
        var runContext = TestDataHelper.CalculatorRun2025;
        TestDataHelper.SeedDatabaseForInitialRun(dbContext, runContext);

        // Act
        var result = await testSubject.GetProducers(runContext);

        // Assert
        result.Count.ShouldBe(3);

        var firstProducer = result.Single(p => p.OrganisationId == 1);

        var firstPd = firstProducer.Producers.Single();
        firstPd.ProducerReportedMaterials.Count.ShouldBe(6);

        var submissionPeriods = firstPd.ProducerReportedMaterials.Select(m => m.SubmissionPeriod).Distinct();
        submissionPeriods.ShouldBe(["2025-H1", "2025-H2"]);
    }
}
