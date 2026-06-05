using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1
    {
        [JsonPropertyName("totalProducerFeeForLADisposalCostsWithoutBadDebtProvision")]
        public required string TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebtProvisionForLADisposalCosts")]
        public required string BadDebtProvisionForLADisposalCosts { get; set; }

        [JsonPropertyName("totalProducerFeeForLADisposalCostsWithBadDebtProvision")]
        public required string TotalProducerFeeForLADisposalCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("englandTotalForLADisposalCostsWithBadDebtProvision")]
        public required string EnglandTotalForLADisposalCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalForLADisposalCostsWithBadDebtProvision")]
        public required string WalesTotalForLADisposalCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalForLADisposalCostsWithBadDebtProvision")]
        public required string ScotlandTotalForLADisposalCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("northernIrelandTotalForLADisposalCostsWithBadDebtProvision")]
        public required string NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision { get; set; }

        public static CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1 From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.LocalAuthorityDisposalCostsSectionOne;
            return new CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1
            {
                TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision),
                BadDebtProvisionForLADisposalCosts = CurrencyConverterUtils.ConvertToCurrency(costs?.BadDebtProvision),
                TotalProducerFeeForLADisposalCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision),
                EnglandTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.EnglandTotalWithBadDebtProvision),
                WalesTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.WalesTotalWithBadDebtProvision),
                ScotlandTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.ScotlandTotalWithBadDebtProvision),
                NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.NorthernIrelandTotalWithBadDebtProvision),
            };
        }
    }
}
