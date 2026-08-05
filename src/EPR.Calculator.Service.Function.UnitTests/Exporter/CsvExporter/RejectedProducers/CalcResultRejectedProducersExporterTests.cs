using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.RejectedProducers;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.RejectedProducers
{
    [TestClass]
    public class CalcResultRejectedProducersExporterTests
    {
        [TestMethod]
        public void Export_WritesDataRows()
        {
            // Arrange
            var exporter = new CalcResultRejectedProducersExporter();
            var csvContent = new StringBuilder();
            var rejectedProducers = new List<CalcResultRejectedProducer>
            {
                new CalcResultRejectedProducer
                {
                    ProducerId = 1,
                    ProducerName = "Producer A",
                    TradingName = "Trade A",
                    SuggestedBillingInstruction = "Bill A",
                    SuggestedInvoiceAmount = 123.45m,
                    InstructionConfirmedDate = new DateTime(2024, 1, 2, 14, 30, 0),
                    InstructionConfirmedBy = "User A",
                    ReasonForRejection = "Reason A"
                }
            };

            // Act
            exporter.Export(rejectedProducers, csvContent);

            // Assert
            var lines = csvContent.ToString().Split(Environment.NewLine);
            Assert.IsTrue(lines[5].Contains("1"));
            Assert.IsTrue(lines[5].Contains("Producer A"));
            Assert.IsTrue(lines[5].Contains("Trade A"));
            Assert.IsTrue(lines[5].Contains("Bill A"));
            Assert.IsTrue(lines[5].Contains("123.45"));
            Assert.IsTrue(lines[5].Contains("2024"));
            Assert.IsTrue(lines[5].Contains("User A"));
            Assert.IsTrue(lines[5].Contains("02/01/2024 14:30:00"));
        }

        [TestMethod]
        public void Export_DateIsNull_WritesDataRows()
        {
            // Arrange
            var exporter = new CalcResultRejectedProducersExporter();
            var csvContent = new StringBuilder();
            var rejectedProducers = new List<CalcResultRejectedProducer>
            {
                new CalcResultRejectedProducer
                {
                    ProducerId = 1,
                    ProducerName = "Producer A",
                    TradingName = "Trade A",
                    SuggestedBillingInstruction = "Bill A",
                    SuggestedInvoiceAmount = 123.45m,
                    InstructionConfirmedDate = null,
                    InstructionConfirmedBy = "User A",
                    ReasonForRejection = "Reason A"
                }
            };

            // Act
            exporter.Export(rejectedProducers, csvContent);

            // Assert
            var lines = csvContent.ToString().Split(Environment.NewLine);
            Assert.IsFalse(lines[5].Contains("02/01/2024 00:00:00"));
        }

        [TestMethod]
        public void Export_MultipleProducers_WritesMultipleRows()
        {
            // Arrange
            var exporter = new CalcResultRejectedProducersExporter();
            var csvContent = new StringBuilder();
            var rejectedProducers = new List<CalcResultRejectedProducer>
            {
                new CalcResultRejectedProducer
                {
                    ProducerId = 1,
                    ProducerName = "Producer A",
                    TradingName = "Trade A",
                    SuggestedBillingInstruction = "Bill A",
                    SuggestedInvoiceAmount = 123.45m,
                    InstructionConfirmedDate = new DateTime(2024, 1, 2),
                    InstructionConfirmedBy = "User A",
                    ReasonForRejection = "Reason A"
                },
                new CalcResultRejectedProducer
                {
                    ProducerId = 2,
                    ProducerName = "Producer B",
                    TradingName = "Trade B",
                    SuggestedBillingInstruction = "Bill B",
                    SuggestedInvoiceAmount = 678.90m,
                    InstructionConfirmedDate = new DateTime(2024, 2, 3),
                    InstructionConfirmedBy = "User B",
                    ReasonForRejection = "Reason B"
                }
            };

            // Act
            exporter.Export(rejectedProducers, csvContent);

            // Assert
            var lines = csvContent.ToString().Split(Environment.NewLine);
            Assert.IsTrue(lines[2].Contains("Rejected Report"));
            Assert.IsTrue(lines[5].Contains("Producer A"));
            Assert.IsTrue(lines[6].Contains("Producer B"));
        }
    }
}
