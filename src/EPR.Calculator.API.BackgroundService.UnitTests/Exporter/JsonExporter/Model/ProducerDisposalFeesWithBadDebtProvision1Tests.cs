using EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Exporter.JsonExporter.Model;

[TestClass]
public class ProducerDisposalFeesWithBadDebtProvision1Tests
{
    [TestMethod]
    public void From_MapsMaterialBreakdown()
    {
        var byMaterial = TestDataHelper.GetProducerDisposalFeesByMaterial();
        var materials = TestDataHelper.GetMaterialDetails();

        var result = ProducerDisposalFeesWithBadDebtProvision1.From(byMaterial, materials, "1", applyModulation: false);

        Assert.IsNotNull(result);
        var list = result.MaterialBreakdown.ToList();
        var al = list.Single(m => m.MaterialName == "Aluminium");
        Assert.AreEqual(1000m, al.HouseholdPackagingWasteTonnage);
        Assert.AreEqual("643.98", al.Fee);
        Assert.AreEqual("607.53", al.FeeWithoutBadDebt);

        var gl = list.Single(m => m.MaterialName == "Glass");
        Assert.AreEqual(220m, gl.HouseholdDrinksContainersTonnageGlass);
    }

    [TestMethod]
    public void From_MapsMaterialBreakdown_Modulated()
    {
        var byMaterial = TestDataHelper.GetProducerDisposalFeesByMaterial(applyModulation: true);
        var materials = TestDataHelper.GetMaterialDetails();

        var result = ProducerDisposalFeesWithBadDebtProvision1.From(byMaterial, materials, "1", applyModulation: true);

        Assert.IsNotNull(result);
        var list = result.MaterialBreakdown.ToList();
        var al = list.Single(m => m.MaterialName == "Aluminium");
        Assert.AreEqual(1000m, al.HouseholdPackagingWasteTonnage.Modulated!.Total);
        Assert.AreEqual("643.98", al.Fee);
        Assert.AreEqual("607.53", al.FeeWithoutBadDebt.Modulated!.Total);
        Assert.AreEqual("4.53"  , al.FeeWithoutBadDebt.Modulated!.RedAndRedMedical);
        Assert.AreEqual("5.00"  , al.FeeWithoutBadDebt.Modulated!.AmberAndAmberMedical);
        Assert.AreEqual("6.00"  , al.FeeWithoutBadDebt.Modulated!.GreenAndGreenMedical);

        var gl = list.Single(m => m.MaterialName == "Glass");
        Assert.AreEqual(220m, gl.HouseholdDrinksContainersTonnageGlass!.Modulated!.Total);
    }
}
