using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.Builder;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter
{

    [TestClass]
    public class CalcResultsJsonExporterTests
    {
        private CalcResultsJsonExporter testClass;

        public CalcResultsJsonExporterTests()
        {
            testClass = new CalcResultsJsonExporter();
        }

        [TestMethod]
        public void Export_ShouldReturnJsonContent()
        {
            // Arrange
            var calcResult = CreateCalcResult();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var result = testClass.Export(calcResult, materials, new List<int> { 1, 2 });

            // Assert
            Assert.IsNotNull(result);
        }

        private static CalcResult CreateCalcResult()
        {
            return new CalcResult
            {
                ApplyModulation = true,
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 1,
                    RunDate = DateTime.UtcNow,
                    RunName = "CalculatorRunName",
                    RunBy = "Test user",
                    RelativeYear = new RelativeYear(2024),
                    RpdFileORG = "21/07/2017 17:32",
                    RpdFilePOM = "21/07/2017 17:32",
                    LapcapFile = "lapcap-data.csv,24/06/2025 10:00, test",
                    ParametersFile = "parameter-data.csv,24/06/2025 10:00, test"
                },
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    ByMaterial = new() {
                        ["AL"] = new ByCountryCost
                        {
                            England         = 13280.45m,
                            Wales           = 210.28m,
                            Scotland        = 91.00m,
                            NorthernIreland = 91.00m
                        }
                    }
                },
                CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
                {
                    ByMaterial = new Dictionary<string, CalcResultLateReportingTonnageDetail>
                    {
                        ["AL"] = new() { Red = 1000.00m, Amber = 2000.00m, Green = 5000.00m, Total = 8000.00m },
                        ["PL"] = new() { Red = 1000.00m, Amber =  500.00m, Green =  500.00m, Total = 2000.00m }
                    }
                },
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    BadDebtValue = 6m,
                    LaDataPrepCharge = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
                    SaOperatingCost = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
                    SchemeSetupCost = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 }
                },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment
                {
                    LaDisposalCost   = new() { England = 0.10M, Wales = 20M, Scotland = 0.15M, NorthernIreland = 0.15M },
                    LADataPrepCharge = new() { England = 0.10M, Wales = 20M, Scotland = 0.15M, NorthernIreland = 0.15M }
                },
                CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
                {
                    ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
                    {
                        ["AL"] =
                            new CalcResultLaDisposalCostDataDetail
                            {
                                Cost = ByCountryCost.Empty,
                                HouseholdPackagingWasteTonnage = 0m,
                                PublicBinTonnage = 0m,
                                HouseholdDrinkContainersTonnage = 0
                            },
                        ["PL"] =
                            new CalcResultLaDisposalCostDataDetail
                            {
                                Cost = ByCountryCost.Empty,
                                HouseholdPackagingWasteTonnage = 0,
                                PublicBinTonnage = 0,
                                HouseholdDrinkContainersTonnage = 0
                            },
                        ["GL"] =
                            new CalcResultLaDisposalCostDataDetail
                            {
                                Cost = ByCountryCost.Empty,
                                HouseholdPackagingWasteTonnage = 0,
                                PublicBinTonnage = 0,
                                HouseholdDrinkContainersTonnage = 0
                            }
                    },
                },
                CalcResultScaledupProducers = TestDataHelper.GetScaledupProducers(),
                CalcResultSummary = TestDataHelper.GetCalcResultSummary(),
                CalcResultPartialObligations = new CalcResultPartialObligations(),
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
                CalcResultModulation = new ModulationResult()
                {
                    GreenFactor = 1,
                    RedFactor = 2,
                    MaterialModulation = new Dictionary<MaterialDetail, MaterialModulation>()
                }
            };
        }
    }
}
