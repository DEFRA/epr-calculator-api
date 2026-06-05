using System.Text.Json;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResultOnePlusFourApportionmentJsonTests
    {
        [TestMethod]
        public void OnePlusFourApportionmentJson_From()
        {
            var onePlusFourApportionment = TestDataHelper.GetCalcResultOnePlusFourApportionment();
            var result = CalcResultOnePlusFourApportionmentJson.From(onePlusFourApportionment);

            Assert.IsNotNull(result);

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);

            var expectedJson = """
                {
                "oneFeeForLADisposalCosts": {
                  "england"        : "£30.00",
                  "wales"          : "£5.00",
                  "scotland"       : "£15.00",
                  "northernIreland": "£35.00",
                  "total"          : "£85.00"
                },
                "fourLADataPrepCharge": {
                  "england"        : "£10.00",
                  "wales"          : "£5.00",
                  "scotland"       : "£0.00",
                  "northernIreland": "£0.00",
                  "total"          : "£15.00"
                },
                "totalOfonePlusFour": {
                  "england"        : "£40.00",
                  "wales"          : "£10.00",
                  "scotland"       : "£15.00",
                  "northernIreland": "£35.00",
                  "total"          : "£100.00"
                },
                "onePlusFourApportionmentPercentages": {
                  "england"        : "40.00000000%",
                  "wales"          : "10.00000000%",
                  "scotland"       : "15.00000000%",
                  "northernIreland": "35.00000000%",
                  "total"          : "100.00000000%"
                }
                }
                """;

            JsonTestUtils.AssertJson(expectedJson, json);
        }
    }
}
