using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Detail;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultDetailBuilderTests : TestsFor<CalcResultDetailBuilder>
{
    [TestMethod]
    public async Task Construct_AllPropertiesPresent_ReturnsCorrectData()
    {
        var runContext = TestDataHelper.CalculatorRun2024;

        var calculatorRun = new CalculatorRun
        {
            Id = runContext.RunId,
            Name = runContext.RunName,
            CreatedBy = runContext.User,
            CreatedAt = new DateTime(2023, 1, 1),
            RelativeYear = runContext.RelativeYear,
            CalculatorRunOrganisationDataMaster = new CalculatorRunOrganisationDataMaster { CreatedBy = "", RelativeYear = runContext.RelativeYear, EffectiveFrom = new DateTime(2023, 1, 1), CreatedAt = new DateTime(2023, 1, 1) },
            CalculatorRunPomDataMaster = new CalculatorRunPomDataMaster { CreatedBy = "", RelativeYear = runContext.RelativeYear, EffectiveFrom = new DateTime(2023, 1, 1), CreatedAt = new DateTime(2023, 1, 1) },
            LapcapDataMaster = new LapcapDataMaster
            {
                LapcapFileName = "LapcapFile.csv",
                CreatedAt = new DateTime(2023, 1, 1),
                CreatedBy = "TestUser",
                RelativeYear = runContext.RelativeYear
            },
            DefaultParameterSettingMaster = new DefaultParameterSettingMaster
            {
                ParameterFileName = "Parameters.csv",
                CreatedAt = new DateTime(2023, 1, 1),
                CreatedBy = "TestUser",
                RelativeYear = runContext.RelativeYear
            }
        };

        dbContext.CalculatorRuns.Add(calculatorRun);
        await dbContext.SaveChangesAsync();


        var result = await testSubject.ConstructAsync(runContext);

        Assert.AreEqual(runContext.RunId, result.RunId);
        Assert.AreEqual(runContext.RunName, result.RunName);
        Assert.AreEqual(runContext.User, result.RunBy);
        Assert.AreEqual(new DateTime(2023, 1, 1), result.RunDate);
        Assert.AreEqual(runContext.RelativeYear, result.RelativeYear);
        Assert.AreEqual("01/01/2023 00:00", result.RpdFileORG);
        Assert.AreEqual("01/01/2023 00:00", result.RpdFilePOM);
        Assert.AreEqual("LapcapFile.csv,01/01/2023 00:00,TestUser", result.LapcapFile);
        Assert.AreEqual("Parameters.csv,01/01/2023 00:00,TestUser", result.ParametersFile);
    }

    [TestMethod]
    public async Task Construct_MissingOptionalProperties_ReturnsPartialData()
    {
        var runContext = TestDataHelper.CalculatorRun2025;

        var calculatorRun = new CalculatorRun
        {
            Id = runContext.RunId,
            Name = runContext.RunName,
            CreatedBy = runContext.User,
            CreatedAt = new DateTime(2024, 1, 1),
            RelativeYear = runContext.RelativeYear
        };

        dbContext.CalculatorRuns.Add(calculatorRun);
        await dbContext.SaveChangesAsync();

        var result = await testSubject.ConstructAsync(runContext);

        Assert.AreEqual(runContext.RunId, result.RunId);
        Assert.AreEqual(runContext.RunName, result.RunName);
        Assert.AreEqual(runContext.User, result.RunBy);
        Assert.AreEqual(new DateTime(2024, 1, 1), result.RunDate);
        Assert.AreEqual(runContext.RelativeYear, result.RelativeYear);
        Assert.IsTrue(string.IsNullOrEmpty(result.RpdFileORG));
        Assert.IsTrue(string.IsNullOrEmpty(result.RpdFilePOM));
        Assert.IsTrue(string.IsNullOrEmpty(result.LapcapFile));
        Assert.IsTrue(string.IsNullOrEmpty(result.ParametersFile));
    }
}
