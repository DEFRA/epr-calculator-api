using System.Text.Json.Serialization;
using EPR.Calculator.API.BackgroundService.Converter;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;


public record CalcResultModulationResults
{
    [JsonPropertyName("redFactor")]
    public required decimal RedFactor { get; set; }

    [JsonPropertyName("greenDiscountFactor")]
    [JsonConverter(typeof(DecimalPrecision6Converter))]
    public required decimal GreenDiscountFactor { get; set; }

    public static CalcResultModulationResults From(ModulationResult modulation)
    {
        return new CalcResultModulationResults
        {
            RedFactor           = modulation.RedFactor,
            GreenDiscountFactor = modulation.GreenFactor
        };
    }
}
