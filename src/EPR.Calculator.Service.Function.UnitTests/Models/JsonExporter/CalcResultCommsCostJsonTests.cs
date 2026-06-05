using System.Text.Json;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResultCommsCostJsonTests {

        [TestMethod]
        public void CalcResultCommsCost_From()
        {
            var data = TestDataHelper.GetCalcResultCommsCostReportDetail();

            var result = CalcResultCommsCostJson.From(data);
            Console.WriteLine(result.OnePlusFourCommsCostApportionmentPercentages);
            Console.WriteLine(result.OnePlusFourCommsCostApportionmentPercentages);
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
            var expectedJson = """
                {
                  "onePlusFourCommsCostApportionmentPercentages": {
                    "england"        : "50.23000000%",
                    "wales"          : "30.34000000%",
                    "scotland"       : "10.45000000%",
                    "northernIreland": "8.98000000%",
                    "total"          : "100.00000000%"
                  }
                }
                """;
            JsonTestUtils.AssertJson(expectedJson, json);
        }
    }
}
