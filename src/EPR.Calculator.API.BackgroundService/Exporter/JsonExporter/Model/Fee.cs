using System.Globalization;
using System.Text.Json.Serialization;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;

public record Fee
{
    [JsonPropertyName("base")]
    public required string Base { get; init; }

    [JsonPropertyName("badDebtProvision")]
    public required string BadDebtProvision { get; init; }

    public static Fee From(FeeWithBadDebt s) => new()
    {
        Base             = s.FeeWithoutBadDebt.ToString("F2", CultureInfo.InvariantCulture),
        BadDebtProvision = s.BadDebt.ToString("F2", CultureInfo.InvariantCulture),
    };
}
