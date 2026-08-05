using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ErrorReport;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.ErrorReport
{
    [TestClass]
    public class CalcResultErrorReportExporterTests
    {
        [TestMethod]
        public void Export_ShouldWriteExpectedHeadersToCsv()
        {
            // Arrange
            var exporter = new CalcResultErrorReportExporter();
            var stringBuilder = new StringBuilder();

            var calcResultErrorReport = new List<CalcResultErrorReport>();          

            // Act
            exporter.Export(calcResultErrorReport, stringBuilder);
            var csvOutput = stringBuilder.ToString();

            // Assert
            Assert.IsTrue(csvOutput.Contains(CommonConstants.ErrorReportHeader), "CSV should include title header.");            
            Assert.IsTrue(csvOutput.Contains(CommonConstants.ProducerId), "CSV should include ProducerId column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.SubsidiaryId), "CSV should include SubsidiaryId column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.ProducerSubsidaryName), "CSV should include ProducerOrSubsidiaryName column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.TradingName), "CSV should include TradingName column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.LeaverCode), "CSV should include LeaverCode column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.ErrorCodeText), "CSV should include ErrorCodeText column.");

        }

        [TestMethod]
        public void Export_ShouldWriteExpectedValuesToCsv()
        {
            // Arrange
            var exporter = new CalcResultErrorReportExporter();
            var stringBuilder = new StringBuilder();

            var calcResultErrorReport = new List<CalcResultErrorReport>();
            var fixture = new Fixture();
            var error = fixture.Create<CalcResultErrorReport>();
            calcResultErrorReport.Add(error);

            // Act
            exporter.Export(calcResultErrorReport, stringBuilder);
            var csvOutput = stringBuilder.ToString();

            // Assert
            Assert.IsTrue(csvOutput.Contains(error.ProducerId.ToString()));
            Assert.IsTrue(csvOutput.Contains(error.SubsidiaryId));
            Assert.IsTrue(csvOutput.Contains(error.ProducerName));
            Assert.IsTrue(csvOutput.Contains(error.TradingName));
            Assert.IsTrue(csvOutput.Contains(error.LeaverCode));
            Assert.IsTrue(csvOutput.Contains(error.ErrorCodeText));
        }

        [TestMethod]
        public void Export_ShouldHandleEmptyErrorReport()
        {
            // Arrange  
            var exporter = new CalcResultErrorReportExporter();
            var response = new List<CalcResultErrorReport>();
            var csvContent = new StringBuilder();

            // Act  
            exporter.Export(response, csvContent);

            // Assert  
            var result = csvContent.ToString();
            Assert.IsTrue(result.Contains("Error"));
            Assert.IsTrue(result.Contains("Producer ID"));
            Assert.IsTrue(result.Contains("Subsidiary ID"));
            Assert.IsTrue(result.Contains("Trading Name"));
            Assert.IsTrue(result.Contains("Producer / Subsidiary Name"));
            Assert.IsTrue(result.Contains("Leaver Code"));
            Assert.IsTrue(result.Contains("Error Code Text"));
        }

        [TestMethod]
        public void Export_ShouldHandleNullErrorReport()
        {
            // Arrange  
            var exporter = new CalcResultErrorReportExporter();
            var response = new List<CalcResultErrorReport>
            {
                new CalcResultErrorReport
                {
                    ProducerId = 0,
                    ErrorCodeText = "test",
                    LeaverCode = "LeaverCode",
                    ProducerName = "Name",
                    SubsidiaryId = "SusbId",
                    TradingName = "Trading"


                }
            };
            var csvContent = new StringBuilder();

            // Act  
            exporter.Export(response, csvContent);

            // Assert  
            var result = csvContent.ToString();
            Assert.IsTrue(result.Contains(CommonConstants.Hyphen));
        }
    }
}
