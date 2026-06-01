using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.UnitTests.Builder;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class BillingFileJsonTests
    {
        [TestMethod]
        public void From_MapFieldsCorrectly()
        {
            var calcResult = CreateCalcResult();
            var materials = TestDataHelper.GetMaterials();
            var acceptedProducerIds = new List<int> { 1, 2 };

            var result = BillingFileJson.From(calcResult, acceptedProducerIds, materials);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.CalcResultDetail);
            Assert.AreEqual(1, result.CalcResultDetail!.RunId);
            Assert.IsNotNull(result.CalcResultLapcapData);
            // Lapcap mapping assertions
            var lapcap = result.CalcResultLapcapData as CalcResultLapcapDataJson;
            Assert.IsNotNull(lapcap);
            Assert.IsNotNull(lapcap.CalcResultLapcapDataTotal);
            Assert.AreEqual("£13,672.73", lapcap.CalcResultLapcapDataTotal!.TotalLaDisposalCost);
            var ladetails = result.CalcResultLaDisposalCostData!.CalcResultLaDisposalCostDetails.ToList();
            Assert.IsTrue(ladetails.Any(d => d.DisposalCostPricePerTonne == "£20.0000"));
            Assert.IsNotNull(result.CalcResult2aCommsDataByMaterial);
            var comms = result.CalcResult2aCommsDataByMaterial!.CalcResult2aCommsDataDetails;
            Assert.IsTrue(comms.Any(d => d.MaterialName == "Aluminium"));
            var aluminium = comms.Single(d => d.MaterialName == "Aluminium");
            Assert.AreEqual("£0.4200", aluminium.CommsCostByMaterialPricePerTonne);
            Assert.IsNotNull(result.CalcResult2bCommsDataByUkWide);
            Assert.AreEqual("£1,500.00", result.CalcResult2bCommsDataByUkWide!.EnglandCommsCostUKWide);
            Assert.IsNotNull(result.CalcResult2cCommsDataByCountry);
            Assert.AreEqual("£250.00", result.CalcResult2cCommsDataByCountry.WalesCommsCostByCountry);
            Assert.IsNotNull(result.ParametersCommsCost);
            var onePlusFourPct = result.ParametersCommsCost!.OnePlusFourCommsCostApportionmentPercentages;
            Assert.IsNotNull(onePlusFourPct);
            Assert.AreEqual("50.23000000%", onePlusFourPct.England);
            Assert.AreEqual("30.34000000%", onePlusFourPct.Wales);
            Assert.IsNotNull(result.ScaleUpProducers!.ProducerSubmissions);
            var subs = result.ScaleUpProducers.ProducerSubmissions!.ToList();
            Assert.IsTrue(subs.Count >= 1);
            Assert.AreEqual(1, subs[0].ProducerId);
            var calcResults = result.CalculationResults as CalculationResultsJson;
            Assert.IsNotNull(calcResults);
            Assert.IsNotNull(calcResults.ProducerCalculationResultsSummary);
        }

        private CalcResult CreateCalcResult()
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
                        ["AL"] = new()
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
                    SaOperatingCost  = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
                    SchemeSetupCost  = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 }
                },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment
                {
                    LaDisposalCost   = new() { England = 0.10M, Wales = 20M, Scotland = 0.15M, NorthernIreland = 0.15M },
                    LADataPrepCharge = new() { England = 0.10M, Wales = 20M, Scotland = 0.15M, NorthernIreland = 0.15M },
                },
                CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
                {
                    ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
                    {
                        ["AL"] =
                            new CalcResultLaDisposalCostDataDetail
                            {
                                Cost = ByCountryCost.Empty with { England = 2000 },
                                HouseholdPackagingWasteTonnage = 100,
                                PublicBinTonnage               = 0,
                                HouseholdDrinkContainersTonnage = 0
                            },
                        ["PL"] =
                            new CalcResultLaDisposalCostDataDetail
                            {
                                Cost = ByCountryCost.Empty,
                                HouseholdPackagingWasteTonnage = 0,
                                PublicBinTonnage               = 0,
                                HouseholdDrinkContainersTonnage = 0
                            },
                        ["GL"] =
                            new CalcResultLaDisposalCostDataDetail
                            {
                                Cost = ByCountryCost.Empty,
                                HouseholdPackagingWasteTonnage = 0,
                                PublicBinTonnage               = 0,
                                HouseholdDrinkContainersTonnage = 0
                            },
                    }
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
