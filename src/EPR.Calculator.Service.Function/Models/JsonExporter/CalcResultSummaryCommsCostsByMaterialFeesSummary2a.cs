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
                TotalProducerFeeForCommsCostsWithoutBadDebtProvision2a = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision),
                TotalBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.BadDebtProvision),
                TotalProducerFeeForCommsCostsWithBadDebtProvision2a = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision),
                EnglandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.EnglandTotalWithBadDebtProvision),
                WalesTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.WalesTotalWithBadDebtProvision),
                ScotlandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.ScotlandTotalWithBadDebtProvision),
                NorthernIrelandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.NorthernIrelandTotalWithBadDebtProvision)
            };
        }
    }
}
