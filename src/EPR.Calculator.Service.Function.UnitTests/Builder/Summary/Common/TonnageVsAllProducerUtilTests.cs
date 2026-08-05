using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.Common;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class TonnageVsAllProducerUtilTests
{
    private IFixture Fixture { get; } = TestFixtures.New();


    [TestMethod]
    public void CanCallGetPercentageofProducerReportedHHTonnagevsAllProducers()
    {
        // Arrange
        var testProducerId = Fixture.Create<int>();
        var testCalculatorRunId = Fixture.Create<int>();
        var testSubsidaryId = Fixture.Create<string>();
        var materialDetails = Fixture.Create<List<MaterialDetail>>();

        //CalcResultSummaryBuilder.ScaledupProducers = Fixture.Create<List<CalcResultScaledupProducer>>();

        var producer = Fixture.Create<ProducerDetail>();
        var allResults = GenerateAllResults(testProducerId, testCalculatorRunId, testSubsidaryId);

        producer.ProducerId = testProducerId;
        producer.SubsidiaryId = testSubsidaryId;
        producer.CalculatorRunId = testCalculatorRunId;
        allResults.First().ProducerMaterialPackaging.MaterialId = materialDetails.First().Id;

        var TotalPackagingTonnage = ProducerFeesBuilder.GetTotalPackagingTonnagePerRun(allResults, materialDetails, testCalculatorRunId);

        // Act
        var result = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(
            producer,
            TotalPackagingTonnage);

        // Assert
        Assert.AreEqual(50, result);
    }

    [TestMethod]
    public void GetPercentageofProducerReportedTonnagevsAllProducersTotal_ReturnsValue_WhenMatchingProducer()
    {
        // Arrange
        var testProducerId = Fixture.Create<int>();
        var testCalculatorRunId = Fixture.Create<int>();
        var testSubsidaryId = Fixture.Create<string>();
        var materialDetails = Fixture.Create<List<MaterialDetail>>();

        var producer = Fixture.Create<ProducerDetail>();
        var allResults = GenerateAllResults(testProducerId, testCalculatorRunId, testSubsidaryId);

        allResults.First().ProducerMaterialPackaging.MaterialId = materialDetails.First().Id;
        allResults.First().ProducerMaterialPackaging.PackagingType = "PB";

        producer.ProducerId = testProducerId;
        producer.SubsidiaryId = testSubsidaryId;
        producer.CalculatorRunId = testCalculatorRunId;

        //CalcResultSummaryBuilder.ScaledupProducers = Fixture.Create<List<CalcResultScaledupProducer>>();

        var totalPackagingTonnage = ProducerFeesBuilder.GetTotalPackagingTonnagePerRun(allResults, materialDetails, testCalculatorRunId);

        // Act
        var result = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(
            producer,
            totalPackagingTonnage);

        // Assert
        Assert.AreEqual(50, result);
    }

    private List<CalcResultProducerAndReportMaterialDetail> GenerateAllResults(
        int testProducerId,
        int testCalculatorRunId,
        string testSubsidaryId)
    {
        var allResults = Fixture.Create<List<CalcResultProducerAndReportMaterialDetail>>();
        allResults.First().ProducerMaterialPackaging.ProducerDetailId = testProducerId;
        allResults.First().ProducerDetail.Id = testProducerId;
        allResults.First().ProducerDetail.ProducerId = testProducerId;
        allResults.First().ProducerDetail.CalculatorRunId = testCalculatorRunId;
        allResults.First().ProducerDetail.SubsidiaryId = testSubsidaryId;
        allResults.First().ProducerMaterialPackaging.PackagingType = "HH";

        allResults.Last().ProducerMaterialPackaging.ProducerDetailId = testProducerId;
        allResults.Last().ProducerDetail.Id = testProducerId;
        allResults.Last().ProducerDetail.ProducerId = testProducerId;
        allResults.Last().ProducerDetail.CalculatorRunId = testCalculatorRunId;
        allResults.Last().ProducerDetail.SubsidiaryId = Fixture.Create<string>();
        allResults.Last().ProducerMaterialPackaging.PackagingType = "HH";

        return allResults;
    }
}
