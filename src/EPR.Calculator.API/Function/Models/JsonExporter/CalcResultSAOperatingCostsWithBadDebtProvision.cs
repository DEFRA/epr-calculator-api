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
                TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision ?? 0),
                BadDebtProvisionFor3 = CurrencyConverterUtils.ConvertToCurrency(costs?.BadDebtProvision ?? 0),
                TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision ?? 0),
                EnglandTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.EnglandTotalWithBadDebtProvision ?? 0),
                WalesTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.WalesTotalWithBadDebtProvision ?? 0),
                ScotlandTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.ScotlandTotalWithBadDebtProvision ?? 0),
                NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.NorthernIrelandTotalWithBadDebtProvision ?? 0)
            };
        }
    }
}
