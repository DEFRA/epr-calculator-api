using System.Text;
using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Exporter.CsvExporter.CancelledProducers
{
    [TestClass]
    public class CalcResultCancelledProducersExporterTests
    {
        private readonly ImmutableList<MaterialDetail> materials =
        [
            new() { Id = 1, Code = "AL", Name = "Aluminium" },
            new() { Id = 2, Code = "GL", Name = "Glass" },
            new() { Id = 3, Code = "OT", Name = "Other Materials" }
        ];

        [TestMethod]
        public void Export_ShouldWriteExpectedHeadersToCsv()
        {
            // Arrange
            var exporter = new CalcResultCancelledProducersExporter();
            var stringBuilder = new StringBuilder();

            var cancelledProducersResponse = new List<CalcResultCancelledProducer>
                {
                    new CalcResultCancelledProducer
                    {
                         ProducerId = 1,
                         TradingName = "TestTrading",
                         ProducerOrSubsidiaryName = "Test Producer",
                          LastTonnage = new LastTonnage
                          {
                               Aluminium = null,
                               FibreComposite = null,
                               OtherMaterials = null,
                               Glass = null,
                               PaperOrCard = null,
                               Steel = null,
                               Plastic = null,
                               Wood = null,
                          },
                           LatestInvoice = new LatestInvoice
                           {
                                BillingInstructionId = "1_1",
                                CurrentYearInvoicedTotalToDate = 100,
                                RunName = "Run1",
                                RunNumber = "1",
                           }
                }
            };

            // Act
            exporter.Export(cancelledProducersResponse, materials, stringBuilder);
            var csvOutput = stringBuilder.ToString();

            // Assert
            Assert.IsTrue(csvOutput.Contains(CalcResultCancelledProducersHeader.CancelledProducers), "CSV should include title header.");
            Assert.IsTrue(csvOutput.Contains(CalcResultCancelledProducersHeader.LastTonnage), "CSV should include LastTonnage subheader.");
            Assert.IsTrue(csvOutput.Contains(CalcResultCancelledProducersHeader.LatestInvoice), "CSV should include LatestInvoice subheader.");
            Assert.IsTrue(csvOutput.Contains(CalcResultCancelledProducersHeader.ProducerId), "CSV should include ProducerId column.");
            Assert.IsTrue(csvOutput.Contains(CalcResultCancelledProducersHeader.BillingInstructionId), "CSV should include BillingInstructionId column.");
            Assert.IsTrue(csvOutput.Contains(CalcResultCancelledProducersHeader.TradingName), "CSV should include TradingName column.");
            Assert.IsTrue(csvOutput.Contains("Aluminium"), "CSV should include Aluminium column.");
            Assert.IsTrue(csvOutput.Contains("Glass"), "CSV should include Glass column.");
            Assert.IsTrue(csvOutput.Contains("Other Materials"), "CSV should include Other Materials column.");
            Assert.IsTrue(csvOutput.Contains(CalcResultCancelledProducersHeader.CurrentYearInvoicedTotalToDate), "CSV should include CurrentYearInvoicedTotalToDate column.");
            Assert.IsTrue(csvOutput.Contains(CalcResultCancelledProducersHeader.RunNumber), "CSV should include RunNumber column.");
            Assert.IsTrue(csvOutput.Contains(CalcResultCancelledProducersHeader.RunName), "CSV should include RunName column.");
            Assert.IsTrue(csvOutput.Contains(CalcResultCancelledProducersHeader.BillingInstructionId), "CSV should include BillingInstructionId column.");
            Assert.IsTrue(csvOutput.Contains("Run1"));
            Assert.IsTrue(csvOutput.Contains("1"));
            Assert.IsTrue(csvOutput.Contains("1_1"));
            Assert.IsTrue(csvOutput.Contains("100"));
            Assert.IsTrue(csvOutput.Contains(",,,,,"));

        }

        [TestMethod]
        public void Export_ShouldAddEmptyLinesAndHeaders()
        {
            // Arrange
            var exporter = new CalcResultCancelledProducersExporter();
            var response = new List<CalcResultCancelledProducer>();
            var csvContent = new StringBuilder();

            // Act
            exporter.Export(response, materials, csvContent);

            // Assert
            var result = csvContent.ToString();
            Assert.IsTrue(result.Contains("Cancelled Producers"));
            Assert.IsTrue(result.Contains("Last Tonnage"));
            Assert.IsTrue(result.Contains("Latest Invoice"));
            Assert.IsTrue(result.Contains(",,,,,")); // Check for empty values

        }

        [TestMethod]
        public void Export_ShouldHandleLastInvoiceNull()
        {
            // Arrange
            var exporter = new CalcResultCancelledProducersExporter();
            var cancelledProducersResponse = new List<CalcResultCancelledProducer>
                {
                    new CalcResultCancelledProducer
                    {
                         ProducerId = 1,
                         TradingName = "TestTrading",
                         ProducerOrSubsidiaryName = "Test Producer",
                           LatestInvoice = new LatestInvoice
                           {
                                BillingInstructionId = "1_1",
                                CurrentYearInvoicedTotalToDate = 100,
                                RunName = "Run1",
                                RunNumber = "1",
                           }
                    }
            };
            var csvContent = new StringBuilder();

            // Act
            exporter.Export(cancelledProducersResponse, materials, csvContent);

            // Assert
            var result = csvContent.ToString();
            Assert.IsTrue(result.Contains("Cancelled Producers"));
            Assert.IsTrue(result.Contains("Last Tonnage"));
            Assert.IsTrue(result.Contains("Latest Invoice"));
            Assert.IsTrue(result.Contains(",,,,,")); // Check for empty values

        }

        [TestMethod]
        public void Export_ShouldHandleEmptyCancelledProducers()
        {
            // Arrange
            var exporter = new CalcResultCancelledProducersExporter();
            var response = new List<CalcResultCancelledProducer>();
            var csvContent = new StringBuilder();

            // Act
            exporter.Export(response, materials, csvContent);

            // Assert
            var result = csvContent.ToString();
            Assert.IsTrue(result.Contains("Cancelled Producers"));
            Assert.IsFalse(result.Contains("ProducerId"));
        }
    }
}
