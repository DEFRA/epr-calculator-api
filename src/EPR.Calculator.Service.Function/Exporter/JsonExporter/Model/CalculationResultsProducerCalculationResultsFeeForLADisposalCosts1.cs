using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1
{
    [JsonPropertyName("totalProducerFeeForLADisposalCostsWithoutBadDebtProvision")]
    public required string TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision { get; set; }

    [JsonPropertyName("badDebtProvisionForLADisposalCosts")]
    public required string BadDebtProvisionForLADisposalCosts { get; set; }

    [JsonPropertyName("totalProducerFeeForLADisposalCostsWithBadDebtProvision")]
    public required string TotalProducerFeeForLADisposalCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("englandTotalForLADisposalCostsWithBadDebtProvision")]
    public required string EnglandTotalForLADisposalCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("walesTotalForLADisposalCostsWithBadDebtProvision")]
    public required string WalesTotalForLADisposalCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("scotlandTotalForLADisposalCostsWithBadDebtProvision")]
    public required string ScotlandTotalForLADisposalCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("northernIrelandTotalForLADisposalCostsWithBadDebtProvision")]
    public required string NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision { get; set; }

    public static CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1 From(FeeDetail procucerFeesProducerDisposalFees)
    {
        var costs = procucerFeesProducerDisposalFees.LADisposalCostsSection1;
        return new CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1
        {
            TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision  = FormatUtils.FormatCurrency(costs.FeeWithoutBadDebt),
            BadDebtProvisionForLADisposalCosts                         = FormatUtils.FormatCurrency(costs.BadDebt),
            TotalProducerFeeForLADisposalCostsWithBadDebtProvision     = FormatUtils.FormatCurrency(costs.ByCountry.Total),
            EnglandTotalForLADisposalCostsWithBadDebtProvision         = FormatUtils.FormatCurrency(costs.ByCountry.England),
            WalesTotalForLADisposalCostsWithBadDebtProvision           = FormatUtils.FormatCurrency(costs.ByCountry.Wales),
            ScotlandTotalForLADisposalCostsWithBadDebtProvision        = FormatUtils.FormatCurrency(costs.ByCountry.Scotland),
            NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision = FormatUtils.FormatCurrency(costs.ByCountry.NorthernIreland),
        };
    }
}
