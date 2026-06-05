using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CalcResultCommsCostByMaterial2AJson
    {
        [JsonPropertyName("materialBreakdown")]
        public required IEnumerable<CalcResultCommsCostByMaterial2AMaterialBreakdown> MaterialBreakdown { get; init; }

        public static CalcResultCommsCostByMaterial2AJson From(
            Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostByMaterial,
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
                        breakdown.HouseholdDrinksContainersTonnageGlass = item.Value.HouseholdDrinksContainersTonnage;
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

        public static CalcResultCommsCostByMaterial2AMaterialBreakdown From(string materialName, CalcResultSummaryProducerCommsFeesCostByMaterial item)
        {
            return new CalcResultCommsCostByMaterial2AMaterialBreakdown
            {
                MaterialName = materialName,
                HouseholdPackagingWasteTonnage = item.HouseholdPackagingWasteTonnage,
                PublicBinTonnage = item.PublicBinTonnage,
                TotalTonnage = item.TotalReportedTonnage,
                PricePerTonne = CurrencyConverterUtils.ConvertToCurrency(item.PriceperTonne, (int)DecimalPlaces.Four),
                ProducerTotalCostWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(item.ProducerTotalCostWithoutBadDebtProvision),
                BadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(item.BadDebtProvision),
                ProducerTotalCostwithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(item.ProducerTotalCostwithBadDebtProvision),
                EnglandWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(item.EnglandWithBadDebtProvision),
                WalesWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(item.WalesWithBadDebtProvision),
                ScotlandWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(item.ScotlandWithBadDebtProvision),
                NorthernIrelandWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(item.NorthernIrelandWithBadDebtProvision)
            };
        }
    }
}
