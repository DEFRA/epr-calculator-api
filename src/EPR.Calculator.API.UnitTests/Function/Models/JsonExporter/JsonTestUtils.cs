using System.Text.Json;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter;

public static class JsonTestUtils
{
    public static void AssertJson(string expectedJson, string actualJson)
    {
        var opts = new JsonSerializerOptions { WriteIndented = true };
        string normExpected = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(expectedJson), opts);
        string normActual   = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(actualJson), opts);
        Assert.AreEqual(normExpected, normActual, $"{expectedJson}\nbut was\n{actualJson}");
    }
}
