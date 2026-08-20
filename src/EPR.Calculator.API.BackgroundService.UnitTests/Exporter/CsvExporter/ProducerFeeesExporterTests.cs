using System.Text;
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
        public void ProducerFeesExporter_CanCallExport()
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

            var csvContent = new StringBuilder();

            // Act
            var calcResult = TestDataHelper.GetCalcResult();
            var scaledupIds = calcResult.CalcResultScaledupProducers.ScaledupProducers.Select(p => p.ProducerId).ToList();
            var partialIds = calcResult.CalcResultPartialObligations.PartialObligations.Select(p => (p.ProducerId, p.SubsidiaryId)).ToList();
            _testClass.Export(runContext, producerFees, materials, scaledupIds, partialIds, csvContent);

            // Assert
            Assert.IsNotNull(csvContent.ToString());
        }
    }
}
