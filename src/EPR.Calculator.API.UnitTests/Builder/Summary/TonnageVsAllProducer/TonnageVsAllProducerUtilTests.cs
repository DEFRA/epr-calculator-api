namespace EPR.Calculator.API.UnitTests.Builder.Summary.TonnageVsAllProducer
{
  using AutoFixture;
  using EPR.Calculator.API.Builder.Summary;
  using EPR.Calculator.API.Builder.Summary.TonnageVsAllProducer;
  using EPR.Calculator.API.Data.DataModels;
  using EPR.Calculator.API.Models;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using System.Collections.Generic;

  [TestClass]
  public class TonnageVsAllProducerUtilTests
  {
    private Fixture Fixture { get; init; } = new Fixture();

    [TestMethod]
    public void CanCallGetPercentageofProducerReportedHHTonnagevsAllProducersTotal()
    {
      // Arrange
      var producers = Fixture.Create<List<ProducerDetail>>();

      var testProducerId = Fixture.Create<int>();
      var testCalculatorRunId = Fixture.Create<int>();
      var testSubsidaryId = Fixture.Create<string>();
      var testMaterialId = Fixture.Create<int>();
      var materialDetails = Fixture.Create<List<MaterialDetail>>();

      var allResults = GenerateAllResults(testProducerId, testCalculatorRunId, testSubsidaryId);

      producers.First().ProducerId = testProducerId;
      producers.First().SubsidiaryId = testSubsidaryId;
      producers.First().CalculatorRunId = testCalculatorRunId;
      allResults.First().ProducerReportedMaterial.MaterialId = testMaterialId;
      materialDetails.First().Id = testMaterialId;

      var TotalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(allResults, materialDetails, producers.First().CalculatorRunId);
      // Act
      var result = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducersTotal(producers, TotalPackagingTonnage);

      // Assert
      Assert.AreEqual(50, result);
    }

    /// <summary>
    /// Check that the percentage of HH tonnage returns zero if there is no producer details corresponding
    /// to the ID given in the materials details.
    /// </summary>
    [TestMethod]
    public void GetPercentageofProducerReportedHHTonnagevsAllProducersTotal_ReturnsZeroWhenNoMatchingProducer()
    {
      // Arrange
      var fixture = new Fixture();
      var producers = fixture.Create<List<ProducerDetail>>();
      var allResults = fixture.Create<List<CalcResultsProducerAndReportMaterialDetail>>();
      var materialDetails = Fixture.Create<List<MaterialDetail>>();

      var testProducerId = fixture.Create<int>();
      var testCalculatorRunId = fixture.Create<int>();

      allResults.First().ProducerReportedMaterial.ProducerDetailId = testProducerId;
      allResults.First().ProducerDetail.Id = testProducerId;
      allResults.First().ProducerDetail.ProducerId = testProducerId;
      allResults.First().ProducerDetail.CalculatorRunId = testCalculatorRunId;
      allResults.First().ProducerReportedMaterial.PackagingType = "HH";

      var TotalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(allResults, materialDetails, testCalculatorRunId);

      // Act
      var result = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducersTotal(producers, TotalPackagingTonnage);

      // Assert
      Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void CanCallGetPercentageofProducerReportedHHTonnagevsAllProducers()
    {
      // Arrange
      var testProducerId = Fixture.Create<int>();
      var testCalculatorRunId = Fixture.Create<int>();
      var testSubsidaryId = Fixture.Create<string>();
      var materialDetails = Fixture.Create<List<MaterialDetail>>();
      var testMaterialId = Fixture.Create<int>();

      var producer = Fixture.Create<ProducerDetail>();
      var allResults = GenerateAllResults(testProducerId, testCalculatorRunId, testSubsidaryId);

      producer.ProducerId = testProducerId;
      producer.SubsidiaryId = testSubsidaryId;
      producer.CalculatorRunId = testCalculatorRunId;
      allResults.First().ProducerReportedMaterial.MaterialId = testMaterialId;
      materialDetails.First().Id = testMaterialId;

      var TotalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(allResults, materialDetails, testCalculatorRunId);

      // Act
      var result = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(
          producer,
          TotalPackagingTonnage);

      // Assert
      Assert.AreEqual(50, result);
    }

    [TestMethod]
    public void GetPercentageofProducerReportedTonnagevsAllProducersTotal_ReturnsZero_WhenNoMatchingProducer()
    {
      // Arrange
      var fixture = new Fixture();
      var producers = fixture.Create<List<ProducerDetail>>();
      var allResults = fixture.Create<List<CalcResultsProducerAndReportMaterialDetail>>();
      var materialDetails = fixture.Create<List<MaterialDetail>>();

      var testProducerId = fixture.Create<int>();
      var testCalculatorRunId = fixture.Create<int>();

      allResults.First().ProducerReportedMaterial.ProducerDetailId = testProducerId;
      allResults.First().ProducerDetail.Id = testProducerId;
      allResults.First().ProducerDetail.ProducerId = testProducerId;
      allResults.First().ProducerDetail.CalculatorRunId = testCalculatorRunId;
      allResults.First().ProducerReportedMaterial.PackagingType = "PB";

      var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(allResults, materialDetails, testCalculatorRunId);

      // Act
      var result = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducersTotal(producers, totalPackagingTonnage);

      // Assert
      Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void GetPercentageofProducerReportedTonnagevsAllProducersTotal_ReturnsValue_WhenMatchingProducer()
    {
      // Arrange
      var testProducerId = Fixture.Create<int>();
      var testCalculatorRunId = Fixture.Create<int>();
      var testMaterialId = Fixture.Create<int>();
      var testSubsidaryId = Fixture.Create<string>();
      var materialDetails = Fixture.Create<List<MaterialDetail>>();

      var producer = Fixture.Create<ProducerDetail>();
      var allResults = GenerateAllResults(testProducerId, testCalculatorRunId, testSubsidaryId);

      allResults.First().ProducerReportedMaterial.MaterialId = testMaterialId;
      allResults.First().ProducerReportedMaterial.PackagingType = "PB";
      materialDetails.First().Id = testMaterialId;

      producer.ProducerId = testProducerId;
      producer.SubsidiaryId = testSubsidaryId;
      producer.CalculatorRunId = testCalculatorRunId;

      var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(allResults, materialDetails, testCalculatorRunId);

      // Act
      var result = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(
        producer,
        totalPackagingTonnage);

      // Assert
      Assert.AreEqual(50, result);
    }

    private List<CalcResultsProducerAndReportMaterialDetail> GenerateAllResults(
            int testProducerId,
            int testCalculatorRunId,
            string testSubsidaryId)
    {
      var allResults = Fixture.Create<List<CalcResultsProducerAndReportMaterialDetail>>();
      allResults.First().ProducerReportedMaterial.ProducerDetailId = testProducerId;
      allResults.First().ProducerDetail.Id = testProducerId;
      allResults.First().ProducerDetail.ProducerId = testProducerId;
      allResults.First().ProducerDetail.CalculatorRunId = testCalculatorRunId;
      allResults.First().ProducerDetail.SubsidiaryId = testSubsidaryId;
      allResults.First().ProducerReportedMaterial.PackagingType = "HH";

      allResults.Last().ProducerReportedMaterial.ProducerDetailId = testProducerId;
      allResults.Last().ProducerDetail.Id = testProducerId;
      allResults.Last().ProducerDetail.ProducerId = testProducerId;
      allResults.Last().ProducerDetail.CalculatorRunId = testCalculatorRunId;
      allResults.Last().ProducerDetail.SubsidiaryId = Fixture.Create<string>();
      allResults.Last().ProducerReportedMaterial.PackagingType = "HH";

      return allResults;
    }
  }
}