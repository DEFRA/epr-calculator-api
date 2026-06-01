using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B
    {
        [JsonPropertyName("totalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebtProvisionFor2b")]
        public string? BadDebtProvisionFor2b { get; set; }

        [JsonPropertyName("totalProducerFeeForCommsCostsUKWideWithBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision { get; set; }

        [JsonPropertyName("englandTotalWithBadDebtProvision")]
        public string? EnglandTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalWithBadDebtProvision")]
        public string? WalesTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalWithBadDebtProvision")]
        public string? ScotlandTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("northernIrelandTotalWithBadDebtProvision")]
        public string? NorthernIrelandTotalWithBadDebtProvision { get; set; }

        public static CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoB;
            return new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B
            {
                TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision ?? 0),
                BadDebtProvisionFor2b = CurrencyConverterUtils.ConvertToCurrency(costs?.BadDebtProvision ?? 0),
                TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision ?? 0),
                EnglandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.EnglandTotalWithBadDebtProvision ?? 0),
                WalesTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.WalesTotalWithBadDebtProvision ?? 0),
                ScotlandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.ScotlandTotalWithBadDebtProvision ?? 0),
                NorthernIrelandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.NorthernIrelandTotalWithBadDebtProvision ?? 0)
            };
        }
    }
}
