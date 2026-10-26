using System.Text.Json.Serialization;

namespace EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;

public class BillingFileJson2025
{
    [JsonPropertyName("$schema")]
    public required string Schema { get; init; }

    [JsonPropertyName("runId")]
    public required int RunId { get; init; }

    [JsonPropertyName("financialYear")]
    public required string FinancialYear { get; init; }

    [JsonPropertyName("badDebtProvisionPercentage")]
    public required string BadDebtProvisionPercentage { get; init; }

    [JsonPropertyName("materials")]
    public required IEnumerable<MaterialPrices> Materials { get; init; }

    [JsonPropertyName("producers")]
    public required IEnumerable<ProducerResult> Producers { get; init; }
}
