using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record TotalProducerBillWithBadDebtProvision
    {
        [JsonPropertyName("totalProducerBillWithoutBadDebtProvision")]
        public required string TotalProducerBillWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebtProvisionForTotalProducerBill")]
        public required string BadDebtProvisionForTotalProducerBill { get; set; }

        [JsonPropertyName("totalProducerBillWithBadDebtProvision")]
        public required string TotalProducerBillWithBadDebtProvisionAmount { get; set; }

        [JsonPropertyName("englandTotalForProducerBillWithBadDebtProvision")]
        public required string EnglandTotalForProducerBillWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalForProducerBillWithBadDebtProvision")]
        public required string WalesTotalForProducerBillWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalForProducerBillWithBadDebtProvision")]
        public required string ScotlandTotalForProducerBillWithBadDebtProvision { get; set; }

        [JsonPropertyName("northernIrelandTotalForProducerBillWithBadDebtProvision")]
        public required string NorthernIrelandTotalForProducerBillWithBadDebtProvision { get; set; }

        public static TotalProducerBillWithBadDebtProvision From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownCosts;
            return new TotalProducerBillWithBadDebtProvision
            {
                TotalProducerBillWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision ?? 0),
                BadDebtProvisionForTotalProducerBill = CurrencyConverterUtils.ConvertToCurrency(costs?.BadDebtProvision ?? 0),
                TotalProducerBillWithBadDebtProvisionAmount = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision ?? 0),
                EnglandTotalForProducerBillWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.EnglandTotalWithBadDebtProvision ?? 0),
                WalesTotalForProducerBillWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.WalesTotalWithBadDebtProvision ?? 0),
                ScotlandTotalForProducerBillWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.ScotlandTotalWithBadDebtProvision ?? 0),
                NorthernIrelandTotalForProducerBillWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.NorthernIrelandTotalWithBadDebtProvision ?? 0)
            };
        }
    }
}
