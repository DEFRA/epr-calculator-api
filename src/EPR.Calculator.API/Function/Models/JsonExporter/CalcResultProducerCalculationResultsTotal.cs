using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CalcResultProducerCalculationResultsTotal
    {
        [JsonPropertyName("producerCalculationResultsTotal")]
        public string? ProducerCalculationResultsTotal { get; set; }

        public static CalcResultProducerCalculationResultsTotal? From(CalcResultSummary calcResultSummary)
        {
            // specified in user story as remaining null
            return null;
        }
    }
}
