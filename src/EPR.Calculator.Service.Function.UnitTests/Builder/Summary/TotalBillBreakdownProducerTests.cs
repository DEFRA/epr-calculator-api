using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class TotalBillBreakdownProducerTests
{
    private readonly CalcResult calcResult = TestDataHelper.GetCalcResult();

    /// <summary>
    ///     The CanCallSetValues
    /// </summary>
    [TestMethod]
    public void TotalBillBreakdownProducer_CanCallSetValues()
    {
        // Act
        TotalBillBreakdownProducer.SetValues(calcResult.ProducerFees);

        // Assert
        Assert.AreEqual(17673.2373499970378m , calcResult.ProducerFees.Details.ToList()[0].FeeDetail.TotalBillBreakdown!.FeeWithoutBadDebt);
        Assert.AreEqual(1060.39424099982226m , calcResult.ProducerFees.Details.ToList()[0].FeeDetail.TotalBillBreakdown!.BadDebt);
        Assert.AreEqual(18733.63159099686001m, calcResult.ProducerFees.Details.ToList()[0].FeeDetail.TotalBillBreakdown!.ByCountry.Total);
        Assert.AreEqual(9610.6053147004709m  , calcResult.ProducerFees.Details.ToList()[0].FeeDetail.TotalBillBreakdown!.ByCountry.England);
        Assert.AreEqual(2653.2546023494487m  , calcResult.ProducerFees.Details.ToList()[0].FeeDetail.TotalBillBreakdown!.ByCountry.Wales);
        Assert.AreEqual(4576.19121409722784m , calcResult.ProducerFees.Details.ToList()[0].FeeDetail.TotalBillBreakdown!.ByCountry.Scotland);
        Assert.AreEqual(1893.58045984971257m , calcResult.ProducerFees.Details.ToList()[0].FeeDetail.TotalBillBreakdown!.ByCountry.NorthernIreland);
    }
}
