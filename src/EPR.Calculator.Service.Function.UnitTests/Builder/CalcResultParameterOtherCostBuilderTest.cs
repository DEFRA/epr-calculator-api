using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Builder;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultParameterOtherCostBuilderTest : TestsFor<CalcResultParameterOtherCostBuilder>
{

    [TestMethod]
    public async Task ConstructTest()
    {
        dbContext.CostType.Add(new CostType { Code = "1", Name = "LA Data Prep Charge", Description = "LA Data Prep Charge" });
        await dbContext.SaveChangesAsync();

        var runContext = TestDataHelper.CalculatorRun2024;
        runContext = runContext with
        {
            DefaultParameters = runContext.DefaultParameters with
            {
                SchemeAdministratorOperatingCostsByCountry = new ByCountryCost
                {
                    England = 40,
                    Wales = 30,
                    Scotland = 20,
                    NorthernIreland = 10
                },

                SchemeSetupCostsByCountry = new ByCountryCost
                {
                    England = 40,
                    Wales = 30,
                    Scotland = 20,
                    NorthernIreland = 10
                },

                LocalAuthorityDataPreparationCostsByCountry = new ByCountryCost
                {
                    England = 40,
                    Wales = 30,
                    Scotland = 20,
                    NorthernIreland = 10
                },

                MaterialityThreshold = new Threshold
                {
                    AmountIncrease = 10,
                    AmountDecrease = 10,
                    PercentIncrease = 10,
                    PercentDecrease = 10
                },

                TonnageChangeThreshold = new Threshold
                {
                    AmountIncrease = 10,
                    AmountDecrease = 10,
                    PercentIncrease = 10,
                    PercentDecrease = 10
                },

                BadDebtProvision = 10,
                CutOffDate = null
            }
        };

        var otherCost = await testSubject.ConstructAsync(runContext);

        Assert.AreEqual(40M, otherCost.SaOperatingCost.England);
        Assert.AreEqual(20, otherCost.SaOperatingCost.Scotland);
        Assert.AreEqual(30, otherCost.SaOperatingCost.Wales);
        Assert.AreEqual(10, otherCost.SaOperatingCost.NorthernIreland);

        var dataLa = otherCost.LaDataPrepCharge;
        Assert.AreEqual(40M, dataLa.England);
        Assert.AreEqual(20M, dataLa.Scotland);
        Assert.AreEqual(30M, dataLa.Wales);
        Assert.AreEqual(10M, dataLa.NorthernIreland);

        var counteyAppLa = otherCost.CountryApportionment;
        Assert.AreEqual(40, counteyAppLa.England);
        Assert.AreEqual(20, counteyAppLa.Scotland);
        Assert.AreEqual(30, counteyAppLa.Wales);
        Assert.AreEqual(10, counteyAppLa.NorthernIreland);

        var schemeSetup = otherCost.SchemeSetupCost;
        Assert.AreEqual(40, schemeSetup.England);
        Assert.AreEqual(20, schemeSetup.Scotland);
        Assert.AreEqual(30, schemeSetup.Wales);
        Assert.AreEqual(10, schemeSetup.NorthernIreland);

        Assert.AreEqual(10, otherCost.MaterialityIncrease.Amount);
        Assert.AreEqual(10, otherCost.MaterialityIncrease.Percentage);
        Assert.AreEqual(10, otherCost.MaterialityDecrease.Amount);
        Assert.AreEqual(10, otherCost.MaterialityDecrease.Percentage);
        Assert.AreEqual(10, otherCost.TonnageChangeIncrease.Amount);
        Assert.AreEqual(10, otherCost.TonnageChangeIncrease.Percentage);
        Assert.AreEqual(10, otherCost.TonnageChangeDecrease.Amount);
        Assert.AreEqual(10, otherCost.TonnageChangeDecrease.Percentage);

        Assert.AreEqual(10, otherCost.BadDebtValue);
        Assert.IsNull(otherCost.CutOffDate);
    }
}
