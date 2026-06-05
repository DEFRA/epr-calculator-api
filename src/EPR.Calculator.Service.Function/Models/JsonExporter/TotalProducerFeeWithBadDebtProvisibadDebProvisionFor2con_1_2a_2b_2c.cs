using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class TotalProducerFeeWithBadDebtProvisionFor2Con12A2B2CMapper
    {
        [JsonPropertyName("totalFeeWithBadDebtProvision")]
        public required string TotalFeeWithBadDebtProvision { get; set; }

        [JsonPropertyName("producerPercentageOfOverallProducerCost")]
        public required string ProducerPercentageOfOverallProducerCost { get; set; }

        public static TotalProducerFeeWithBadDebtProvisionFor2Con12A2B2CMapper From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            return new TotalProducerFeeWithBadDebtProvisionFor2Con12A2B2CMapper
            {
                TotalFeeWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(calcResultSummaryProducerDisposalFees.ProducerTotalOnePlus2A2B2CWithBadDeptProvision),
                ProducerPercentageOfOverallProducerCost = $"{calcResultSummaryProducerDisposalFees.ProducerOverallPercentageOfCostsForOnePlus2A2B2C.ToString("F8")}%"
            };

        }
    }
}
