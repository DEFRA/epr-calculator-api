using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultParameterOtherCostBuilderTest : TestsFor<CalcResultParameterOtherCostBuilder>
{
    protected override void TestInitialize()
    {
        dbContext.DefaultParameterTemplateMasterList.RemoveRange(dbContext.DefaultParameterTemplateMasterList);
        dbContext.SaveChanges();
        dbContext.DefaultParameterTemplateMasterList.AddRange(TestDataHelper.GetDefaultParameterTemplateMasterData().ToList());
        dbContext.SaveChanges();
    }

    [TestMethod]
    public async Task ConstructTest()
    {
        var runContext = TestDataHelper.CalculatorRun2024;

        var run = new CalculatorRun
        {
            Id = runContext.RunId,
            RelativeYear = runContext.RelativeYear,
            Name = runContext.RunName,
            CalculatorRunClassificationId = (int)RunClassification.RUNNING,
            CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
            CreatedBy = "Test User",
            DefaultParameterSettingMasterId = 1
        };

        var templateMasterList = dbContext.DefaultParameterTemplateMasterList.ToList();

        var defaultMaster = new DefaultParameterSettingMaster { RelativeYear = new RelativeYear(2024) };
        defaultMaster.RelativeYear = new RelativeYear(2024);

        dbContext.DefaultParameterSettings.Add(defaultMaster);
        dbContext.CalculatorRuns.Add(run);
        await dbContext.SaveChangesAsync();

        foreach (var templateMaster in templateMasterList)
        {
            var defaultDetail = new DefaultParameterSettingDetail
            {
                ParameterUniqueReferenceId = templateMaster.ParameterUniqueReferenceId,
                ParameterValue = GetValue(templateMaster),
                DefaultParameterSettingMasterId = 1,
                DefaultParameterSettingMaster = null
            };
            dbContext.DefaultParameterSettingDetail.Add(defaultDetail);
        }

        var param = dbContext.DefaultParameterTemplateMasterList.Single(x =>
            x.ParameterUniqueReferenceId == "BADEBT-P");
        param.ParameterCategory = "Percentage";
        param.ParameterType = "Bad debt provision";

        dbContext.DefaultParameterTemplateMasterList.Update(param);

        dbContext.CostType.Add(new CostType
            { Code = "1", Name = "LA Data Prep Charge", Description = "LA Data Prep Charge" });

        dbContext.Country.Add(new Country { Code = "En", Name = "England", Description = "England" });
        dbContext.Country.Add(new Country { Code = "Wa", Name = "Wales", Description = "Wales" });
        dbContext.Country.Add(new Country { Code = "Sc", Name = "Scotland", Description = "Scotland" });
        dbContext.Country.Add(new Country { Code = "NI", Name = "Northern Ireland", Description = "Northern Ireland" });

        await dbContext.SaveChangesAsync();

        var otherCost = await testSubject.ConstructAsync(runContext);

        Assert.AreEqual(40M, otherCost.SaOperatingCost.England);
        Assert.AreEqual(10, otherCost.SaOperatingCost.NorthernIreland);
        Assert.AreEqual(20, otherCost.SaOperatingCost.Scotland);
        Assert.AreEqual(30, otherCost.SaOperatingCost.Wales);


        var dataLa = otherCost.LaDataPrepCharge;
        Assert.AreEqual(40M, dataLa.England);
        Assert.AreEqual(10M, dataLa.NorthernIreland);
        Assert.AreEqual(20M, dataLa.Scotland);
        Assert.AreEqual(30M, dataLa.Wales);

        var counteyAppLa = otherCost.CountryApportionment;
        Assert.AreEqual(40, counteyAppLa.England);
        Assert.AreEqual(10, counteyAppLa.NorthernIreland);
        Assert.AreEqual(20, counteyAppLa.Scotland);
        Assert.AreEqual(30, counteyAppLa.Wales);

        Assert.AreEqual(10, otherCost.BadDebtValue);

        var schemeSetup = otherCost.SchemeSetupCost;
        Assert.AreEqual(40, schemeSetup.England);
        Assert.AreEqual(10, schemeSetup.NorthernIreland);
        Assert.AreEqual(20, schemeSetup.Scotland);
        Assert.AreEqual(30, schemeSetup.Wales);

        Assert.AreEqual(10, otherCost.MaterialityIncrease.Amount);
        Assert.AreEqual(10, otherCost.MaterialityIncrease.Percentage);
        Assert.AreEqual(10, otherCost.MaterialityDecrease.Amount);
        Assert.AreEqual(10, otherCost.MaterialityDecrease.Percentage);
        Assert.AreEqual(10, otherCost.TonnageChangeIncrease.Amount);
        Assert.AreEqual(10, otherCost.TonnageChangeIncrease.Percentage);
        Assert.AreEqual(10, otherCost.TonnageChangeDecrease.Amount);
        Assert.AreEqual(10, otherCost.TonnageChangeDecrease.Percentage);
    }

    private static decimal GetValue(DefaultParameterTemplateMaster templateMaster)
    {
        if (templateMaster.ParameterType == "Scheme setup costs" ||
            templateMaster.ParameterType == "Scheme administrator operating costs" ||
            templateMaster.ParameterType == "Local authority data preparation costs")
        {
            switch (templateMaster.ParameterCategory)
            {
                case "England":
                    return 40M;
                case "Northern Ireland":
                    return 10M;
                case "Scotland":
                    return 20M;
                case "Wales":
                    return 30M;
            }
        }

        return 10;
    }
}
