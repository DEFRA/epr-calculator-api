using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2A
    {
        [JsonPropertyName("totalProducerFeeForCommsCostsWithoutBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebtProvisionFor2a")]
        public string? BadDebtProvisionFor2a { get; set; }

        [JsonPropertyName("totalProducerFeeForCommsCostsWithBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("englandTotalWithBadDebtProvision")]
        public string? EnglandTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalWithBadDebtProvision")]
        public string? WalesTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalWithBadDebtProvision")]
        public string? ScotlandTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("northernIrelandTotalWithBadDebtProvision")]
        public string? NorthernIrelandTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("percentageOfProducerTonnageVsAllProducers")]
        public string? PercentageOfProducerTonnageVsAllProducers { get; set; }

        public static CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2A From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoA;
            return new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2A
            {
                TotalProducerFeeForCommsCostsWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision ?? 0),
                BadDebtProvisionFor2a = CurrencyConverterUtils.ConvertToCurrency(costs?.BadDebtProvision ?? 0),
                TotalProducerFeeForCommsCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision ?? 0),
                EnglandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.EnglandTotalWithBadDebtProvision ?? 0),
                WalesTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.WalesTotalWithBadDebtProvision ?? 0),
                ScotlandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.ScotlandTotalWithBadDebtProvision ?? 0),
                NorthernIrelandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.NorthernIrelandTotalWithBadDebtProvision ?? 0),
                PercentageOfProducerTonnageVsAllProducers = $"{Math.Round(calcResultSummaryProducerDisposalFees.PercentageofProducerReportedTonnagevsAllProducers, (int)DecimalPlaces.Eight).ToString()}%"
            };
        }
    }
}
