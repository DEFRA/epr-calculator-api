using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    [TestClass]
    public class CalcResultSummaryExporterTests
    {
        private CalcResultSummaryExporter _testClass;

        public CalcResultSummaryExporterTests()
        {
            _testClass = new CalcResultSummaryExporter();
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var runContext = TestDataHelper.CalculatorRun2025;
            var resultSummary = new CalcResultSummary
            {
                ProducerDisposalFees = TestDataHelper.GetProducerDisposalFees()
            };

            var materials = TestDataHelper.GetMaterialDetails();

            var csvContent = new StringBuilder();

            // Act
            _testClass.Export(runContext, resultSummary, materials, csvContent);

            // Assert
            Assert.IsNotNull(csvContent.ToString());
        }
    }
}
