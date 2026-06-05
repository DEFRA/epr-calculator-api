using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultLateReportingBuilderTest : TestsFor<CalcResultLateReportingBuilder>
{
    private static readonly IImmutableList<MaterialDetail> Materials = ImmutableList.Create(
        new MaterialDetail { Id = 1, Code = "AL", Name = "Aluminium" },
        new MaterialDetail { Id = 2, Code = "FC", Name = "Fibre composite" }
    );

    private readonly RunContext runContext = TestDataHelper.CalculatorRun2025;

    protected override void TestInitialize()
    {
        var calculatorRuns = new List<CalculatorRun>
        {
            new()
            {
                Id = runContext.RunId,
                RelativeYear = runContext.RelativeYear,
                Name = runContext.RunName,
                DefaultParameterSettingMasterId = 1,
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User",
                LapcapDataMasterId = 2
            }
        };

        var defaultParameterSettings = new List<DefaultParameterSettingMaster>
        {
            new() { Id = 1, RelativeYear = new RelativeYear(2024) }
        };

        var defaultParameterTemplateMasterList = new List<DefaultParameterTemplateMaster>
        {
            new() { ParameterUniqueReferenceId = "1", ParameterType = "Late Reporting Tonnage", ParameterCategory = "Aluminium-R" },
            new() { ParameterUniqueReferenceId = "2", ParameterType = "Late Reporting Tonnage", ParameterCategory = "Aluminium-A" },
            new() { ParameterUniqueReferenceId = "3", ParameterType = "Late Reporting Tonnage", ParameterCategory = "Aluminium-G" },
            new() { ParameterUniqueReferenceId = "4", ParameterType = "Late Reporting Tonnage", ParameterCategory = "Fibre composite-R" },
            new() { ParameterUniqueReferenceId = "5", ParameterType = "Late Reporting Tonnage", ParameterCategory = "Fibre composite-A" },
            new() { ParameterUniqueReferenceId = "6", ParameterType = "Late Reporting Tonnage", ParameterCategory = "Fibre composite-G" }
        };

        var defaultParameterSettingDetails = new List<DefaultParameterSettingDetail>
        {
            new() { DefaultParameterSettingMasterId = 1, ParameterUniqueReferenceId = "1", ParameterValue = 100, DefaultParameterSettingMaster = defaultParameterSettings[0] },
            new() { DefaultParameterSettingMasterId = 1, ParameterUniqueReferenceId = "2", ParameterValue = 200, DefaultParameterSettingMaster = defaultParameterSettings[0] },
            new() { DefaultParameterSettingMasterId = 1, ParameterUniqueReferenceId = "3", ParameterValue = 300, DefaultParameterSettingMaster = defaultParameterSettings[0] },
            new() { DefaultParameterSettingMasterId = 1, ParameterUniqueReferenceId = "4", ParameterValue = 400, DefaultParameterSettingMaster = defaultParameterSettings[0] },
            new() { DefaultParameterSettingMasterId = 1, ParameterUniqueReferenceId = "5", ParameterValue = 500, DefaultParameterSettingMaster = defaultParameterSettings[0] },
            new() { DefaultParameterSettingMasterId = 1, ParameterUniqueReferenceId = "6", ParameterValue = 600, DefaultParameterSettingMaster = defaultParameterSettings[0] }
        };

        dbContext.CalculatorRuns.AddRange(calculatorRuns);
        dbContext.DefaultParameterSettings.AddRange(defaultParameterSettings);
        dbContext.DefaultParameterSettingDetail.AddRange(defaultParameterSettingDetails);
        dbContext.DefaultParameterTemplateMasterList.AddRange(defaultParameterTemplateMasterList);
        dbContext.SaveChanges();
    }

    [TestMethod]
    public async Task Construct_ShouldReturnCorrectResults()
    {
        var result = await testSubject.ConstructAsync(runContext, Materials);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ByMaterial.Count);

        var aluminium = result.ByMaterial["AL"];
        Assert.AreEqual(100m, aluminium.Red);
        Assert.AreEqual(200m, aluminium.Amber);
        Assert.AreEqual(300m, aluminium.Green);
        Assert.AreEqual(600m, aluminium.Total);

        var fibre = result.ByMaterial["FC"];
        Assert.AreEqual(400m, fibre.Red);
        Assert.AreEqual(500m, fibre.Amber);
        Assert.AreEqual(600m, fibre.Green);
        Assert.AreEqual(1500m, fibre.Total);

        var total = result.Total;
        Assert.AreEqual(500m, total.Red);
        Assert.AreEqual(700m, total.Amber);
        Assert.AreEqual(900m, total.Green);
        Assert.AreEqual(2100m, total.Total);
    }
}
