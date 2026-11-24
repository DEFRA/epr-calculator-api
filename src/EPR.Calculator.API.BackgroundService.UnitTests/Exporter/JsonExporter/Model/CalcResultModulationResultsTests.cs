using System.Text.Json;
using EPR.Calculator.API.BackgroundService.Builder.Modulation;
using EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Exporter.JsonExporter.Model;

[TestClass]
public class CalcResultModulationResultsJsonTests
{
    [TestMethod]
    public void From_WithValidData_MapsAndSerializesCorrectly()
    {
        var result = CalcResultModulationResults.From(CalcResultModulationBuilder.Create(
            calculatorRunId: 1,
            redFactor: 1.25m,
            greenFactor: 0.75m,
            byMaterial: new Dictionary<MaterialDetail, ModulationDetail>()));

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
