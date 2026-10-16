using EPR.Calculator.API.BackgroundService.Builder.Summary;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class OnePlus2A2B2CProducerTests
{
    private readonly ProducerFees producerFees = TestDataHelper.GetProducerFees();

    [TestMethod]
    public void OnePlus2A2B2CProducer_CanCallSetValues()
    {
        // Act
        OnePlus2A2B2CProducer.SetValues(producerFees);

        // Assert
        Assert.AreEqual(10491.16776684412368m, producerFees.Total.TotalOnePlus2A2B2CWithBadDebt());
        Assert.AreEqual(10491.16776684412368m, producerFees.Details.ToList()[0].FeeDetail.TotalOnePlus2A2B2CWithBadDebt());
        Assert.AreEqual(100m, producerFees.Details.ToList()[0].FeeDetail.TotalOnePlus2A2B2CWithBadDebtPercentage);
    }
}
