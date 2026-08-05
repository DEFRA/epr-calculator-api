using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class DisposalFeeSummary1
{
    [JsonPropertyName("totalProducerDisposalFeeWithoutBadDebtProvision")]
    public required string TotalProducerDisposalFeeWithoutBadDebtProvision { get; set; }

    [JsonPropertyName("badDebtProvision")]
    public required string BadDebtProvision { get; set; }

    [JsonPropertyName("totalProducerDisposalFeeWithBadDebtProvision")]
    public required string TotalProducerDisposalFeeWithBadDebtProvision { get; set; }

    [JsonPropertyName("englandTotal")]
    public required string EnglandTotal { get; set; }

    [JsonPropertyName("walesTotal")]
    public required string WalesTotal { get; set; }

    [JsonPropertyName("scotlandTotal")]
    public required string ScotlandTotal { get; set; }

    [JsonPropertyName("northernIrelandTotal")]
    public required string NorthernIrelandTotal { get; set; }

    [JsonPropertyName("tonnageChangeCount")]
    public required string TonnageChangeCount { get; set; }

    [JsonPropertyName("tonnageChangeAdvice")]
    public required string TonnageChangeAdvice { get; set; }

    public static DisposalFeeSummary1 From(FeeDetail producerFees)
    {
        return new DisposalFeeSummary1
        {
            TotalProducerDisposalFeeWithoutBadDebtProvision = FormatUtils.FormatCurrency(producerFees.LADisposalCostsSection1.FeeWithoutBadDebt),
            BadDebtProvision                                = FormatUtils.FormatCurrency(producerFees.LADisposalCostsSection1.BadDebt),
            TotalProducerDisposalFeeWithBadDebtProvision    = FormatUtils.FormatCurrency(producerFees.LADisposalCostsSection1.ByCountry.Total),
            EnglandTotal                                    = FormatUtils.FormatCurrency(producerFees.LADisposalCostsSection1.ByCountry.England),
            WalesTotal                                      = FormatUtils.FormatCurrency(producerFees.LADisposalCostsSection1.ByCountry.Wales),
            ScotlandTotal                                   = FormatUtils.FormatCurrency(producerFees.LADisposalCostsSection1.ByCountry.Scotland),
            NorthernIrelandTotal                            = FormatUtils.FormatCurrency(producerFees.LADisposalCostsSection1.ByCountry.NorthernIreland),
            TonnageChangeCount                              = producerFees.TonnageChangeCount ?? CommonConstants.Hyphen,
            TonnageChangeAdvice                             = producerFees.TonnageChangeAdvice ?? CommonConstants.Hyphen,
        };
    }
}
