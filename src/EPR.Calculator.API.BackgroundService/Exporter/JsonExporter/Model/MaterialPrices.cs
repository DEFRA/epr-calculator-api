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
    public required RagPricePerTonne DisposalPricePerTonne { get; init; }

    [JsonPropertyName("commsPricePerTonne")]
    public required string CommsPricePerTonne { get; init; }

    public static IEnumerable<MaterialPrices> FromAll(IImmutableList<MaterialDetail> materials, CalcResult calcResult) =>
        materials.Select(m =>
        {
            decimal? red = null, amber = null, green = null;
            // Match on code rather than MaterialDetail equality, as material names may have been overridden for display
            var mod = calcResult.CalcResultModulation?.ModulationByMaterial.FirstOrDefault(kv => kv.Key.Code == m.Code).Value;
            if (mod is not null)
            {
                red   = mod.RedMaterialDisposalCost;
                amber = mod.AmberMaterialDisposalCost;
                green = mod.GreenMaterialDisposalCost;
            }
            var commsDetail = calcResult.CalcResultCommsCostReportDetail.ByMaterial.GetValueOrDefault(m.Code);
            return new MaterialPrices
            {
                MaterialName = m.Name,
                DisposalPricePerTonne = new RagPricePerTonne
                {
                    RedAndRedMedical     = (red   ?? 0).ToString("F4", CultureInfo.InvariantCulture),
                    AmberAndAmberMedical = (amber ?? 0).ToString("F4", CultureInfo.InvariantCulture),
                    GreenAndGreenMedical = (green ?? 0).ToString("F4", CultureInfo.InvariantCulture),
                },
                CommsPricePerTonne = (commsDetail?.PricePerTonne ?? 0).ToString("F4", CultureInfo.InvariantCulture),
            };
        });
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
