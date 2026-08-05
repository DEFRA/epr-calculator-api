using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public record CalcResultCommsCostByMaterial2AJson
{
    [JsonPropertyName("materialBreakdown")]
    public required IEnumerable<CalcResultCommsCostByMaterial2AMaterialBreakdown> MaterialBreakdown { get; init; }

    public static CalcResultCommsCostByMaterial2AJson From(
        IReadOnlyDictionary<string, CommsFee> commsCostByMaterial,
        IImmutableList<MaterialDetail> materials)
    {
        IEnumerable<CalcResultCommsCostByMaterial2AMaterialBreakdown> GetMaterialBreakdown()
        {
            var materialBreakdown = new List<CalcResultCommsCostByMaterial2AMaterialBreakdown>();

            foreach (var item in commsCostByMaterial)
            {
                var material = materials.Single(m => m.Code == item.Key);

                var breakdown = CalcResultCommsCostByMaterial2AMaterialBreakdown.From(material.Name, item.Value);

                if (item.Key == MaterialCodes.Glass)
                {
                    breakdown.HouseholdDrinksContainersTonnageGlass = item.Value.HdcTonnage;
                }

                materialBreakdown.Add(breakdown);
            }

            return materialBreakdown;
        }

        return new CalcResultCommsCostByMaterial2AJson
        {
            MaterialBreakdown = GetMaterialBreakdown()
        };
    }
}

public record CalcResultCommsCostByMaterial2AMaterialBreakdown
{
    [JsonPropertyName("materialName")]
    public required string MaterialName { get; init; }

    [JsonPropertyName("householdPackagingWasteTonnage")]
    public decimal HouseholdPackagingWasteTonnage { get; init; }

    [JsonPropertyName("publicBinTonnage")]
    public decimal PublicBinTonnage { get; init; }

    [JsonPropertyName("totalTonnage")]
    public decimal TotalTonnage { get; init; }

    [JsonPropertyName("householdDrinksContainersTonnageGlass")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? HouseholdDrinksContainersTonnageGlass { get; set; }

    [JsonPropertyName("pricePerTonne")]
    public required string PricePerTonne { get; init; }

    [JsonPropertyName("producerTotalCostWithoutBadDebtProvision")]
    public required string ProducerTotalCostWithoutBadDebtProvision { get; init; }

    [JsonPropertyName("badDebtProvision")]
    public required string BadDebtProvision { get; init; }

    [JsonPropertyName("producerTotalCostWithBadDebtProvision")]
    public required string ProducerTotalCostwithBadDebtProvision { get; init; }

    [JsonPropertyName("englandWithBadDebtProvision")]
    public required string EnglandWithBadDebtProvision { get; init; }

    [JsonPropertyName("walesWithBadDebtProvision")]
    public required string WalesWithBadDebtProvision { get; init; }

    [JsonPropertyName("scotlandWithBadDebtProvision")]
    public required string ScotlandWithBadDebtProvision { get; init; }

    [JsonPropertyName("northernIrelandWithBadDebtProvision")]
    public required string NorthernIrelandWithBadDebtProvision { get; init; }

    public static CalcResultCommsCostByMaterial2AMaterialBreakdown From(string materialName, CommsFee item)
    {
        return new CalcResultCommsCostByMaterial2AMaterialBreakdown
        {
            MaterialName                             = materialName,
            HouseholdPackagingWasteTonnage           = item.HhTonnage,
            PublicBinTonnage                         = item.PbTonnage,
            TotalTonnage                             = item.TotalTonnage,
            PricePerTonne                            = FormatUtils.FormatCurrency(item.PricePerTonne, (int)DecimalPlaces.Four),
            ProducerTotalCostWithoutBadDebtProvision = FormatUtils.FormatCurrency(item.Costs.FeeWithoutBadDebt),
            BadDebtProvision                         = FormatUtils.FormatCurrency(item.Costs.BadDebt),
            ProducerTotalCostwithBadDebtProvision    = FormatUtils.FormatCurrency(item.Costs.ByCountry.Total),
            EnglandWithBadDebtProvision              = FormatUtils.FormatCurrency(item.Costs.ByCountry.England),
            WalesWithBadDebtProvision                = FormatUtils.FormatCurrency(item.Costs.ByCountry.Wales),
            ScotlandWithBadDebtProvision             = FormatUtils.FormatCurrency(item.Costs.ByCountry.Scotland),
            NorthernIrelandWithBadDebtProvision      = FormatUtils.FormatCurrency(item.Costs.ByCountry.NorthernIreland)
        };
    }
}
