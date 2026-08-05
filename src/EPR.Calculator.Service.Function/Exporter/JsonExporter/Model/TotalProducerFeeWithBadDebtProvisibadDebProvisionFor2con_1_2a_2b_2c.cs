using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class TotalProducerFeeWithBadDebtProvisionFor2Con12A2B2CMapper
{
    [JsonPropertyName("totalFeeWithBadDebtProvision")]
    public required string TotalFeeWithBadDebtProvision { get; set; }

    [JsonPropertyName("producerPercentageOfOverallProducerCost")]
    public required string ProducerPercentageOfOverallProducerCost { get; set; }

    public static TotalProducerFeeWithBadDebtProvisionFor2Con12A2B2CMapper From(FeeDetail procucerFeesProducerDisposalFees)
    {
        return new TotalProducerFeeWithBadDebtProvisionFor2Con12A2B2CMapper
        {
            TotalFeeWithBadDebtProvision            = FormatUtils.FormatCurrency(procucerFeesProducerDisposalFees.TotalOnePlus2A2B2CWithBadDebt()),
            ProducerPercentageOfOverallProducerCost = FormatUtils.FormatPercentage(procucerFeesProducerDisposalFees.TotalOnePlus2A2B2CWithBadDebtPercentage)
        };

    }
}
