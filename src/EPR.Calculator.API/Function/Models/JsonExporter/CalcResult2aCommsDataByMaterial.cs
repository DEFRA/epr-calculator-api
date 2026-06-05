using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResult2ACommsDataByMaterial
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "2a Comms Costs - by Material";

        [JsonPropertyName("calcResult2aCommsDataDetails")]
        public required IEnumerable<CalcResult2ACommsDataDetails> CalcResult2aCommsDataDetails {  get; set; }

        [JsonPropertyName("calcResult2aCommsDataDetailsTotal")]
        public required CalcResult2ACommsDataDetailsTotal CalcResult2aCommsDataDetailsTotal { get; set; }

        public static CalcResult2ACommsDataByMaterial From(IImmutableList<MaterialDetail> materials, Dictionary<string, CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial, CalcResultCommsCostCommsCostByMaterial total)
        {
            var commsByMaterialDataDetails = new List<CalcResult2ACommsDataDetails>();
            foreach (var item in commsCostByMaterial)
            {
                var material = materials.First(m => m.Code == item.Key);
                commsByMaterialDataDetails.Add(CalcResult2ACommsDataDetails.From(material, item.Value));
            }

            return new CalcResult2ACommsDataByMaterial
            {
                CalcResult2aCommsDataDetails      = commsByMaterialDataDetails,
                CalcResult2aCommsDataDetailsTotal = CalcResult2ACommsDataDetailsTotal.From(total)
            };
        }
    }

    public class CalcResult2ACommsDataDetails : BaseLaDisposalcostAnd2ACommsData
    {
        [JsonPropertyName("materialName")]
        public required string MaterialName { get; init; }

        [JsonPropertyName("englandCommsCost")]
        public required string EnglandCommsCost { get; init; }

        [JsonPropertyName("walesCommsCost")]
        public required string WalesCommsCost { get; init; }

        [JsonPropertyName("scotlandCommsCost")]
        public required string ScotlandCommsCost { get; init; }

        [JsonPropertyName("northernIrelandCommsCost")]
        public required string NorthernIrelandCommsCost { get; init; }

        [JsonPropertyName("totalCommsCost")]
        public required string TotalCommsCost { get; init; }

        [JsonPropertyName("commsCostByMaterialPricePerTonne")]
        public required string CommsCostByMaterialPricePerTonne { get; init; }

        public static CalcResult2ACommsDataDetails From(MaterialDetail material, CalcResultCommsCostCommsCostByMaterial commsCost)
        {
            return new CalcResult2ACommsDataDetails
            {
                MaterialName                           = material.Name,
                ProducerHouseholdPackagingWasteTonnage = Math.Round(commsCost.HouseholdPackagingWasteTonnage, 3),
                PublicBinTonnage                       = Math.Round(commsCost.PublicBinTonnage, 3),
                HouseholdDrinksContainersTonnage       = Math.Round(commsCost.HouseholdDrinksContainersTonnage, 3),
                TotalTonnage                           = Math.Round(commsCost.TotalTonnage, 3),
                CommsCostByMaterialPricePerTonne       = CurrencyConverterUtils.ConvertToCurrency(commsCost.PricePerTonne, precision : 4),
                EnglandCommsCost                       = CurrencyConverterUtils.ConvertToCurrency(commsCost.Cost.England),
                WalesCommsCost                         = CurrencyConverterUtils.ConvertToCurrency(commsCost.Cost.Wales),
                ScotlandCommsCost                      = CurrencyConverterUtils.ConvertToCurrency(commsCost.Cost.Scotland),
                NorthernIrelandCommsCost               = CurrencyConverterUtils.ConvertToCurrency(commsCost.Cost.NorthernIreland),
                TotalCommsCost                         = CurrencyConverterUtils.ConvertToCurrency(commsCost.Cost.Total),
                LateReportingTonnage                   = Math.Round(commsCost.LateReportingTonnage, 3)
            };
        }
    }
}
