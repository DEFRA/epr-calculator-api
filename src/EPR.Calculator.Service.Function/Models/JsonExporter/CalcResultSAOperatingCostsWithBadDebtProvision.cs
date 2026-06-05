using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultSAOperatingCostsWithBadDebtProvision
    {

        [JsonPropertyName("totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision")]
        public required string  TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebProvisionFor3")]
        public required string BadDebtProvisionFor3 { get; set; }

        [JsonPropertyName("totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision")]
        public required string TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision { get; set; }

        [JsonPropertyName("englandTotalForSAOperatingCostsWithBadDebtProvision")]
        public required string EnglandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalForSAOperatingCostsWithBadDebtProvision")]
        public required string WalesTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalForSAOperatingCostsWithBadDebtProvision")]
        public required string ScotlandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("northernIrelandTotalForSAOperatingCostsWithBadDebtProvision")]
        public required string NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

         public static CalcResultSAOperatingCostsWithBadDebtProvision From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.SchemeAdministratorOperatingCosts;
            return new CalcResultSAOperatingCostsWithBadDebtProvision
            {
                TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision),
                BadDebtProvisionFor3 = CurrencyConverterUtils.ConvertToCurrency(costs?.BadDebtProvision),
                TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision),
                EnglandTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.EnglandTotalWithBadDebtProvision),
                WalesTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.WalesTotalWithBadDebtProvision),
                ScotlandTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.ScotlandTotalWithBadDebtProvision),
                NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.NorthernIrelandTotalWithBadDebtProvision)
            };
        }
    }
}
