using System.Globalization;
using System.Text.Json.Serialization;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;

public record FeeWithCountries
{
    [JsonPropertyName("base")]
    public required string Base { get; init; }

    [JsonPropertyName("badDebtProvision")]
    public required string BadDebtProvision { get; init; }

    [JsonPropertyName("total")]
    public required string Total { get; init; }

    [JsonPropertyName("england")]
    public required string England { get; init; }

    [JsonPropertyName("wales")]
    public required string Wales { get; init; }

    [JsonPropertyName("scotland")]
    public required string Scotland { get; init; }

    [JsonPropertyName("northernIreland")]
    public required string NorthernIreland { get; init; }

    public static FeeWithCountries From(FeeWithBadDebt s) => new()
    {
        Base             = s.FeeWithoutBadDebt.ToString("F2", CultureInfo.InvariantCulture),
        BadDebtProvision = s.BadDebt.ToString("F2", CultureInfo.InvariantCulture),
        Total            = s.ByCountry.Total.ToString("F2", CultureInfo.InvariantCulture),
        England          = s.ByCountry.England.ToString("F2", CultureInfo.InvariantCulture),
        Wales            = s.ByCountry.Wales.ToString("F2", CultureInfo.InvariantCulture),
        Scotland         = s.ByCountry.Scotland.ToString("F2", CultureInfo.InvariantCulture),
        NorthernIreland  = s.ByCountry.NorthernIreland.ToString("F2", CultureInfo.InvariantCulture),
    };
}
