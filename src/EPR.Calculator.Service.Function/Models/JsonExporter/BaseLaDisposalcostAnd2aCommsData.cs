using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Converter;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class BaseLaDisposalcostAnd2ACommsData
    {
        [JsonPropertyName("producerHouseholdPackagingWasteTonnage")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public required decimal ProducerHouseholdPackagingWasteTonnage { get; init; }

        [JsonPropertyName("publicBinTonnage")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public required decimal PublicBinTonnage { get; init; }

        [JsonPropertyName("householdDrinksContainersTonnage")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public required decimal HouseholdDrinksContainersTonnage { get; init; }

        [JsonPropertyName("lateReportingTonnage")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public required decimal LateReportingTonnage { get; init; }

        [JsonPropertyName("totalTonnage")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public required decimal TotalTonnage { get; init; }
    }
}
