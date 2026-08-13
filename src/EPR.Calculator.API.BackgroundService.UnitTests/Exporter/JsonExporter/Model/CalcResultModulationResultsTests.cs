using System.Text.Json;
using EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Exporter.JsonExporter.Model;

[TestClass]
public class CalcResultModulationResultsJsonTests
{
    [TestMethod]
    public void From_WithValidData_MapsAndSerializesCorrectly()
    {
        var result = CalcResultModulationResults.From(new ModulationResult
        {
            CalculatorRunId = 1,
            RedFactor = 1.25m,
            GreenFactor = 0.75m,
            ModulationByMaterial = new Dictionary<MaterialDetail, ModulationDetail>()
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
