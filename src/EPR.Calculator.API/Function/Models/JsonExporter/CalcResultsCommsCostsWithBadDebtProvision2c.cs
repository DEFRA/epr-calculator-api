using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultsCommsCostsWithBadDebtProvision2C
    {
        [JsonPropertyName("totalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebProvisionFor2c")]        
        public string? BadDebtProvisionFor2c { get; set; }

        [JsonPropertyName("totalProducerFeeForCommsCostsByCountryWithBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision { get; set; }


        [JsonPropertyName("englandTotalWithBadDebtProvision")]
        public string? EnglandTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalWithBadDebtProvision")]
        public string? WalesTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalWithBadDebtProvision")]
        public string? ScotlandTotalWithBadDebtProvision { get; set; }
        
        [JsonPropertyName("northernIrelandTotalWithBadDebtProvision")]
        public string? NorthernIrelandTotalWithBadDebtProvision { get; set; }

        public static CalcResultsCommsCostsWithBadDebtProvision2C From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            return new CalcResultsCommsCostsWithBadDebtProvision2C
            {
                TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt),
                EnglandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCEnglandTotalWithBadDebt),
                NorthernIrelandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCNorthernIrelandTotalWithBadDebt),
                ScotlandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCScotlandTotalWithBadDebt),               
                BadDebtProvisionFor2c = CurrencyConverterUtils.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCBadDebtProvision),
                WalesTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCWalesTotalWithBadDebt),
                TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCTotalProducerFeeForCommsCostsWithBadDebt)
            };
        }

    }

}
