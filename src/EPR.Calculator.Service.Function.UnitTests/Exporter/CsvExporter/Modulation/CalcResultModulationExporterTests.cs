using System.Text;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Modulation;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Modulation
{
    [TestClass]
    public class CalcResultModulationExporterTests
    {
        private CalcResultModulationExporter exporter;

        private MaterialDetail al = TestDataHelper.GetMaterialDetails().First(m => m.Code == "AL");
        private MaterialDetail fc = TestDataHelper.GetMaterialDetails().First(m => m.Code == "FC");

        public CalcResultModulationExporterTests()
        {
            exporter = new CalcResultModulationExporter();
        }

        [TestMethod]
        public void Modulation_Export_CSV()
        {
            // Arrange
            var calcResultLaDisposalCostData = new CalcResultLaDisposalCostData
            {
                ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
                {
                    [al.Code] = new CalcResultLaDisposalCostDataDetail
                    {
                        Cost = ByCountryCost.Empty,
                        HouseholdPackagingWasteTonnage = 26181753.110m,
                        PublicBinTonnage = 24610.429m,
                        HouseholdDrinkContainersTonnage = 0,
                        LateReportingTonnage = 3500
                    },
                    [fc.Code] = new CalcResultLaDisposalCostDataDetail
                    {
                        Cost = ByCountryCost.Empty,
                        HouseholdPackagingWasteTonnage = 401772.341m,
                        PublicBinTonnage = 1146.546m,
                        HouseholdDrinkContainersTonnage = 0,
                        LateReportingTonnage = 789
                    }
                }
            };

            SelfManagedConsumerWasteData mkSmcwData(decimal netR, decimal netA, decimal netG, decimal? actionedSmcwR, decimal? actionedSmcwA, decimal? actionedSmcwG, decimal? residualSmcw)
            {
                return new SelfManagedConsumerWasteData
                {
                    SelfManagedConsumerWasteTonnage = 0m,
                    ActionedSelfManagedConsumerWasteTonnage = (total: (actionedSmcwR + actionedSmcwA + actionedSmcwG), red: actionedSmcwR, amber: actionedSmcwA, green: actionedSmcwG),
                    ResidualSelfManagedConsumerWasteTonnage = residualSmcw,
                    NetReportedTonnage = (total: (netR + netA + netG), red: netR, amber: netA, green: netG)
                };
            };

            var smcw = new SelfManagedConsumerWaste
            {
                ProducerTotals = new List<ProducerSelfManagedConsumerWaste>(),
                OverallTotalPerMaterials = new Dictionary<string, SelfManagedConsumerWasteData>
                {
                    [al.Code] = mkSmcwData(netR: 21000000.123m, netA: 5000000.234m, netG: 209863.182m, actionedSmcwR: 1000, actionedSmcwA: 2000, actionedSmcwG: 3000, residualSmcw: 654.321m),
                    [fc.Code] = mkSmcwData(netR:     3001.333m, netA:  400000.222m, netG:    706.332m, actionedSmcwR: null, actionedSmcwA: null, actionedSmcwG: null, residualSmcw: null    )
                }
            };

            var modulationResult = new ModulationResult
            {
                RedFactor = 1.2m,
                GreenFactor = 0.751106m,
                MaterialModulation = new Dictionary<MaterialDetail, MaterialModulation>
                {
                    [al] = new MaterialModulation
                    {
                        AmberMaterialDisposalCost = 0.631234m,
                        RedMaterialDisposalCost   = 0.761234m,
                        GreenMaterialDisposalCost = 0.471234m,
                        AmberMaterialTonnages = 5000000.23m,
                        RedMaterialTonnages   = 21000000.12m,
                        GreenMaterialTonnages = 209863.18m,
                        TotalRedMaterialAtAmberDisposalCost   = 6280704.41m,
                        TotalGreenMaterialAtAmberDisposalCost = 4955297.80m
                    },
                    [fc] = new MaterialModulation
                    {
                        AmberMaterialDisposalCost =  96.591234m,
                        RedMaterialDisposalCost   = 115.901234m,
                        GreenMaterialDisposalCost =  72.551234m,
                        AmberMaterialTonnages  = 400000.22m,
                        RedMaterialTonnages   = 3001.33m,
                        GreenMaterialTonnages = 706.33m,
                        TotalRedMaterialAtAmberDisposalCost   = 14817325.23m,
                        TotalGreenMaterialAtAmberDisposalCost = 11697888.16m
                    }
                }
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(calcResultLaDisposalCostData, smcw, modulationResult, csvContent);
            var result = csvContent.ToString().ReplaceLineEndings("\n").Split("\n").Select(s => s.TrimEnd(',')).ToArray();
            //Console.WriteLine($">> {JsonConvert.SerializeObject(result, Formatting.Indented)}");
            Console.WriteLine(string.Join("\n", result));

            var expected = new[] {
                new string[] {},
                new string[] {},
                new[] { "Modulation Calculation" },
                new[] { "Red Modulation Factor", "1.200" },
                new[] { "Green Modulation Factor", "0.751106" },
                new[]
                {
                    "Material",
                    "Producer Household Packaging Tonnage",
                    "Public Bin Tonnage",
                    "Household Drinks Containers Tonnage",
                    "Late Reporting Tonnage",
                    "Actioned Self-Managed Consumer Waste",
                    "Net Tonnage + Late Reporting Tonnage",
                    "Red + Red Medical Net Tonnage + Late Reporting Tonnage",
                    "Amber + Amber Medical Net Tonnage + Late Reporting Tonnage",
                    "Green + Green Medical Net Tonnage + Late Reporting Tonnage",
                    "Total Red Material at Amber Disposal Cost = Amber Material Disposal Cost x Red Material Tonnage",
                    "Total Green Material at Amber Disposal Cost = Amber Material Disposal Cost x Green Material Tonnage",
                    "Red Material Disposal Cost = Red Modulation Factor * Amber Material Disposal Cost",
                    "Amber Material Disposal Cost = Material Disposal Cost per Tonne",
                    "Green Material Disposal Cost = Green Modulation Factor * Amber Material Disposal Cost"
                },
                new[] { "Aluminium"      , "26181753.110", "24610.429", null   , "3500.000", "6000.000", "26209863.530", "21000000.120", "5000000.230", "209863.180",  "£6280704.41",  "£4955297.80",   "£0.7612",  "£0.6312",  "£0.4712" },
                new[] { "Fibre composite",   "401772.341",  "1146.546", null   ,  "789.000",   "0.000",   "403707.880",     "3001.330",  "400000.220",    "706.330", "£14817325.23", "£11697888.16", "£115.9012", "£96.5912", "£72.5512" },
                new[] { "Total"          , "26583525.451", "25756.975", "0.000", "4289.000", "6000.000", "26613571.410", "21003001.450", "5400000.450", "210569.510", "£21098029.64", "£16653185.96" },
                new string[] { }
            };

            CsvTestUtils.AssertCsv(expected, result);
        }
    }
}
