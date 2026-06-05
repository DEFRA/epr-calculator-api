using System.Text.Json;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResultParameterOtherCostJsonTests
    {
        [TestMethod]
        public void CalcResultParameterOtherCostJsonTests_CreateJson()
        {
            // Arrange
            var otherCost = new CalcResultParameterOtherCost
            {
                SaOperatingCost      = new() { England = 25000, Wales = 14000, Scotland = 17000, NorthernIreland = 9000 },
                LaDataPrepCharge     = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
                SchemeSetupCost      = new() { England = 17500, Wales = 23400, Scotland = 12400, NorthernIreland = 9450 },
                CountryApportionment = new() { England = 43.83561644m, Wales = 19.17808219m, Scotland = 24.65753425m, NorthernIreland = 12.32876712m },
                BadDebtValue = 6,
                MaterialityIncrease = new Materiality { Amount = 5000, Percentage = 2 },
                MaterialityDecrease = new Materiality { Amount = -1000, Percentage = -1 },
                TonnageChangeIncrease = new Materiality { Amount = 50, Percentage = 2 },
                TonnageChangeDecrease = new Materiality { Amount = -10, Percentage = -0.5m }
            };

            // Act
            var result = CalcResultParametersOtherJson.From(otherCost);


            // Assert
            Assert.IsNotNull(result);

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);

            var expectedJson = """
                {
                "threeSAOperatingCost": {
                  "england"        : "£25,000.00",
                  "wales"          : "£14,000.00",
                  "scotland"       : "£17,000.00",
                  "northernIreland": "£9,000.00",
                  "total"          : "£65,000.00"
                },
                "fourDataPreparationCharge": {
                  "england"        : "£40.00",
                  "wales"          : "£30.00",
                  "scotland"       : "£20.00",
                  "northernIreland": "£10.00",
                  "total"          : "£100.00"
                },
                "fourCountryApportionmentPercentages": {
                  "england"        : "43.83561644%",
                  "wales"          : "19.17808219%",
                  "scotland"       : "24.65753425%",
                  "northernIreland": "12.32876712%",
                  "total"          : "100.00000000%"
                },
                "fiveSchemeSetupCost": {
                  "england"        : "£17,500.00",
                  "wales"          : "£23,400.00",
                  "scotland"       : "£12,400.00",
                  "northernIreland": "£9,450.00",
                  "total"          : "£62,750.00"
                },
                "sixBadDebtProvision": {
                  "percentage": "6.00%"
                },
                "sevenMateriality": {
                  "increase": {
                    "amount"    : "£5,000.00",
                    "percentage": "2.00%"
                  },
                  "decrease": {
                    "amount"    : "-£1,000.00",
                    "percentage": "-1.00%"
                  }
                },
                "eightTonnageChange": {
                  "increase": {
                    "amount"    : "£50.00",
                    "percentage": "2.00%"
                  },
                  "decrease": {
                    "amount"    : "-£10.00",
                    "percentage": "-0.50%"
                  }
                }
                }
                """;
            JsonTestUtils.AssertJson(expectedJson, json);
        }
    }
}
