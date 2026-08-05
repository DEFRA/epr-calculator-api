using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class OnePlus2A2B2CProducerTests
{
    private readonly CalcResult calcResult = TestDataHelper.GetCalcResult();

    [TestMethod]
    public void OnePlus2A2B2CProducer_CanCallSetValues()
    {
        // Act
        OnePlus2A2B2CProducer.SetValues(calcResult.ProducerFees);

        // Assert
        Assert.AreEqual(10491.16776684412368m, calcResult.ProducerFees.Total.TotalOnePlus2A2B2CWithBadDebt());
        Assert.AreEqual(10491.16776684412368m, calcResult.ProducerFees.Details.ToList()[0].FeeDetail.TotalOnePlus2A2B2CWithBadDebt());
        Assert.AreEqual(100m, calcResult.ProducerFees.Details.ToList()[0].FeeDetail.TotalOnePlus2A2B2CWithBadDebtPercentage);
    }
}
