using System.Text.Json.Serialization;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public record CalcResultProducerCalculationResultsTotal
{
    [JsonPropertyName("producerCalculationResultsTotal")]
    public string? ProducerCalculationResultsTotal { get; set; }

    public static CalcResultProducerCalculationResultsTotal? From(ProducerFees producerFees)
    {
        // specified in user story as remaining null
        return null;
    }
}
