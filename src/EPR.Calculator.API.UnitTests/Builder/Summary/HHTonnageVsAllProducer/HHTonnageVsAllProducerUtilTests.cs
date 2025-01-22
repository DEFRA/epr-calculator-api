namespace EPR.Calculator.API.UnitTests.Builder.Summary.HHTonnageVsAllProducer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using AutoFixture;
    using EPR.Calculator.API.Builder.Summary;
    using EPR.Calculator.API.Builder.Summary.HHTonnageVsAllProducer;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class HHTonnageVsAllProducerUtilTests
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

            var allResults = GenerateAllResults(testProducerId, testCalculatorRunId,testSubsidaryId);

            producers.First().ProducerId = testProducerId;
            producers.First().SubsidiaryId = testSubsidaryId;
            producers.First().CalculatorRunId = testCalculatorRunId;

            var hhTotalPackagingTonnage = CalcResultSummaryBuilder.GetHhTotalPackagingTonnagePerRun(allResults, producers.First().CalculatorRunId);
            // Act
            var result = HHTonnageVsAllProducerUtil.GetPercentageofProducerReportedHHTonnagevsAllProducersTotal(producers, hhTotalPackagingTonnage);

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

            var testProducerId = fixture.Create<int>();
            var testCalculatorRunId = fixture.Create<int>();

            allResults.First().ProducerReportedMaterial.ProducerDetailId = testProducerId;
            allResults.First().ProducerDetail.Id = testProducerId;
            allResults.First().ProducerDetail.ProducerId = testProducerId;
            allResults.First().ProducerDetail.CalculatorRunId = testCalculatorRunId;
            allResults.First().ProducerReportedMaterial.PackagingType = "HH";


            // Act
            var result = HHTonnageVsAllProducerUtil.GetPercentageofProducerReportedHHTonnagevsAllProducersTotal(producers, allResults);

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

            var producer = Fixture.Create<ProducerDetail>();
            var allResults = GenerateAllResults(testProducerId, testCalculatorRunId, testSubsidaryId);

            producer.ProducerId = testProducerId;
            producer.SubsidiaryId = testSubsidaryId;
            producer.CalculatorRunId = testCalculatorRunId;

            // Act
            var result = HHTonnageVsAllProducerUtil.GetPercentageofProducerReportedHHTonnagevsAllProducers(
                producer, 
                allResults);

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