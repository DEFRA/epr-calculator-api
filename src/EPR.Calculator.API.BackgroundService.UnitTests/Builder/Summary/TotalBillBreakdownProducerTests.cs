using EPR.Calculator.API.BackgroundService.Builder.Summary;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class TotalBillBreakdownProducerTests
{
    private readonly ProducerFees producerFees = TestDataHelper.GetProducerFees();

    /// <summary>
    ///     The CanCallSetValues
    /// </summary>
    [TestMethod]
    public void TotalBillBreakdownProducer_CanCallSetValues()
    {
        // Act
        TotalBillBreakdownProducer.SetValues(producerFees);

        // Assert
        Assert.AreEqual(17673.2373499970378m , producerFees.Details.ToList()[0].FeeDetail.TotalBillBreakdown!.FeeWithoutBadDebt);
        Assert.AreEqual(1060.39424099982226m , producerFees.Details.ToList()[0].FeeDetail.TotalBillBreakdown!.BadDebt);
        Assert.AreEqual(18733.63159099686001m, producerFees.Details.ToList()[0].FeeDetail.TotalBillBreakdown!.ByCountry.Total);
        Assert.AreEqual(9610.6053147004709m  , producerFees.Details.ToList()[0].FeeDetail.TotalBillBreakdown!.ByCountry.England);
        Assert.AreEqual(2653.2546023494487m  , producerFees.Details.ToList()[0].FeeDetail.TotalBillBreakdown!.ByCountry.Wales);
        Assert.AreEqual(4576.19121409722784m , producerFees.Details.ToList()[0].FeeDetail.TotalBillBreakdown!.ByCountry.Scotland);
        Assert.AreEqual(1893.58045984971257m , producerFees.Details.ToList()[0].FeeDetail.TotalBillBreakdown!.ByCountry.NorthernIreland);
    }
}
