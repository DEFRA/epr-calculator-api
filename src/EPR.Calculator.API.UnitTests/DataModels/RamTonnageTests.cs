using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.UnitTests.DataModels;

[TestClass]
public class RamTonnageTests
{
    private static RamTonnage Make() =>
        new() { Red = 1, RedMedical = 2, Amber = 3, AmberMedical = 4, Green = 5, GreenMedical = 6 };

    [TestMethod]
    public void Totals_AreTheSumsUntilStored()
    {
        var tonnage = Make();

        tonnage.TotalRamTonnage.ShouldBe(21);
        tonnage.RedAndRedMedical.ShouldBe(3);
        tonnage.AmberAndAmberMedical.ShouldBe(7);
        tonnage.GreenAndGreenMedical.ShouldBe(11);
    }

    [TestMethod]
    public void Totals_AreTheStoredTotalsWhenSet()
    {
        var tonnage = Make() with { TotalRamTonnage = 21.001m, RedAndRedMedical = 3.001m, AmberAndAmberMedical = 7.001m, GreenAndGreenMedical = 11.001m };

        tonnage.TotalRamTonnage.ShouldBe(21.001m);
        tonnage.RedAndRedMedical.ShouldBe(3.001m);
        tonnage.AmberAndAmberMedical.ShouldBe(7.001m);
        tonnage.GreenAndGreenMedical.ShouldBe(11.001m);
    }

    [TestMethod]
    public void Equality_ComparesValuesWhetherTheTotalsAreStoredOrSummed()
    {
        Make().ShouldBe(Make() with { TotalRamTonnage = 21.000m });
        Make().ShouldNotBe(Make() with { TotalRamTonnage = 22 });
    }
}
