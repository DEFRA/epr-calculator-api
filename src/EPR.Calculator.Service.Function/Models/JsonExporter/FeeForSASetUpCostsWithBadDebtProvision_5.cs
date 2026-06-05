using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class FeeForSaSetUpCostsWithBadDebtProvision5
    {
        [JsonPropertyName("totalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision")]
        public string? TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebtProvisionFor5")]
        public string? BadDebtProvisionFor5 { get; set; }

        [JsonPropertyName("totalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision")]
        public string? TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("englandTotalForSASetUpCostsWithBadDebtProvision")]
        public string? EnglandTotalForSASetUpCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalForSASetUpCostsWithBadDebtProvision")]
        public string? WalesTotalForSASetUpCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalForSASetUpCostsWithBadDebtProvision")]
        public string? ScotlandTotalForSASetUpCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("northernIrelandTotalForSASetUpCostsWithBadDebtProvision")]
        public string? NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision { get; set; }

        public static FeeForSaSetUpCostsWithBadDebtProvision5 From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.OneOffSchemeAdministrationSetupCosts;
            return new FeeForSaSetUpCostsWithBadDebtProvision5
            {
                TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision),
                BadDebtProvisionFor5 = CurrencyConverterUtils.ConvertToCurrency(costs?.BadDebtProvision),
                TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision),
                EnglandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.EnglandTotalWithBadDebtProvision),
                WalesTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.WalesTotalWithBadDebtProvision),
                ScotlandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.ScotlandTotalWithBadDebtProvision),
                NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.NorthernIrelandTotalWithBadDebtProvision)
            };
        }
    }
}
