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
                TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision ?? 0),
                BadDebtProvisionFor5 = CurrencyConverterUtils.ConvertToCurrency(costs?.BadDebtProvision ?? 0),
                TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision ?? 0),
                EnglandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.EnglandTotalWithBadDebtProvision ?? 0),
                WalesTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.WalesTotalWithBadDebtProvision ?? 0),
                ScotlandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.ScotlandTotalWithBadDebtProvision ?? 0),
                NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.NorthernIrelandTotalWithBadDebtProvision ?? 0),
            };
        }
    }
}
