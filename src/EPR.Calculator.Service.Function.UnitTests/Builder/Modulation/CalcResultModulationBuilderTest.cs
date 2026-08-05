using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Features.CalculatorRuns.Contexts;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Modulation;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultModulationBuilderTest
{
    private static readonly IImmutableList<MaterialDetail> materials = TestDataHelper.GetMaterialDetails();

    private readonly MaterialDetail al = materials.First(m => m.Code == "AL");
    private readonly CalcResultModulationBuilder builder;
    private readonly MaterialDetail fc = materials.First(m => m.Code == "FC");
    private readonly MaterialDetail gl = materials.First(m => m.Code == "GL");

    private readonly MaterialDetail ot = materials.First(m => m.Code == "OT");
    private readonly MaterialDetail pc = materials.First(m => m.Code == "PC");
    private readonly MaterialDetail pl = materials.First(m => m.Code == "PL");
    private readonly MaterialDetail st = materials.First(m => m.Code == "ST");
    private readonly MaterialDetail wd = materials.First(m => m.Code == "WD");

    public CalcResultModulationBuilderTest()
    {
        builder = new CalcResultModulationBuilder();
    }

    private static CalcResultLaDisposalCostDataDetail MkLaDisposalCost(decimal costPerTonnage) =>
        new()
        {
            Cost = ByCountryCost.Empty with { England = 100 * costPerTonnage },
            HouseholdPackagingWasteTonnage = 100,
            PublicBinTonnage = 0,
            HouseholdDrinkContainersTonnage = 0
        };

    private static SelfManagedConsumerWasteData MkSmcw(decimal red, decimal amber, decimal green)
    {
        return new SelfManagedConsumerWasteData
        {
            SmcwTonnage = 0m,
            ActionedSmcwTonnage = new RamTonnageGroup { Total = null, Red = null, Amber = null, Green = null },
            ResidualSmcwTonnage = null,
            NetTonnage = new RamTonnageGroup { Total = null, Red = red, Amber = amber, Green = green }
        };
    }

    private static ModulationDetail MkModulationDetail(decimal adc, decimal rdc, decimal gdc, decimal at, decimal rt, decimal gt, decimal rAtAdc, decimal gAtAdc)
    {
        return new ModulationDetail
        {
            RedMaterialDisposalCost = rdc,
            AmberMaterialDisposalCost = adc,
            GreenMaterialDisposalCost = gdc,
            RedMaterialTonnages = rt,
            AmberMaterialTonnages = at,
            GreenMaterialTonnages = gt,
            TotalRedMaterialAtAmberDisposalCost = rAtAdc,
            TotalGreenMaterialAtAmberDisposalCost = gAtAdc
        };
    }

    private static CalculatorRunContext WithModulationParameters(
        CalculatorRunContext run,
        decimal redFactor,
        decimal lateRed,
        decimal lateAmber,
        decimal lateGreen)
    {
        return run with
        {
            DefaultParameters = run.DefaultParameters with
            {
                RedModulationFactor = redFactor,

                LateReportingTonnageByMaterialCode = run.DefaultParameters
                    .LateReportingTonnageByMaterialCode
                    .ToDictionary(
                        x => x.Key,
                        _ => new RamTonnageGroup
                        {
                            Red = lateRed,
                            Amber = lateAmber,
                            Green = lateGreen,
                            Total = lateRed + lateAmber + lateGreen
                        })
            }
        };
    }

    [TestMethod]
    public async Task ModulationBuilder_TestCalculation()
    {
        var laDisposalCostData = new CalcResultLaDisposalCostData
        {
            ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
            {
                [al.Code] = MkLaDisposalCost(100),
                [fc.Code] = MkLaDisposalCost(130),
                [gl.Code] = MkLaDisposalCost(150),
                [pc.Code] = MkLaDisposalCost(200),
                [pl.Code] = MkLaDisposalCost(250),
                [st.Code] = MkLaDisposalCost(175),
                [wd.Code] = MkLaDisposalCost(150),
                [ot.Code] = MkLaDisposalCost(400)
            }
        };

        var smcw = new SelfManagedConsumerWaste
        {
            CalculatorRunId = 1,
            ProducerTotals = [],
            TotalByMaterial = new Dictionary<string, SelfManagedConsumerWasteData>
            {
                [al.Code] = MkSmcw(220, 330, 550),
                [fc.Code] = MkSmcw(275, 55, 55),
                [gl.Code] = MkSmcw(110, 220, 220),
                [pc.Code] = MkSmcw(400, 1050, 2400),
                [pl.Code] = MkSmcw(2150, 275, 270),
                [st.Code] = MkSmcw(33, 40, 74),
                [wd.Code] = MkSmcw(265, 0, 0),
                [ot.Code] = MkSmcw(30, 0, 0)
            }
        };

        var redFactor = 1.2m;

        var defaultParameters = TestDataHelper.CalculatorRun2026.DefaultParameters;

        var runContext = TestDataHelper.CalculatorRun2026 with
        {
            DefaultParameters = defaultParameters with
            {
                RedModulationFactor = redFactor,

                LateReportingTonnageByMaterialCode = defaultParameters
                    .LateReportingTonnageByMaterialCode
                    .ToDictionary(
                        x => x.Key,
                        x => new RamTonnageGroup
                        {
                            Red   = 1,
                            Amber = 2,
                            Green = 3,
                            Total = 6
                        })
            }
        };

        var modulationResults = await builder.ConstructAsync(runContext, TestDataHelper.GetMaterialDetails(), laDisposalCostData, smcw);
        //Console.WriteLine($">> {JsonConvert.SerializeObject(modulationResults, Formatting.Indented)}");

        Assert.AreEqual(1.2m, modulationResults.RedFactor);
        Assert.AreEqual(0.772567m, modulationResults.GreenFactor);

        var expected =
            new Dictionary<MaterialDetail, ModulationDetail>
            {
                [al] = MkModulationDetail(100, 120, 77.2567m, 332, 221, 553, 22100, 55300),
                [fc] = MkModulationDetail(130, 156, 100.4337m, 57, 276, 58, 35880, 7540),
                [gl] = MkModulationDetail(150, 180, 115.8851m, 222, 111, 223, 16650, 33450),
                [pc] = MkModulationDetail(200, 240, 154.5134m, 1052, 401, 2403, 80200, 480600),
                [pl] = MkModulationDetail(250, 300, 193.1418m, 277, 2151, 273, 537750, 68250),
                [st] = MkModulationDetail(175, 210, 135.1992m, 42, 34, 77, 5950, 13475),
                [wd] = MkModulationDetail(150, 180, 115.8851m, 2, 266, 3, 39900, 450),
                [ot] = MkModulationDetail(400, 480, 309.0268m, 2, 31, 3, 12400, 1200)
            };

        CollectionAssert.AreEquivalent(expected.Keys.ToList(), modulationResults.ModulationByMaterial.Keys.ToList());

        foreach (var kvp in expected)
            Assert.AreEqual(kvp.Value, modulationResults.ModulationByMaterial[kvp.Key], $"Value mismatch for key: {kvp.Key}");
    }

    [TestMethod]
    public async Task ModulationBuilder_TestCalculationRounding()
    {
        var laDisposalCostData = new CalcResultLaDisposalCostData
        {
            ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
            {
                [al.Code] = MkLaDisposalCost(0.1508m),
                [fc.Code] = MkLaDisposalCost(0.0045m),
                [gl.Code] = MkLaDisposalCost(0.4961m),
                [pc.Code] = MkLaDisposalCost(0.5788m),
                [pl.Code] = MkLaDisposalCost(0.0057m),
                [st.Code] = MkLaDisposalCost(0.2118m),
                [wd.Code] = MkLaDisposalCost(0.1134m),
                [ot.Code] = MkLaDisposalCost(0.0039m)
            }
        };

        var smcw = new SelfManagedConsumerWaste
        {
            CalculatorRunId = 1,
            ProducerTotals = new List<ProducerSelfManagedConsumerWaste>(),
            TotalByMaterial = new Dictionary<string, SelfManagedConsumerWasteData>
            {
                [al.Code] = MkSmcw(96.000m, 696175.000m, 50.000m),
                [fc.Code] = MkSmcw(101.000m, 3838302.000m, 50.000m),
                [gl.Code] = MkSmcw(138.000m, 9121268.500m, 72.000m),
                [pc.Code] = MkSmcw(121.000m, 39046.000m, 50.000m),
                [pl.Code] = MkSmcw(131.000m, 6376556.120m, 50.000m),
                [st.Code] = MkSmcw(141.000m, 99915.100m, 50.000m),
                [wd.Code] = MkSmcw(151.000m, 155059.900m, 50.000m),
                [ot.Code] = MkSmcw(161.000m, 2645868.000m, 50.000m)
            }
        };

        var redFactor = 1.2m;

        var runContext = WithModulationParameters(
            TestDataHelper.CalculatorRun2026,
            redFactor,
            lateRed: 1,
            lateAmber: 2,
            lateGreen: 3);

        var modulationResults = await builder.ConstructAsync(runContext, TestDataHelper.GetMaterialDetails(), laDisposalCostData, smcw);
        //Console.WriteLine($">> {JsonConvert.SerializeObject(modulationResults, Formatting.Indented)}");

        Assert.AreEqual(1.2m, modulationResults.RedFactor);
        Assert.AreEqual(0.566720m, modulationResults.GreenFactor);

        var expected =
            new Dictionary<MaterialDetail, ModulationDetail>
            {
                [al] = MkModulationDetail(0.1508m, 0.1810m, 0.0855m, 696177.000m, 97.000m, 53.000m, 14.63m, 7.99m),
                [fc] = MkModulationDetail(0.0045m, 0.0054m, 0.0026m, 3838304.000m, 102.000m, 53.000m, 0.46m, 0.24m),
                [gl] = MkModulationDetail(0.4961m, 0.5953m, 0.2811m, 9121270.500m, 139.000m, 75.000m, 68.96m, 37.21m),
                [pc] = MkModulationDetail(0.5788m, 0.6946m, 0.3280m, 39048.000m, 122.000m, 53.000m, 70.61m, 30.68m),
                [pl] = MkModulationDetail(0.0057m, 0.0068m, 0.0032m, 6376558.120m, 132.000m, 53.000m, 0.75m, 0.30m),
                [st] = MkModulationDetail(0.2118m, 0.2542m, 0.1200m, 99917.100m, 142.000m, 53.000m, 30.08m, 11.23m),
                [wd] = MkModulationDetail(0.1134m, 0.1361m, 0.0643m, 155061.900m, 152.000m, 53.000m, 17.24m, 6.01m),
                [ot] = MkModulationDetail(0.0039m, 0.0047m, 0.0022m, 2645870.000m, 162.000m, 53.000m, 0.63m, 0.21m)
            };

        CollectionAssert.AreEquivalent(expected.Keys.ToList(), modulationResults.ModulationByMaterial.Keys.ToList());

        foreach (var kvp in expected)
            Assert.AreEqual(kvp.Value, modulationResults.ModulationByMaterial[kvp.Key], $"Value mismatch for key: {kvp.Key}");
    }

    [TestMethod]
    public async Task ModulationBuilder_Factor1()
    {
        var laDisposalCostData = new CalcResultLaDisposalCostData
        {
            ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
            {
                [al.Code] = MkLaDisposalCost(100),
                [fc.Code] = MkLaDisposalCost(130),
                [gl.Code] = MkLaDisposalCost(150),
                [pc.Code] = MkLaDisposalCost(200),
                [pl.Code] = MkLaDisposalCost(250),
                [st.Code] = MkLaDisposalCost(175),
                [wd.Code] = MkLaDisposalCost(150),
                [ot.Code] = MkLaDisposalCost(400)
            }
        };
        var smcw = new SelfManagedConsumerWaste
        {
            CalculatorRunId = 1,
            ProducerTotals = new List<ProducerSelfManagedConsumerWaste>(),
            TotalByMaterial = new Dictionary<string, SelfManagedConsumerWasteData>
            {
                [al.Code] = MkSmcw(220, 330, 550),
                [fc.Code] = MkSmcw(275, 55, 55),
                [gl.Code] = MkSmcw(110, 220, 220),
                [pc.Code] = MkSmcw(400, 1050, 2400),
                [pl.Code] = MkSmcw(2150, 275, 270),
                [st.Code] = MkSmcw(33, 40, 74),
                [wd.Code] = MkSmcw(265, 0, 0),
                [ot.Code] = MkSmcw(30, 0, 0)
            }
        };

        var redFactor = 1m;

        var runContext = WithModulationParameters(
            TestDataHelper.CalculatorRun2026,
            redFactor,
            lateRed: 1,
            lateAmber: 2,
            lateGreen: 3);

        var modulationResults = await builder.ConstructAsync(runContext, materials, laDisposalCostData, smcw);

        Assert.AreEqual(1m, modulationResults.RedFactor);
        Assert.AreEqual(1m, modulationResults.GreenFactor);
        foreach (var material in materials)
        {
            var cost = laDisposalCostData.ByMaterial[material.Code].DisposalCostPricePerTonne;

            var mm = modulationResults.ModulationByMaterial[material];
            Assert.AreEqual(cost, mm.AmberMaterialDisposalCost);
            Assert.AreEqual(cost, mm.RedMaterialDisposalCost);
            Assert.AreEqual(cost, mm.GreenMaterialDisposalCost);
        }
    }

    [TestMethod]
    public async Task ModulationBuilder_NoGreen()
    {
        var laDisposalCostData = new CalcResultLaDisposalCostData
        {
            ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
            {
                [al.Code] = MkLaDisposalCost(100),
                [fc.Code] = MkLaDisposalCost(130),
                [gl.Code] = MkLaDisposalCost(150),
                [pc.Code] = MkLaDisposalCost(200),
                [pl.Code] = MkLaDisposalCost(250),
                [st.Code] = MkLaDisposalCost(175),
                [wd.Code] = MkLaDisposalCost(150),
                [ot.Code] = MkLaDisposalCost(400)
            }
        };
        var smcw = new SelfManagedConsumerWaste
        {
            CalculatorRunId = 1,
            ProducerTotals = new List<ProducerSelfManagedConsumerWaste>(),
            TotalByMaterial = new Dictionary<string, SelfManagedConsumerWasteData>
            {
                [al.Code] = MkSmcw(220, 330, 0),
                [fc.Code] = MkSmcw(275, 55, 0),
                [gl.Code] = MkSmcw(110, 220, 0),
                [pc.Code] = MkSmcw(400, 1050, 0),
                [pl.Code] = MkSmcw(2150, 275, 0),
                [st.Code] = MkSmcw(33, 40, 0),
                [wd.Code] = MkSmcw(265, 0, 0),
                [ot.Code] = MkSmcw(30, 0, 0)
            }
        };

        var redFactor = 1.2m;

        var runContext = WithModulationParameters(
            TestDataHelper.CalculatorRun2026,
            redFactor,
            lateRed: 1,
            lateAmber: 2,
            lateGreen: 0);

        var modulationResults = await builder.ConstructAsync(runContext, materials, laDisposalCostData, smcw);

        Assert.AreEqual(redFactor, modulationResults.RedFactor);
        Assert.AreEqual(1.0m, modulationResults.GreenFactor);
        foreach (var material in materials)
        {
            var cost = laDisposalCostData.ByMaterial[material.Code].DisposalCostPricePerTonne;

            var mm = modulationResults.ModulationByMaterial[material];
            Assert.AreEqual(cost, mm.AmberMaterialDisposalCost);
            Assert.AreEqual(cost * redFactor, mm.RedMaterialDisposalCost);
            Assert.AreEqual(cost, mm.AmberMaterialDisposalCost);
        }
    }
}
