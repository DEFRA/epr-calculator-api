using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
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

    [TestMethod]
    public async Task Construct_ShouldReturnCorrectResults()
    {
        var runContext = TestDataHelper.CalculatorRun2025;
        runContext = runContext with
        {
            DefaultParameters = runContext.DefaultParameters with
            {
                LateReportingTonnageByMaterialCode =
                    new Dictionary<string, RamTonnageGroup>()
                    {
                        ["AL"] = new RamTonnageGroup
                        {
                            Red   = 100m,
                            Amber = 200m,
                            Green = 300m,
                            Total = 600m
                        },
                        ["FC"] = new RamTonnageGroup
                        {
                            Red   = 400m,
                            Amber = 500m,
                            Green = 600m,
                            Total = 1500m
                        }
                    }
            }
        };

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
