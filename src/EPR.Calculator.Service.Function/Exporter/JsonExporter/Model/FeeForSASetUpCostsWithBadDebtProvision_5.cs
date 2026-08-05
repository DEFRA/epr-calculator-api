using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

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

    public static FeeForSaSetUpCostsWithBadDebtProvision5 From(FeeDetail procucerFeesProducerDisposalFees)
    {
        var costs = procucerFeesProducerDisposalFees.SaSetupCostsSection5;
        return new FeeForSaSetUpCostsWithBadDebtProvision5
        {
            TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision = FormatUtils.FormatCurrency(costs.FeeWithoutBadDebt),
            BadDebtProvisionFor5                                         = FormatUtils.FormatCurrency(costs.BadDebt),
            TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision    = FormatUtils.FormatCurrency(costs.ByCountry.Total),
            EnglandTotalForSASetUpCostsWithBadDebtProvision              = FormatUtils.FormatCurrency(costs.ByCountry.England),
            WalesTotalForSASetUpCostsWithBadDebtProvision                = FormatUtils.FormatCurrency(costs.ByCountry.Wales),
            ScotlandTotalForSASetUpCostsWithBadDebtProvision             = FormatUtils.FormatCurrency(costs.ByCountry.Scotland),
            NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision      = FormatUtils.FormatCurrency(costs.ByCountry.NorthernIreland)
        };
    }
}
