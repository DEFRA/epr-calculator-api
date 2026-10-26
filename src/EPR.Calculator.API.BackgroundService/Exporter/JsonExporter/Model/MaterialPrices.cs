using System.Globalization;
using System.Text.Json.Serialization;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;

public record MaterialPrices
{
    [JsonPropertyName("material")]
    public required string MaterialName { get; init; }

    [JsonPropertyName("disposalPricePerTonne")]
    public required StringOrRagPricePerTonne DisposalPricePerTonne { get; init; }

    [JsonPropertyName("commsPricePerTonne")]
    public required string CommsPricePerTonne { get; init; }

    private static string F4(decimal value) => value.ToString("F4", CultureInfo.InvariantCulture);

    public static IEnumerable<MaterialPrices> FromAll(IImmutableList<MaterialDetail> materials, CalcResult calcResult, bool applyModulation) =>
        materials.Select(m => new MaterialPrices
        {
            MaterialName          = m.Name,
            DisposalPricePerTonne = applyModulation ? ModulatedDisposalPrice(m, calcResult) : UnmodulatedDisposalPrice(m, calcResult),
            CommsPricePerTonne    = F4(calcResult.CalcResultCommsCostReportDetail.ByMaterial.GetValueOrDefault(m.Code)?.PricePerTonne ?? 0),
        });

    private static StringOrRagPricePerTonne UnmodulatedDisposalPrice(MaterialDetail material, CalcResult calcResult) =>
        F4(calcResult.CalcResultLaDisposalCostData.ByMaterial.GetValueOrDefault(material.Code)?.DisposalCostPricePerTonne ?? 0);

    private static StringOrRagPricePerTonne ModulatedDisposalPrice(MaterialDetail material, CalcResult calcResult)
    {
        // Match on code rather than MaterialDetail equality, as material names may have been overridden for display
        var mod = calcResult.CalcResultModulation?.ModulationByMaterial.FirstOrDefault(kv => kv.Key.Code == material.Code).Value;
        return new RagPricePerTonne
        {
            RedAndRedMedical     = F4(mod?.RedMaterialDisposalCost   ?? 0),
            AmberAndAmberMedical = F4(mod?.AmberMaterialDisposalCost ?? 0),
            GreenAndGreenMedical = F4(mod?.GreenMaterialDisposalCost ?? 0),
        };
    }
}

public class StringOrRagPricePerTonneConverter
    : ValueOrModulatedConverter<StringOrRagPricePerTonne, string, RagPricePerTonne> {}

[JsonConverter(typeof(StringOrRagPricePerTonneConverter))]
public record StringOrRagPricePerTonne
    : ValueOrModulated<string, RagPricePerTonne>
{
    public static implicit operator StringOrRagPricePerTonne(string value)
        => FromValue<StringOrRagPricePerTonne>(value);

    public static implicit operator StringOrRagPricePerTonne(RagPricePerTonne value)
        => FromModulated<StringOrRagPricePerTonne>(value);
}

public record RagPricePerTonne
{
    [JsonPropertyName("redAndRedMedical")]
    public required string RedAndRedMedical { get; init; }

    [JsonPropertyName("amberAndAmberMedical")]
    public required string AmberAndAmberMedical { get; init; }

    [JsonPropertyName("greenAndGreenMedical")]
    public required string GreenAndGreenMedical { get; init; }
}
