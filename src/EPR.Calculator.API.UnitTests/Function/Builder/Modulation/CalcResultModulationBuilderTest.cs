using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Modulation
{
    [TestClass]
    public class CalcResultModulationBuilderTest
    {
        private CalcResultModulationBuilder builder;

        private static IImmutableList<MaterialDetail> materials = TestDataHelper.GetMaterials();

        private MaterialDetail al = materials.First(m => m.Code == "AL");
        private MaterialDetail fc = materials.First(m => m.Code == "FC");
        private MaterialDetail gl = materials.First(m => m.Code == "GL");
        private MaterialDetail pc = materials.First(m => m.Code == "PC");
        private MaterialDetail pl = materials.First(m => m.Code == "PL");
        private MaterialDetail st = materials.First(m => m.Code == "ST");
        private MaterialDetail wd = materials.First(m => m.Code == "WD");
        private MaterialDetail ot = materials.First(m => m.Code == "OT");

        private IReadOnlyDictionary<string, decimal> lateReportingTonnageDict;

        public CalcResultModulationBuilderTest()
        {
            builder = new CalcResultModulationBuilder();

            lateReportingTonnageDict = materials
                .SelectMany(m => new[]
                {
                    ($"LRET-{m.Code}-R", 1m),
                    ($"LRET-{m.Code}"  , 2m),
                    ($"LRET-{m.Code}-G", 3m)
                })
                .ToDictionary(t => t.Item1, t => t.Item2);
        }

        private CalcResultLaDisposalCostDataDetail mkLaDisposalCost(decimal costPerTonnage)
        {
            return new CalcResultLaDisposalCostDataDetail
            {
                Cost = ByCountryCost.Empty with { England = 100 * costPerTonnage },
                HouseholdPackagingWasteTonnage = 100,
                PublicBinTonnage = 0,
                HouseholdDrinkContainersTonnage = 0
            };
        }

        private SelfManagedConsumerWasteData mkProducerData(decimal red, decimal amber, decimal green)
        {
            return new SelfManagedConsumerWasteData
            {
                SelfManagedConsumerWasteTonnage = 0m,
                ActionedSelfManagedConsumerWasteTonnage = (total: null, red: null, amber: null, green: null),
                ResidualSelfManagedConsumerWasteTonnage = null,
                NetReportedTonnage = (total: null, red: red, amber: amber, green: green)
            };
        }

        private MaterialModulation mkMaterialModulation(decimal adc, decimal rdc, decimal gdc, decimal at, decimal rt, decimal gt, decimal rAtAdc, decimal gAtAdc)
        {
            return new MaterialModulation
            {
                RedMaterialDisposalCost   = rdc,
                AmberMaterialDisposalCost = adc,
                GreenMaterialDisposalCost = gdc,
                RedMaterialTonnages       = rt,
                AmberMaterialTonnages     = at,
                GreenMaterialTonnages     = gt,
                TotalRedMaterialAtAmberDisposalCost   = rAtAdc,
                TotalGreenMaterialAtAmberDisposalCost = gAtAdc
            };
        }

        [TestMethod]
        public async Task ModulationBuilder_TestCalculation()
        {
            var laDisposalCostData = new CalcResultLaDisposalCostData
            {
                ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
                {
                    [al.Code] = mkLaDisposalCost(100),
                    [fc.Code] = mkLaDisposalCost(130),
                    [gl.Code] = mkLaDisposalCost(150),
                    [pc.Code] = mkLaDisposalCost(200),
                    [pl.Code] = mkLaDisposalCost(250),
                    [st.Code] = mkLaDisposalCost(175),
                    [wd.Code] = mkLaDisposalCost(150),
                    [ot.Code] = mkLaDisposalCost(400)
                }
            };

            var smcw = new SelfManagedConsumerWaste
            {
                ProducerTotals = new List<ProducerSelfManagedConsumerWaste>(),
                OverallTotalPerMaterials = new Dictionary<string, SelfManagedConsumerWasteData>
                {
                    [al.Code] = mkProducerData(red:  220, amber:  330, green:  550),
                    [fc.Code] = mkProducerData(red:  275, amber:   55, green:   55),
                    [gl.Code] = mkProducerData(red:  110, amber:  220, green:  220),
                    [pc.Code] = mkProducerData(red:  400, amber: 1050, green: 2400),
                    [pl.Code] = mkProducerData(red: 2150, amber:  275, green:  270),
                    [st.Code] = mkProducerData(red:   33, amber:   40, green:   74),
                    [wd.Code] = mkProducerData(red:  265, amber:    0, green:    0),
                    [ot.Code] = mkProducerData(red:   30, amber:    0, green:    0)
                }
            };

            var redFactor = 1.2m;
            var defaultParameters = new Dictionary<string, decimal> { ["REDM-RF"] = redFactor }.Concat(lateReportingTonnageDict).ToDictionary(k => k.Key, v => v.Value);
            var modulationResults = await builder.ConstructAsync(defaultParameters, TestDataHelper.GetMaterials(), laDisposalCostData, smcw);
            //Console.WriteLine($">> {JsonConvert.SerializeObject(modulationResults, Formatting.Indented)}");

            Assert.AreEqual(     1.2m, modulationResults.RedFactor);
            Assert.AreEqual(0.772567m, modulationResults.GreenFactor);

            var expected =
                new Dictionary<MaterialDetail, MaterialModulation>
                {
                    [al] = mkMaterialModulation(100, 120,  77.2567m,  332,  221,  553,  22100,  55300),
                    [fc] = mkMaterialModulation(130, 156, 100.4337m,   57,  276,   58,  35880,   7540),
                    [gl] = mkMaterialModulation(150, 180, 115.8850m,  222,  111,  223,  16650,  33450),
                    [pc] = mkMaterialModulation(200, 240, 154.5134m, 1052,  401, 2403,  80200, 480600),
                    [pl] = mkMaterialModulation(250, 300, 193.1418m,  277, 2151,  273, 537750,  68250),
                    [st] = mkMaterialModulation(175, 210, 135.1992m,   42,   34,   77,   5950,  13475),
                    [wd] = mkMaterialModulation(150, 180, 115.8850m,    2,  266,    3,  39900,    450),
                    [ot] = mkMaterialModulation(400, 480, 309.0268m,    2,   31,    3,  12400,   1200)
                };

            CollectionAssert.AreEquivalent(expected.Keys.ToList(), modulationResults.MaterialModulation.Keys.ToList());

            foreach (var kvp in expected)
            {
                Assert.AreEqual(kvp.Value, modulationResults.MaterialModulation[kvp.Key], $"Value mismatch for key: {kvp.Key}");
            }
        }

                [TestMethod]
        public async Task ModulationBuilder_TestCalculationRounding()
        {
            var laDisposalCostData = new CalcResultLaDisposalCostData
            {
                ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
                {
                    [al.Code] = mkLaDisposalCost(0.1508m),
                    [fc.Code] = mkLaDisposalCost(0.0045m),
                    [gl.Code] = mkLaDisposalCost(0.4961m),
                    [pc.Code] = mkLaDisposalCost(0.5788m),
                    [pl.Code] = mkLaDisposalCost(0.0057m),
                    [st.Code] = mkLaDisposalCost(0.2118m),
                    [wd.Code] = mkLaDisposalCost(0.1134m),
                    [ot.Code] = mkLaDisposalCost(0.0039m)
                }
            };

            var smcw = new SelfManagedConsumerWaste
            {
                ProducerTotals = new List<ProducerSelfManagedConsumerWaste>(),
                OverallTotalPerMaterials = new Dictionary<string, SelfManagedConsumerWasteData>
                {
                    [al.Code] = mkProducerData(red:  96.000m, amber:  696175.000m, green: 50.000m),
                    [fc.Code] = mkProducerData(red: 101.000m, amber: 3838302.000m, green: 50.000m),
                    [gl.Code] = mkProducerData(red: 138.000m, amber: 9121268.500m, green: 72.000m),
                    [pc.Code] = mkProducerData(red: 121.000m, amber:   39046.000m, green: 50.000m),
                    [pl.Code] = mkProducerData(red: 131.000m, amber: 6376556.120m, green: 50.000m),
                    [st.Code] = mkProducerData(red: 141.000m, amber:   99915.100m, green: 50.000m),
                    [wd.Code] = mkProducerData(red: 151.000m, amber:  155059.900m, green: 50.000m),
                    [ot.Code] = mkProducerData(red: 161.000m, amber: 2645868.000m, green: 50.000m)
                }
            };

            var redFactor = 1.2m;
            var defaultParameters = new Dictionary<string, decimal> { ["REDM-RF"] = redFactor }.Concat(lateReportingTonnageDict).ToDictionary(k => k.Key, v => v.Value);
            var modulationResults = await builder.ConstructAsync(defaultParameters, TestDataHelper.GetMaterials(), laDisposalCostData, smcw);
            //Console.WriteLine($">> {JsonConvert.SerializeObject(modulationResults, Formatting.Indented)}");

            Assert.AreEqual(     1.2m, modulationResults.RedFactor);
            Assert.AreEqual(0.566720m, modulationResults.GreenFactor);

            var expected =
                new Dictionary<MaterialDetail, MaterialModulation>
                {
                    [al] = mkMaterialModulation(0.1508m, 0.1810m, 0.0855m,  696177.000m,  97.000m, 53.000m, 14.63m,  7.99m),
                    [fc] = mkMaterialModulation(0.0045m, 0.0054m, 0.0026m, 3838304.000m, 102.000m, 53.000m,  0.46m,  0.24m),
                    [gl] = mkMaterialModulation(0.4961m, 0.5953m, 0.2811m, 9121270.500m, 139.000m, 75.000m, 68.96m, 37.21m),
                    [pc] = mkMaterialModulation(0.5788m, 0.6946m, 0.3280m,   39048.000m, 122.000m, 53.000m, 70.61m, 30.68m),
                    [pl] = mkMaterialModulation(0.0057m, 0.0068m, 0.0032m, 6376558.120m, 132.000m, 53.000m,  0.75m,  0.30m),
                    [st] = mkMaterialModulation(0.2118m, 0.2542m, 0.1200m,   99917.100m, 142.000m, 53.000m, 30.08m, 11.23m),
                    [wd] = mkMaterialModulation(0.1134m, 0.1361m, 0.0643m,  155061.900m, 152.000m, 53.000m, 17.24m,  6.01m),
                    [ot] = mkMaterialModulation(0.0039m, 0.0047m, 0.0022m, 2645870.000m, 162.000m, 53.000m,  0.63m,  0.21m)
                };

            CollectionAssert.AreEquivalent(expected.Keys.ToList(), modulationResults.MaterialModulation.Keys.ToList());

            foreach (var kvp in expected)
            {
                Assert.AreEqual(kvp.Value, modulationResults.MaterialModulation[kvp.Key], $"Value mismatch for key: {kvp.Key}");
            }
        }

        [TestMethod]
        public async Task ModulationBuilder_Factor1()
        {
            var laDisposalCostData = new CalcResultLaDisposalCostData
            {
                ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
                {
                    [al.Code] = mkLaDisposalCost(100),
                    [fc.Code] = mkLaDisposalCost(130),
                    [gl.Code] = mkLaDisposalCost(150),
                    [pc.Code] = mkLaDisposalCost(200),
                    [pl.Code] = mkLaDisposalCost(250),
                    [st.Code] = mkLaDisposalCost(175),
                    [wd.Code] = mkLaDisposalCost(150),
                    [ot.Code] = mkLaDisposalCost(400)
                }
            };
            var smcw = new SelfManagedConsumerWaste
            {
                ProducerTotals = new List<ProducerSelfManagedConsumerWaste>(),
                OverallTotalPerMaterials = new Dictionary<string, SelfManagedConsumerWasteData>
                {
                    [al.Code] = mkProducerData(red:  220, amber:  330, green:  550),
                    [fc.Code] = mkProducerData(red:  275, amber:   55, green:   55),
                    [gl.Code] = mkProducerData(red:  110, amber:  220, green:  220),
                    [pc.Code] = mkProducerData(red:  400, amber: 1050, green: 2400),
                    [pl.Code] = mkProducerData(red: 2150, amber:  275, green:  270),
                    [st.Code] = mkProducerData(red:   33, amber:   40, green:   74),
                    [wd.Code] = mkProducerData(red:  265, amber:    0, green:    0),
                    [ot.Code] = mkProducerData(red:   30, amber:    0, green:    0)
                }
            };

            var redFactor = 1m;
            var defaultParameters = new Dictionary<string, decimal> { ["REDM-RF"] = redFactor }.Concat(lateReportingTonnageDict).ToDictionary(k => k.Key, v => v.Value);
            var modulationResults = await builder.ConstructAsync(defaultParameters, materials, laDisposalCostData, smcw);

            Assert.AreEqual(1m, modulationResults.RedFactor);
            Assert.AreEqual(1m, modulationResults.GreenFactor);
            foreach (var material in materials)
            {
                var cost = laDisposalCostData.ByMaterial[material.Code].DisposalCostPricePerTonne;

                var mm = modulationResults.MaterialModulation[material];
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
                    [al.Code] = mkLaDisposalCost(100),
                    [fc.Code] = mkLaDisposalCost(130),
                    [gl.Code] = mkLaDisposalCost(150),
                    [pc.Code] = mkLaDisposalCost(200),
                    [pl.Code] = mkLaDisposalCost(250),
                    [st.Code] = mkLaDisposalCost(175),
                    [wd.Code] = mkLaDisposalCost(150),
                    [ot.Code] = mkLaDisposalCost(400)
                }
            };
            var smcw = new SelfManagedConsumerWaste
            {
                ProducerTotals = new List<ProducerSelfManagedConsumerWaste>(),
                OverallTotalPerMaterials = new Dictionary<string, SelfManagedConsumerWasteData>
                {
                    [al.Code] = mkProducerData(red:  220, amber:  330, green: 0),
                    [fc.Code] = mkProducerData(red:  275, amber:   55, green: 0),
                    [gl.Code] = mkProducerData(red:  110, amber:  220, green: 0),
                    [pc.Code] = mkProducerData(red:  400, amber: 1050, green: 0),
                    [pl.Code] = mkProducerData(red: 2150, amber:  275, green: 0),
                    [st.Code] = mkProducerData(red:   33, amber:   40, green: 0),
                    [wd.Code] = mkProducerData(red:  265, amber:    0, green: 0),
                    [ot.Code] = mkProducerData(red:   30, amber:    0, green: 0)
                }
            };

            var redFactor = 1.2m;
            var lateReportingTonnageDict = materials
                .SelectMany(m => new[]
                {
                    ($"LRET-{m.Code}-R", 1m),
                    ($"LRET-{m.Code}"  , 2m),
                    ($"LRET-{m.Code}-G", 0m)
                })
                .ToDictionary(t => t.Item1, t => t.Item2);
            var defaultParameters = new Dictionary<string, decimal> { ["REDM-RF"] = redFactor }.Concat(lateReportingTonnageDict).ToDictionary(k => k.Key, v => v.Value);
            var modulationResults = await builder.ConstructAsync(defaultParameters, materials, laDisposalCostData, smcw);

            Assert.AreEqual(redFactor, modulationResults.RedFactor);
            Assert.AreEqual(1.0m, modulationResults.GreenFactor);
            foreach (var material in materials)
            {
                var cost = laDisposalCostData.ByMaterial[material.Code].DisposalCostPricePerTonne;

                var mm = modulationResults.MaterialModulation[material];
                Assert.AreEqual(cost, mm.AmberMaterialDisposalCost);
                Assert.AreEqual(cost * redFactor, mm.RedMaterialDisposalCost);
                Assert.AreEqual(cost, mm.AmberMaterialDisposalCost);
            }
        }
    }
}
