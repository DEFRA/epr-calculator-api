using System.Text.Json;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResultModulationResultsJsonTests
    {
        [TestMethod]
        public void From_WithValidData_MapsAndSerializesCorrectly()
        {
            var result = CalcResultModulationResults.From(new ModulationResult
            {
                RedFactor = 1.25m,
                GreenFactor = 0.75m,
                MaterialModulation = new Dictionary<MaterialDetail, MaterialModulation>()
            });

            var actualJson = JsonSerializer.Serialize(result);

            var expectedJson = """
            {
                "redFactor": 1.25,
                "greenDiscountFactor": 0.750000
            }
            """;

            JsonTestUtils.AssertJson(expectedJson, actualJson);
        }
    }
}
