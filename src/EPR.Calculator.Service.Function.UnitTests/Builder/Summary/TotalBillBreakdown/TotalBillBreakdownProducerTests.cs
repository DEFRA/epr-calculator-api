using EPR.Calculator.Service.Function.Builder.Summary.TotalBillBreakdown;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.TotalBillBreakdown;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class TotalBillBreakdownProducerTests
{
    private readonly CalcResult calcResult = TestDataHelper.GetCalcResult();

    /// <summary>
    ///     The CanCallSetValues
    /// </summary>
    [TestMethod]
    public void CanCallSetValues()
    {
        // Act
        TotalBillBreakdownProducer.SetValues(calcResult.CalcResultSummary);

        // Assert
        Assert.AreEqual(17673.2373499970378m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.TotalProducerFeeWithoutBadDebtProvision);
        Assert.AreEqual(1060.39424099982226m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.BadDebtProvision);
        Assert.AreEqual(18733.6315909968600m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.TotalProducerFeeWithBadDebtProvision);
        Assert.AreEqual(9610.6053147004709m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.EnglandTotalWithBadDebtProvision);
        Assert.AreEqual(2653.2546023494487m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.WalesTotalWithBadDebtProvision);
        Assert.AreEqual(4576.19121409722784m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.ScotlandTotalWithBadDebtProvision);
        Assert.AreEqual(1893.58045984971257m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.NorthernIrelandTotalWithBadDebtProvision);
    }

    /// <summary>
    ///     The CanCallSetValues
    /// </summary>
    [TestMethod]
    public void CanCallSetValues_NullValues()
    {
        // Arrange
        var data = calcResult.CalcResultSummary;
        data.ProducerDisposalFees.ToList()[0].LocalAuthorityDisposalCostsSectionOne = null;
        data.ProducerDisposalFees.ToList()[0].CommunicationCostsSectionTwoA = null;
        data.ProducerDisposalFees.ToList()[0].CommunicationCostsSectionTwoB = null;
        data.ProducerDisposalFees.ToList()[0].TwoCTotalProducerFeeForCommsCostsWithoutBadDebt = 0;
        data.ProducerDisposalFees.ToList()[0].TwoCBadDebtProvision = 0;
        data.ProducerDisposalFees.ToList()[0].TwoCTotalProducerFeeForCommsCostsWithBadDebt = 0;
        data.ProducerDisposalFees.ToList()[0].TwoCEnglandTotalWithBadDebt = 0;
        data.ProducerDisposalFees.ToList()[0].TwoCWalesTotalWithBadDebt = 0;
        data.ProducerDisposalFees.ToList()[0].TwoCScotlandTotalWithBadDebt = 0;
        data.ProducerDisposalFees.ToList()[0].TwoCNorthernIrelandTotalWithBadDebt = 0;
        data.ProducerDisposalFees.ToList()[0].SchemeAdministratorOperatingCosts = null;
        data.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts = null;
        data.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts = null;

        // Act
        TotalBillBreakdownProducer.SetValues(data);

        // Assert
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.TotalProducerFeeWithoutBadDebtProvision);
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.BadDebtProvision);
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.TotalProducerFeeWithBadDebtProvision);
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.EnglandTotalWithBadDebtProvision);
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.WalesTotalWithBadDebtProvision);
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.ScotlandTotalWithBadDebtProvision);
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.NorthernIrelandTotalWithBadDebtProvision);
    }
}
