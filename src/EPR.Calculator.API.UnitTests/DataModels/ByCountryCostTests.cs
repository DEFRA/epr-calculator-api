using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.UnitTests.DataModels;

[TestClass]
public class ByCountryCostTests
{
    private static ByCountryCost Make(decimal england, decimal wales, decimal scotland, decimal ni) =>
        new() { England = england, Wales = wales, Scotland = scotland, NorthernIreland = ni };

    [TestMethod]
    public void Addition_SumsEachCountry()
    {
        var a = Make(1, 2, 3, 4);
        var b = Make(10, 20, 30, 40);

        var result = a + b;

        result.England.ShouldBe(11);
        result.Wales.ShouldBe(22);
        result.Scotland.ShouldBe(33);
        result.NorthernIreland.ShouldBe(44);
    }

    [TestMethod]
    public void Addition_WithEmpty_ReturnsOriginal()
    {
        var a = Make(1, 2, 3, 4);

        var result = a + ByCountryCost.Empty;

        result.England.ShouldBe(a.England);
        result.Wales.ShouldBe(a.Wales);
        result.Scotland.ShouldBe(a.Scotland);
        result.NorthernIreland.ShouldBe(a.NorthernIreland);
    }

    [TestMethod]
    public void Sum_AggregatesTotalPerCountry()
    {
        var costs = new[]
        {
            Make(1, 2, 3, 4),
            Make(10, 20, 30, 40),
            Make(100, 200, 300, 400),
        };

        var result = ByCountryCost.Sum(costs);

        result.England.ShouldBe(111);
        result.Wales.ShouldBe(222);
        result.Scotland.ShouldBe(333);
        result.NorthernIreland.ShouldBe(444);
    }

    [TestMethod]
    public void Sum_EmptyCollection_ReturnsZeroes()
    {
        var result = ByCountryCost.Sum([]);

        result.England.ShouldBe(0);
        result.Wales.ShouldBe(0);
        result.Scotland.ShouldBe(0);
        result.NorthernIreland.ShouldBe(0);
    }

    [TestMethod]
    public void Total_IsSumOfAllCountries()
    {
        var cost = Make(1, 2, 3, 4);

        cost.Total.ShouldBe(10);
    }
}
