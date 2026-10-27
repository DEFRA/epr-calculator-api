using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Exporter.CsvExporter
{
    [TestClass]
    public class ProducerFeesExporterTests
    {
        private ProducerFeesExporter _testClass;

        public ProducerFeesExporterTests()
        {
            _testClass = new ProducerFeesExporter();
        }

        [TestMethod]
        public async Task ProducerFeesExporter_CanCallExport()
        {
            // Arrange
            var runContext = TestDataHelper.CalculatorRun2025;
            var producerFees = new ProducerFees
            {
                CalculatorRunId = 0,
                Details = TestDataHelper.GetProducerFeesDetail(),
                Total = TestDataHelper.GetOverallTotalRow()
            };

            var materials = TestDataHelper.GetMaterialDetails();

            await using var writer = new StringWriter();

            // Act
            var calcResult = TestDataHelper.GetCalcResult();
            var scaledupIds = calcResult.CalcResultScaledupProducers.ScaledupProducers.Select(p => p.ProducerId).ToList();
            var partialIds = calcResult.CalcResultPartialObligations.PartialObligations.Select(p => (p.ProducerId, p.SubsidiaryId)).ToList();
            await _testClass.Export(runContext, producerFees.Total, materials, scaledupIds, partialIds, writer, producerFees.Details.Select(d => d.FeeDetail));

            // Assert
            Assert.IsNotNull(writer.ToString());
        }

        [TestMethod]
        public async Task ProducerFeesExporter_BillingTotalRow_MatchesTruncatingTheFullOutputAfterTheTotalsLabel()
        {
            // Arrange
            var total = TestDataHelper.GetOverallTotalRow() with { LeaverDate = CommonConstants.Totals };
            var details = TestDataHelper.GetProducerFeesDetail().Select(d => d.FeeDetail).ToList();
            var materials = TestDataHelper.GetMaterialDetails();
            var calcResult = TestDataHelper.GetCalcResult();
            var scaledupIds = calcResult.CalcResultScaledupProducers.ScaledupProducers.Select(p => p.ProducerId).ToList();
            var partialIds = calcResult.CalcResultPartialObligations.PartialObligations.Select(p => (p.ProducerId, p.SubsidiaryId)).ToList();

            await using var calculatorWriter = new StringWriter();
            await using var billingWriter = new StringWriter();

            // Act
            await _testClass.Export(TestDataHelper.CalculatorRun2025, total, materials, scaledupIds, partialIds, calculatorWriter, details);
            await _testClass.Export(TestDataHelper.BillingRun2025, total, materials, scaledupIds, partialIds, billingWriter, details);

            // Assert: the billing file used to truncate the full output just after its last "Totals" label.
            var full = calculatorWriter.ToString();
            var truncated = full[..(full.LastIndexOf(CommonConstants.Totals, StringComparison.Ordinal) + CommonConstants.Totals.Length + 2)];
            Assert.AreEqual(truncated, billingWriter.ToString());
            Assert.IsTrue(truncated.EndsWith("\"Totals\",", StringComparison.Ordinal));
        }
    }
}
