using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using System.Text.Json;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResultLaDisposalCostDataJsonTests
    {
        [TestMethod]
        public void From_ConvertsLaDisposalDetailsAndTotal()
        {
            var la = TestDataHelper.GetCalcResultLaDisposalCostData();
            var materials = TestDataHelper.GetMaterials();
            var result = CalcResultLaDisposalCostDataJson.From(la.ByMaterial, la.Total, materials, applyModulation: false);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.CalcResultLaDisposalCostDetails.Any());
            Assert.IsNotNull(result.CalcResultLaDisposalCostDataDetailsTotal);

            Assert.AreEqual(CommonConstants.LADisposalCostData, result.Name);
            Assert.AreEqual(63005.000m, result.CalcResultLaDisposalCostDataDetailsTotal.ProducerHouseholdPackagingWasteTonnageTotal);

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);

            var expectedJson = """
                {
                "name": "LA Disposal Cost Data",
                "calcResultLaDisposalCostDetails": [
                  {
                    "materialName"                          : "Aluminium",
                    "englandLaDisposalCost"                 : "£5,000.00",
                    "walesLaDisposalCost"                   : "£1,750.00",
                    "scotlandLaDisposalCost"                : "£2,000.00",
                    "northernIrelandLaDisposalCost"         : "£1,250.00",
                    "totalLaDisposalCost"                   : "£10,000.00",
                    "disposalCostPricePerTonne"             : "£0.5889",
                    "producerHouseholdPackagingWasteTonnage": 6980.000,
                    "publicBinTonnage"                      : 2000.000,
                    "householdDrinksContainersTonnage"      : 0,
                    "lateReportingTonnage"                  : 8000.000,
                    "totalTonnage"                          : 16980.000
                  },
                  {
                    "materialName"                          : "Fibre composite",
                    "englandLaDisposalCost"                 : "£7,500.00",
                    "walesLaDisposalCost"                   : "£2,100.00",
                    "scotlandLaDisposalCost"                : "£3,400.00",
                    "northernIrelandLaDisposalCost"         : "£1,750.00",
                    "totalLaDisposalCost"                   : "£14,750.00",
                    "disposalCostPricePerTonne"             : "£0.7074",
                    "producerHouseholdPackagingWasteTonnage": 11850.000,
                    "publicBinTonnage"                      : 2000.000,
                    "householdDrinksContainersTonnage"      : 0,
                    "lateReportingTonnage"                  : 7000.000,
                    "totalTonnage"                          : 20850.000
                  },
                  {
                    "materialName"                          : "Glass",
                    "englandLaDisposalCost"                 : "£45,000.00",
                    "walesLaDisposalCost"                   : "£0.00",
                    "scotlandLaDisposalCost"                : "£20,700.00",
                    "northernIrelandLaDisposalCost"         : "£4,500.00",
                    "totalLaDisposalCost"                   : "£70,200.00",
                    "disposalCostPricePerTonne"             : "£5.4000",
                    "producerHouseholdPackagingWasteTonnage": 4900.000,
                    "publicBinTonnage"                      : 2000.000,
                    "householdDrinksContainersTonnage"      : 100.000,
                    "lateReportingTonnage"                  : 6000.000,
                    "totalTonnage"                          : 13000.000
                  },
                  {
                    "materialName"                          : "Paper or card",
                    "englandLaDisposalCost"                 : "£12,500.00",
                    "walesLaDisposalCost"                   : "£2,300.00",
                    "scotlandLaDisposalCost"                : "£4,500.00",
                    "northernIrelandLaDisposalCost"         : "£3,400.00",
                    "totalLaDisposalCost"                   : "£22,700.00",
                    "disposalCostPricePerTonne"             : "£2.0142",
                    "producerHouseholdPackagingWasteTonnage": 4270.000,
                    "publicBinTonnage"                      : 2000.000,
                    "householdDrinksContainersTonnage"      : 0,
                    "lateReportingTonnage"                  : 5000.000,
                    "totalTonnage"                          : 11270.000
                  },
                  {
                    "materialName"                          : "Plastic",
                    "englandLaDisposalCost"                 : "£23,000.00",
                    "walesLaDisposalCost"                   : "£4,500.00",
                    "scotlandLaDisposalCost"                : "£6,700.00",
                    "northernIrelandLaDisposalCost"         : "£2,100.00",
                    "totalLaDisposalCost"                   : "£36,300.00",
                    "disposalCostPricePerTonne"             : "£1.9303",
                    "producerHouseholdPackagingWasteTonnage": 12805.000,
                    "publicBinTonnage"                      : 2000.000,
                    "householdDrinksContainersTonnage"      : 0,
                    "lateReportingTonnage"                  : 4000.000,
                    "totalTonnage"                          : 18805.000
                  },
                  {
                    "materialName"                          : "Steel",
                    "englandLaDisposalCost"                 : "£13,400.00",
                    "walesLaDisposalCost"                   : "£0.00",
                    "scotlandLaDisposalCost"                : "£7,800.00",
                    "northernIrelandLaDisposalCost"         : "£0.00",
                    "totalLaDisposalCost"                   : "£21,200.00",
                    "disposalCostPricePerTonne"             : "£1.6693",
                    "producerHouseholdPackagingWasteTonnage": 7700.000,
                    "publicBinTonnage"                      : 2000.000,
                    "householdDrinksContainersTonnage"      : 0,
                    "lateReportingTonnage"                  : 3000.000,
                    "totalTonnage"                          : 12700.000
                  },
                  {
                    "materialName"                          : "Wood",
                    "englandLaDisposalCost"                 : "£0.00",
                    "walesLaDisposalCost"                   : "£12,000.00",
                    "scotlandLaDisposalCost"                : "£0.00",
                    "northernIrelandLaDisposalCost"         : "£5,600.00",
                    "totalLaDisposalCost"                   : "£17,600.00",
                    "disposalCostPricePerTonne"             : "£1.6296",
                    "producerHouseholdPackagingWasteTonnage": 6800.000,
                    "publicBinTonnage"                      : 2000.000,
                    "householdDrinksContainersTonnage"      : 0,
                    "lateReportingTonnage"                  : 2000.000,
                    "totalTonnage"                          : 10800.000
                  },
                  {
                    "materialName"                          : "Other materials",
                    "englandLaDisposalCost"                 : "£3,400.00",
                    "walesLaDisposalCost"                   : "£2,100.00",
                    "scotlandLaDisposalCost"                : "£4,200.00",
                    "northernIrelandLaDisposalCost"         : "£700.00",
                    "totalLaDisposalCost"                   : "£10,400.00",
                    "disposalCostPricePerTonne"             : "£0.9720",
                    "producerHouseholdPackagingWasteTonnage": 7700.000,
                    "publicBinTonnage"                      : 2000.000,
                    "householdDrinksContainersTonnage"      : 0,
                    "lateReportingTonnage"                  : 1000.000,
                    "totalTonnage"                          : 10700.000
                  }
                ],
                "calcResultLaDisposalCostDataDetailsTotal": {
                  "total"                                      : "Total",
                  "englandLaDisposalCostTotal"                 : "£109,800.00",
                  "walesLaDisposalCostTotal"                   : "£24,750.00",
                  "scotlandLaDisposalCostTotal"                : "£49,300.00",
                  "northernIrelandLaDisposalCostTotal"         : "£19,300.00",
                  "totalLaDisposalCostTotal"                   : "£203,150.00",
                  "producerHouseholdPackagingWasteTonnageTotal": 63005.000,
                  "publicBinTonnage"                           : 16000.000,
                  "householdDrinksContainersTonnageTotal"      : 100.000,
                  "lateReportingTonnageTotal"                  : 36000.000,
                  "totalTonnageTotal"                          : 115105.000
                }
                }
                """;

            JsonTestUtils.AssertJson(expectedJson, json);
        }
    }
}
