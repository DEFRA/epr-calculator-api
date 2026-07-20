using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.UnitTests.DataModels;

[TestClass]
public class FeeWithBadDebtTests
{
    private static ByCountryCost MakeCountryCost(decimal england, decimal wales, decimal scotland, decimal ni) =>
        new() { England = england, Wales = wales, Scotland = scotland, NorthernIreland = ni };

    private static FeeWithBadDebt Make(
        decimal feeWithout, decimal badDebt, decimal england, decimal wales, decimal scotland, decimal ni) =>
        new()
        {
            FeeWithoutBadDebt = feeWithout,
            BadDebt           = badDebt,
            ByCountry    = MakeCountryCost(england, wales, scotland, ni),
        };

    [TestMethod]
    public void Addition_SumsAllFields()
    {
        var a = Make(feeWithout: 10, badDebt: 2, england: 1, wales: 2, scotland: 3, ni: 4);
        var b = Make(feeWithout: 20, badDebt: 3, england: 10, wales: 20, scotland: 30, ni: 40);

        var result = a + b;

        result.FeeWithoutBadDebt.ShouldBe(30);
        result.BadDebt.ShouldBe(5);
        result.ByCountry.England.ShouldBe(11);
        result.ByCountry.Wales.ShouldBe(22);
        result.ByCountry.Scotland.ShouldBe(33);
        result.ByCountry.NorthernIreland.ShouldBe(44);
    }

    [TestMethod]
    public void Addition_WithEmpty_ReturnsOriginal()
    {
        var a = Make(feeWithout: 10, badDebt: 2, england: 1, wales: 2, scotland: 3, ni: 4);

        var result = a + FeeWithBadDebt.Empty;

        result.FeeWithoutBadDebt.ShouldBe(a.FeeWithoutBadDebt);
        result.BadDebt.ShouldBe(a.BadDebt);
        result.ByCountry.England.ShouldBe(a.ByCountry.England);
        result.ByCountry.Wales.ShouldBe(a.ByCountry.Wales);
        result.ByCountry.Scotland.ShouldBe(a.ByCountry.Scotland);
        result.ByCountry.NorthernIreland.ShouldBe(a.ByCountry.NorthernIreland);
    }

    [TestMethod]
    public void Sum_AggregatesTotalAllFields()
    {
        var items = new[]
        {
            Make(feeWithout: 1,   badDebt: 1, england: 1,   wales: 1,   scotland: 1,   ni: 1),
            Make(feeWithout: 10,  badDebt: 2, england: 10,  wales: 10,  scotland: 10,  ni: 10),
            Make(feeWithout: 100, badDebt: 3, england: 100, wales: 100, scotland: 100, ni: 100),
        };

        var result = items.Sum();

        result.FeeWithoutBadDebt.ShouldBe(111);
        result.BadDebt.ShouldBe(6);
        result.ByCountry.England.ShouldBe(111);
        result.ByCountry.Wales.ShouldBe(111);
        result.ByCountry.Scotland.ShouldBe(111);
        result.ByCountry.NorthernIreland.ShouldBe(111);
    }

    [TestMethod]
    public void Sum_EmptyCollection_ReturnsEmpty()
    {
        var result = Array.Empty<FeeWithBadDebt>().Sum();

        result.FeeWithoutBadDebt.ShouldBe(0);
        result.BadDebt.ShouldBe(0);
        result.ByCountry.England.ShouldBe(0);
        result.ByCountry.Wales.ShouldBe(0);
        result.ByCountry.Scotland.ShouldBe(0);
        result.ByCountry.NorthernIreland.ShouldBe(0);
    }
}
