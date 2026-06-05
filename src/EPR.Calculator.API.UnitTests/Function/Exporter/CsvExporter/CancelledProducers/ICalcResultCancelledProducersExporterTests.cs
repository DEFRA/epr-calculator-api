using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.CancelledProducers
{
    [TestClass]
    public class ICalcResultCancelledProducersExporterTests
    {
        private Mock<ICalcResultCancelledProducersExporter> _exporterMock;

        public ICalcResultCancelledProducersExporterTests()
        {
            _exporterMock = new Mock<ICalcResultCancelledProducersExporter>();
        }

        [TestMethod]
        public void Export_ShouldBeCalledWithCorrectParameters()
        {
            // Arrange
            var response = new CalcResultCancelledProducersResponse
            {
                TitleHeader = "Cancelled Producers Export",
                CancelledProducers = new List<CalcResultCancelledProducersDto>
                {
                    new CalcResultCancelledProducersDto
                    {
                        ProducerId = 123,
                        TradingNameValue = "Acme Ltd",
                        LastTonnage = new LastTonnage
                        {
                            AluminiumValue = 25.5M
                        },
                        LatestInvoice = new LatestInvoice
                        {
                            CurrentYearInvoicedTotalToDateValue = 1010.75M
                        }
                    }
                }
            };

            var sb = new StringBuilder();

            // Act
            _exporterMock.Object.Export(response, sb);

            // Assert
            _exporterMock.Verify(e => e.Export(response, sb), Times.Once);
        }
    }
}
