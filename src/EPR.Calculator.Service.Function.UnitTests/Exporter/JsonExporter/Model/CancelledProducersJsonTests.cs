using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.JsonExporter.Model;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.UnitTests.JsonExporter.Model;

[TestClass]
public class CancelledProducersJsonTests
{
    [TestMethod]
    public void From_ConvertsEmptyResponse()
    {
        var response = new List<CalcResultCancelledProducer>();
        var result = CancelledProducers.From(response);

        Assert.IsNotNull(result);
        Assert.AreEqual(CalcResultCancelledProducersHeader.CancelledProducers, result.Name);
        Assert.IsNotNull(result.CancelledProducerTonnageInvoices);
    }

    [TestMethod]
    public void From_MapsNonEmptyResponse()
    {
        var response = new List<CalcResultCancelledProducer>
        {
            new CalcResultCancelledProducer
            {
                ProducerId = 123,
                SubsidiaryId = "S1",
                ProducerOrSubsidiaryName = "Producer Ltd",
                TradingName = "Producer Trading",
                LastTonnage = new LastTonnage
                {
                    Aluminium = 12.5m,
                    Plastic = 3.25m
                },
                LatestInvoice = new LatestInvoice
                {
                    CurrentYearInvoicedTotalToDate = 99.99m,
                    RunNumber = "7",
                    RunName = "RunSeven",
                    BillingInstructionId = "BI-1"
                }
            }
        };

        var result = CancelledProducers.From(response);

        Assert.AreEqual(CalcResultCancelledProducersHeader.CancelledProducers, result.Name);
        var list = result.CancelledProducerTonnageInvoices!.ToList();
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
