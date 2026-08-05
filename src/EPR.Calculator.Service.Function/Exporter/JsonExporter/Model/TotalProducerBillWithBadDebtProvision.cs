using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

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

    public static TotalProducerBillWithBadDebtProvision From(FeeDetail procucerFeesProducerDisposalFees)
    {
        var costs = procucerFeesProducerDisposalFees.TotalBillBreakdown;
        return new TotalProducerBillWithBadDebtProvision
        {
            TotalProducerBillWithoutBadDebtProvision                = FormatUtils.FormatCurrency(costs.FeeWithoutBadDebt),
            BadDebtProvisionForTotalProducerBill                    = FormatUtils.FormatCurrency(costs.BadDebt),
            TotalProducerBillWithBadDebtProvisionAmount             = FormatUtils.FormatCurrency(costs.ByCountry.Total),
            EnglandTotalForProducerBillWithBadDebtProvision         = FormatUtils.FormatCurrency(costs.ByCountry.England),
            WalesTotalForProducerBillWithBadDebtProvision           = FormatUtils.FormatCurrency(costs.ByCountry.Wales),
            ScotlandTotalForProducerBillWithBadDebtProvision        = FormatUtils.FormatCurrency(costs.ByCountry.Scotland),
            NorthernIrelandTotalForProducerBillWithBadDebtProvision = FormatUtils.FormatCurrency(costs.ByCountry.NorthernIreland)
        };
    }
}
