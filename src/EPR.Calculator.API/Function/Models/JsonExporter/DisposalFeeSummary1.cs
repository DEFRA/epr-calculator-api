using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class DisposalFeeSummary1
    {
        [JsonPropertyName("totalProducerDisposalFeeWithoutBadDebtProvision")]
        public required string TotalProducerDisposalFeeWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebtProvision")]
        public required string BadDebtProvision { get; set; }

        [JsonPropertyName("totalProducerDisposalFeeWithBadDebtProvision")]
        public required string TotalProducerDisposalFeeWithBadDebtProvision { get; set; }

        [JsonPropertyName("englandTotal")]
        public required string EnglandTotal { get; set; }

        [JsonPropertyName("walesTotal")]
        public required string WalesTotal { get; set; }

        [JsonPropertyName("scotlandTotal")]
        public required string ScotlandTotal { get; set; }

        [JsonPropertyName("northernIrelandTotal")]
        public required string NorthernIrelandTotal { get; set; }

        [JsonPropertyName("tonnageChangeCount")]
        public required string TonnageChangeCount { get; set; }

        [JsonPropertyName("tonnageChangeAdvice")]
        public required string TonnageChangeAdvice { get; set; }

        public static DisposalFeeSummary1 From(CalcResultSummaryProducerDisposalFees summary)
        {
            return new DisposalFeeSummary1
            {
                TotalProducerDisposalFeeWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(summary.TotalProducerDisposalFee),
                BadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(summary.BadDebtProvision),
                TotalProducerDisposalFeeWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(summary.TotalProducerDisposalFeeWithBadDebtProvision),
                EnglandTotal = CurrencyConverterUtils.ConvertToCurrency(summary.EnglandTotal),
                WalesTotal = CurrencyConverterUtils.ConvertToCurrency(summary.WalesTotal),
                ScotlandTotal = CurrencyConverterUtils.ConvertToCurrency(summary.ScotlandTotal),
                NorthernIrelandTotal = CurrencyConverterUtils.ConvertToCurrency(summary.NorthernIrelandTotal),
                TonnageChangeCount = summary.TonnageChangeCount ?? CommonConstants.Hyphen,
                TonnageChangeAdvice = summary.TonnageChangeAdvice ?? CommonConstants.Hyphen,
            };
        }
    }
}
