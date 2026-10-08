using EPR.Calculator.API.BackgroundService.Builder.Summary;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class TwoBCommsCostProducerTests
{
    private readonly CalcResult calcResult = TestDataHelper.GetCalcResult();
    private readonly ProducerFees producerFees = TestDataHelper.GetProducerFees();

    [TestMethod]
    public void TwoBCommsCostProducer_CanCallSetValues()
    {
        // Act
        TwoBCommsCostProducer.SetValues(calcResult, producerFees);

        // Assert
        Assert.AreEqual(2531m   , producerFees.Total.CommsCostsSection2b.FeeWithoutBadDebt);
        Assert.AreEqual(151.86m , producerFees.Total.CommsCostsSection2b.BadDebt);
        Assert.AreEqual(2682.86m, producerFees.Total.CommsCostsSection2b.ByCountry.Total);
    }
}
