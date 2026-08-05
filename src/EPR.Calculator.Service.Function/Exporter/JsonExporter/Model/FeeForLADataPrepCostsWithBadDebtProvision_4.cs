using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

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

    public static FeeForLADataPrepCostsWithBadDebtProvision_4 From(FeeDetail procucerFeesProducerDisposalFees)
    {
        var costs = procucerFeesProducerDisposalFees.LaDataPrepSection4;
        return new FeeForLADataPrepCostsWithBadDebtProvision_4
        {
            TotalProducerFeeForLADataPrepCostsWithoutBadDebtProvision  = FormatUtils.FormatCurrency(costs.FeeWithoutBadDebt),
            BadDebtProvisionFor4                                       = FormatUtils.FormatCurrency(costs.BadDebt),
            TotalProducerFeeForLADataPrepCostsWithBadDebtProvision     = FormatUtils.FormatCurrency(costs.ByCountry.Total),
            EnglandTotalForLADataPrepCostsWithBadDebtProvision         = FormatUtils.FormatCurrency(costs.ByCountry.England),
            WalesTotalForLADataPrepCostsWithBadDebtProvision           = FormatUtils.FormatCurrency(costs.ByCountry.Wales),
            ScotlandTotalForLADataPrepCostsWithBadDebtProvision        = FormatUtils.FormatCurrency(costs.ByCountry.Scotland),
            NorthernIrelandTotalForLADataPrepCostsWithBadDebtProvision = FormatUtils.FormatCurrency(costs.ByCountry.NorthernIreland)
        };
    }
}
