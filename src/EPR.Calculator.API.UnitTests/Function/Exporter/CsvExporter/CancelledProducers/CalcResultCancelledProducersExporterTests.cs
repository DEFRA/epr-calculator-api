using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.CancelledProducers
{
    [TestClass]
    public class CalcResultCancelledProducersExporterTests
    {
        [TestMethod]
        public void Export_ShouldWriteExpectedHeadersToCsv()
        {
            // Arrange
            var exporter = new CalcResultCancelledProducersExporter();
            var stringBuilder = new StringBuilder();

            var cancelledProducersResponse = new CalcResultCancelledProducersResponse
            {
                TitleHeader = CommonConstants.CancelledProducers,
                CancelledProducers = new List<CalcResultCancelledProducersDto>
                {
                    new CalcResultCancelledProducersDto
                    {
                        ProducerId_Header = CommonConstants.ProducerId,
                        ProducerName_Header = CommonConstants.ProducerOrSubsidiaryName,
                        TradingName_Header = CommonConstants.TradingName,
                        LastTonnage = new LastTonnage
                        {
                            LastTonnage_Header=CommonConstants.LastTonnage,
                            Aluminium_Header = CommonConstants.Aluminium,
                                FibreComposite_Header = CommonConstants.FibreComposite,
                                Glass_Header = CommonConstants.Glass,
                                PaperOrCard_Header = CommonConstants.PaperOrCard,
                                Plastic_Header = CommonConstants.Plastic,
                                Steel_Header = CommonConstants.Steel,
                                Wood_Header = CommonConstants.Wood,
                                OtherMaterials_Header = CommonConstants.OtherMaterials,
                        },
                        LatestInvoice = new LatestInvoice
                        {
                            LatestInvoice_Header = CommonConstants.LatestInvoice,
                            CurrentYearInvoicedTotalToDate_Header = CommonConstants.LastInvoicedTotal,
                            RunNumber_Header = CommonConstants.RunNumber,
                            RunName_Header = CommonConstants.RunName,
                            BillingInstructionId_Header = CommonConstants.BillingInstructionId,
                        }
                    },
                    new CalcResultCancelledProducersDto
                    {
                         ProducerId = 1,
                         TradingNameValue = "TestTrading",
                         ProducerOrSubsidiaryNameValue = "Test Producer",
                          LastTonnage = new LastTonnage
                          {
                               AluminiumValue = null,
                               FibreCompositeValue = null,
                               OtherMaterialsValue = null,
                               GlassValue = null,
                               PaperOrCardValue = null,
                               SteelValue = null,
                               PlasticValue = null,
                               WoodValue = null,
                          },
                           LatestInvoice = new LatestInvoice
                           {
                                BillingInstructionIdValue = "1_1",
                                CurrentYearInvoicedTotalToDateValue = 100,
                                RunNameValue = "Run1",
                                RunNumberValue = "1",
                           }


                    }
                }
            };

            // Act
            exporter.Export(cancelledProducersResponse, stringBuilder);
            var csvOutput = stringBuilder.ToString();

            // Assert
            Assert.IsTrue(csvOutput.Contains(CommonConstants.CancelledProducers), "CSV should include title header.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.LastTonnage), "CSV should include LastTonnage subheader.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.LatestInvoice), "CSV should include LatestInvoice subheader.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.ProducerId), "CSV should include ProducerId column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.BillingInstructionId), "CSV should include BillingInstructionId column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.TradingName), "CSV should include TradingName column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.Aluminium), "CSV should include Aluminium column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.FibreComposite), "CSV should include FibreComposite column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.Glass), "CSV should include Glass column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.PaperOrCard), "CSV should include PaperOrCard column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.Plastic), "CSV should include Plastic column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.Steel), "CSV should include Steel column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.Wood), "CSV should include Wood column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.OtherMaterials), "CSV should include OtherMaterials column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.CurrentYearInvoicedTotalToDate), "CSV should include CurrentYearInvoicedTotalToDate column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.RunNumber), "CSV should include RunNumber column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.RunName), "CSV should include RunName column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.BillingInstructionId), "CSV should include BillingInstructionId column.");
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
            var response = new CalcResultCancelledProducersResponse
            {
                TitleHeader = "Cancelled Producers",
                CancelledProducers = new List<CalcResultCancelledProducersDto>()
            };
            var csvContent = new StringBuilder();

            // Act  
            exporter.Export(response, csvContent);

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
            var cancelledProducersResponse = new CalcResultCancelledProducersResponse
            {
                TitleHeader = CommonConstants.CancelledProducers,
                CancelledProducers = new List<CalcResultCancelledProducersDto>
                {
                    new CalcResultCancelledProducersDto
                    {
                        ProducerId_Header = CommonConstants.ProducerId,
                        ProducerName_Header = CommonConstants.ProducerOrSubsidiaryName,
                        TradingName_Header = CommonConstants.TradingName,
                        LastTonnage = new LastTonnage
                        {
                            LastTonnage_Header=CommonConstants.LastTonnage,
                            Aluminium_Header = CommonConstants.Aluminium,
                                FibreComposite_Header = CommonConstants.FibreComposite,
                                Glass_Header = CommonConstants.Glass,
                                PaperOrCard_Header = CommonConstants.PaperOrCard,
                                Plastic_Header = CommonConstants.Plastic,
                                Steel_Header = CommonConstants.Steel,
                                Wood_Header = CommonConstants.Wood,
                                OtherMaterials_Header = CommonConstants.OtherMaterials,
                        },
                        LatestInvoice = new LatestInvoice
                        {
                            LatestInvoice_Header = CommonConstants.LatestInvoice,
                            CurrentYearInvoicedTotalToDate_Header = CommonConstants.LastInvoicedTotal,
                            RunNumber_Header = CommonConstants.RunNumber,
                            RunName_Header = CommonConstants.RunName,
                            BillingInstructionId_Header = CommonConstants.BillingInstructionId,
                        }
                    },
                    new CalcResultCancelledProducersDto
                    {
                         ProducerId = 1,
                         TradingNameValue = "TestTrading",
                         ProducerOrSubsidiaryNameValue = "Test Producer",
                           LatestInvoice = new LatestInvoice
                           {
                                BillingInstructionIdValue = "1_1",
                                CurrentYearInvoicedTotalToDateValue = 100,
                                RunNameValue = "Run1",
                                RunNumberValue = "1",
                           }


                    }
                }
            };
            var csvContent = new StringBuilder();

            // Act  
            exporter.Export(cancelledProducersResponse, csvContent);

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
            var response = new CalcResultCancelledProducersResponse
            {
                TitleHeader = "Cancelled Producers",
                CancelledProducers = new List<CalcResultCancelledProducersDto>()
            };
            var csvContent = new StringBuilder();

            // Act  
            exporter.Export(response, csvContent);

            // Assert  
            var result = csvContent.ToString();
            Assert.IsTrue(result.Contains("Cancelled Producers"));
            Assert.IsFalse(result.Contains("ProducerId"));
        }
    }
}
