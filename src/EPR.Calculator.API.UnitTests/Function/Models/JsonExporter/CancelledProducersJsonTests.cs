using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CancelledProducersJsonTests
    {
        [TestMethod]
        public void From_ConvertsEmptyResponse()
        {
            var response = new CalcResultCancelledProducersResponse { TitleHeader = "Header", CancelledProducers = new List<CalcResultCancelledProducersDto>() };
            var result = CancelledProducers.From(response);

            Assert.IsNotNull(result);
            Assert.AreEqual(CommonConstants.CancelledProducers, result.Name);
            Assert.IsNotNull(result.CancelledProducerTonnageInvoice);
        }

        [TestMethod]
        public void From_MapsNonEmptyResponse()
        {
            var response = new CalcResultCancelledProducersResponse
            {
                TitleHeader = "Header",
                CancelledProducers = new List<CalcResultCancelledProducersDto>
                {
                    new CalcResultCancelledProducersDto
                    {
                        ProducerId = 123,
                        SubsidiaryIdValue = "S1",
                        ProducerOrSubsidiaryNameValue = "Producer Ltd",
                        TradingNameValue = "Producer Trading",
                        LastTonnage = new LastTonnage
                        {
                            AluminiumValue = 12.5m,
                            PlasticValue = 3.25m
                        },
                        LatestInvoice = new LatestInvoice
                        {
                            CurrentYearInvoicedTotalToDateValue = 99.99m,
                            RunNumberValue = "7",
                            RunNameValue = "RunSeven",
                            BillingInstructionIdValue = "BI-1"
                        }
                    }
                }
            };

            var result = CancelledProducers.From(response);

            Assert.AreEqual(CommonConstants.CancelledProducers, result.Name);
            var list = result.CancelledProducerTonnageInvoice!.ToList();
            Assert.AreEqual(1, list.Count);
            var invoice = list[0];
            Assert.AreEqual(123, invoice.ProducerId);
            Assert.AreEqual("Producer Ltd", invoice.ProducerName);
            Assert.AreEqual("Producer Trading", invoice.TradingName);
            Assert.AreEqual(99.99m, invoice.LastInvoicedTotal);
            Assert.AreEqual(7, invoice.RunNumber);
            Assert.AreEqual("BI-1", invoice.BillingInstructionID);

            var last = invoice.LastProducerTonnages.ToList();
            Assert.IsTrue(last.Any(l => l.MaterialName == "Aluminium" && l.LastTonnage == 12.5m));
            Assert.IsTrue(last.Any(l => l.MaterialName == "Plastic" && l.LastTonnage == 3.25m));
        }
    }
}
