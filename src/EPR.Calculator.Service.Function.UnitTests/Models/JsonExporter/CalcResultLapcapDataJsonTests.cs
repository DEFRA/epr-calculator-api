using System.Text.Json;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResultLapcapDataJsonTests
    {
        [TestMethod]
        public void CalcResultLapcapDataJsonTests_CanCallFrom_WithValidData()
        {
            // Arrange
            var data = new CalcResultLapcapData
            {
                ByMaterial = new Dictionary<string, ByCountryCost>
                {
                    ["PC"] = new() { England = 50,  Wales = 60,  Scotland = 70,  NorthernIreland =  80 },
                    ["PL"] = new() { England = 100, Wales = 200, Scotland = 300, NorthernIreland = 400 }
                }
            };
            var materials = TestDataHelper.GetMaterialDetails();

            // Act
            var result = CalcResultLapcapDataJson.From(data, materials);


            // Assert
            Assert.IsNotNull(result);
            var details = result.CalcResultLapcapDataDetails.ToList();
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
            var expectedJson = """
                {
                  "name": "LAPCAP Data",
                  "calcResultLapcapDataDetails": [
                    {
                      "materialName"                 : "Paper or card",
                      "englandLaDisposalCost"        : "£50.00",
                      "walesLaDisposalCost"          : "£60.00",
                      "scotlandLaDisposalCost"       : "£70.00",
                      "northernIrelandLaDisposalCost": "£80.00",
                      "oneLaDisposalCostTotal"       : "£260.00"
                    },
                    {
                      "materialName"                 : "Plastic",
                      "englandLaDisposalCost"        : "£100.00",
                      "walesLaDisposalCost"          : "£200.00",
                      "scotlandLaDisposalCost"       : "£300.00",
                      "northernIrelandLaDisposalCost": "£400.00",
                      "oneLaDisposalCostTotal"       : "£1,000.00"
                    }
                  ],
                  "calcResultLapcapDataTotal": {
                    "totalEnglandLaDisposalCost"        : "£150.00",
                    "totalWalesLaDisposalCost"          : "£260.00",
                    "totalScotlandLaDisposalCost"       : "£370.00",
                    "totalNorthernIrelandLaDisposalCost": "£480.00",
                    "totalLaDisposalCost"               : "£1,260.00"
                  },
                  "oneCountryApportionmentPercentages": {
                    "englandApportionment"        : "11.90476190%",
                    "walesApportionment"          : "20.63492063%",
                    "scotlandApportionment"       : "29.36507937%",
                    "northernIrelandApportionment": "38.09523810%",
                    "totalApportionment"          : "100.00000000%"
                  }
                }
                """;
            JsonTestUtils.AssertJson(expectedJson, json);
        }
    }
}
