using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Converter;
using EPR.Calculator.Service.Function.Builder.Modulation;


namespace EPR.Calculator.Service.Function.Models.JsonExporter
{

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
}
