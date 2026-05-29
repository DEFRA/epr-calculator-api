using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultSummaryCommsCostsByMaterialFeesSummary2A
    {
        [JsonPropertyName("totalProducerFeeForCommsCostsWithoutBadDebtProvision2a")]
        public required string  TotalProducerFeeForCommsCostsWithoutBadDebtProvision2a { get; set; }

        [JsonPropertyName("totalBadDebtProvision")]
        public required string TotalBadDebtProvision { get; set; }

        [JsonPropertyName("totalProducerFeeForCommsCostsWithBadDebtProvision2a")]
        public required string TotalProducerFeeForCommsCostsWithBadDebtProvision2a { get; set; }

        [JsonPropertyName("englandTotalWithBadDebtProvision")]
        public required string EnglandTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalWithBadDebtProvision")]
        public required string WalesTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalWithBadDebtProvision")]
        public required string ScotlandTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("northernIrelandTotalWithBadDebtProvision")]
        public required string NorthernIrelandTotalWithBadDebtProvision { get; set; }

        public static CalcResultSummaryCommsCostsByMaterialFeesSummary2A From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoA;
            return new CalcResultSummaryCommsCostsByMaterialFeesSummary2A
            {
                TotalProducerFeeForCommsCostsWithoutBadDebtProvision2a = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision ?? 0),
                TotalBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.BadDebtProvision ?? 0),
                TotalProducerFeeForCommsCostsWithBadDebtProvision2a = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision ?? 0),
                EnglandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.EnglandTotalWithBadDebtProvision ?? 0),
                WalesTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.WalesTotalWithBadDebtProvision ?? 0),
                ScotlandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.ScotlandTotalWithBadDebtProvision ?? 0),
                NorthernIrelandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.NorthernIrelandTotalWithBadDebtProvision ?? 0)
            };
        }
    }
}
