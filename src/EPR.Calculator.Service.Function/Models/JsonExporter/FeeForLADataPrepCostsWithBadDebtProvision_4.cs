using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    [SuppressMessage("Naming", "S101:Types should be named in PascalCase", Justification = "Required for JSON contract")]
    public class FeeForLADataPrepCostsWithBadDebtProvision_4
    {
        [JsonPropertyName("totalProducerFeeForLADataPrepCostsWithoutBadDebtProvision")]
        public required string TotalProducerFeeForLADataPrepCostsWithoutBadDebtProvision {get; set;}

        [JsonPropertyName("badDebtProvisionFor4")]
        public required string BadDebtProvisionFor4 {get; set;}

        [JsonPropertyName("totalProducerFeeForLADataPrepCostsWithBadDebtProvision")]
        public required string TotalProducerFeeForLADataPrepCostsWithBadDebtProvision {get; set;}

        [JsonPropertyName("englandTotalForLADataPrepCostsWithBadDebtProvision")]
        public required string EnglandTotalForLADataPrepCostsWithBadDebtProvision {get; set;}

        [JsonPropertyName("walesTotalForLADataPrepCostsWithBadDebtProvision")]
        public required string WalesTotalForLADataPrepCostsWithBadDebtProvision {get; set;}

        [JsonPropertyName("scotlandTotalForLADataPrepCostsWithBadDebtProvision")]
        public required string ScotlandTotalForLADataPrepCostsWithBadDebtProvision {get; set;}

        [JsonPropertyName("northernIrelandTotalForLADataPrepCostsWithBadDebtProvision")]
        public required string NorthernIrelandTotalForLADataPrepCostsWithBadDebtProvision { get; set;}

        public static FeeForLADataPrepCostsWithBadDebtProvision_4 From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.LocalAuthorityDataPreparationCosts;
            return new FeeForLADataPrepCostsWithBadDebtProvision_4
            {
                TotalProducerFeeForLADataPrepCostsWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision),
                BadDebtProvisionFor4 = CurrencyConverterUtils.ConvertToCurrency(costs?.BadDebtProvision),
                TotalProducerFeeForLADataPrepCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision),
                EnglandTotalForLADataPrepCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.EnglandTotalWithBadDebtProvision),
                WalesTotalForLADataPrepCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.WalesTotalWithBadDebtProvision),
                ScotlandTotalForLADataPrepCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.ScotlandTotalWithBadDebtProvision),
                NorthernIrelandTotalForLADataPrepCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.NorthernIrelandTotalWithBadDebtProvision)
            };
        }
    }
}
