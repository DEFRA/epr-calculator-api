using System.Collections.Immutable;
using System.Text;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Exporter.CsvExporter.CancelledProducers
{
    [TestClass]
    public class ICalcResultCancelledProducersExporterTests
    {
        private Mock<ICalcResultCancelledProducersExporter> _exporterMock;

        private readonly ImmutableList<MaterialDetail> materials =
        [
            new() { Id = 1, Code = "AL", Name = "Aluminium" },
            new() { Id = 2, Code = "GL", Name = "Glass" },
            new() { Id = 3, Code = "OT", Name = "Other materials" }
        ];

        public ICalcResultCancelledProducersExporterTests()
        {
            _exporterMock = new Mock<ICalcResultCancelledProducersExporter>();
        }

        [TestMethod]
        public void Export_ShouldBeCalledWithCorrectParameters()
        {
            // Arrange
            var response = new List<CalcResultCancelledProducer>
            {
                new CalcResultCancelledProducer
                {
                    ProducerId = 123,
                    TradingName = "Acme Ltd",
                    LastTonnage = new LastTonnage
                    {
                        Aluminium = 25.5M
                    },
                    LatestInvoice = new LatestInvoice
                    {
                        CurrentYearInvoicedTotalToDate = 1010.75M
                    }
                }
            };

            var sb = new StringBuilder();

            // Act
            _exporterMock.Object.Export(response, materials, sb);

            // Assert
            _exporterMock.Verify(e => e.Export(response, materials, sb), Times.Once);
        }
    }
}
