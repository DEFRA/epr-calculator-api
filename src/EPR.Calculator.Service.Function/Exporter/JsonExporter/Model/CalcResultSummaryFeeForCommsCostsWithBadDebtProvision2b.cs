using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B
{
    [JsonPropertyName("totalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision")]
    public string? TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision { get; set; }

    [JsonPropertyName("badDebtProvisionFor2b")]
    public string? BadDebtProvisionFor2b { get; set; }

    [JsonPropertyName("totalProducerFeeForCommsCostsUKWideWithBadDebtProvision")]
    public string? TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision { get; set; }

    [JsonPropertyName("englandTotalWithBadDebtProvision")]
    public string? EnglandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("walesTotalWithBadDebtProvision")]
    public string? WalesTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("scotlandTotalWithBadDebtProvision")]
    public string? ScotlandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("northernIrelandTotalWithBadDebtProvision")]
    public string? NorthernIrelandTotalWithBadDebtProvision { get; set; }

    public static CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B From(FeeDetail procucerFeesProducerDisposalFees)
    {
        var costs = procucerFeesProducerDisposalFees.CommsCostsSection2b;
        return new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B
        {
            TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision = FormatUtils.FormatCurrency(costs.FeeWithoutBadDebt),
            BadDebtProvisionFor2b                                      = FormatUtils.FormatCurrency(costs.BadDebt),
            TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision    = FormatUtils.FormatCurrency(costs.ByCountry.Total),
            EnglandTotalWithBadDebtProvision                           = FormatUtils.FormatCurrency(costs.ByCountry.England),
            WalesTotalWithBadDebtProvision                             = FormatUtils.FormatCurrency(costs.ByCountry.Wales),
            ScotlandTotalWithBadDebtProvision                          = FormatUtils.FormatCurrency(costs.ByCountry.Scotland),
            NorthernIrelandTotalWithBadDebtProvision                   = FormatUtils.FormatCurrency(costs.ByCountry.NorthernIreland)
        };
    }
}
