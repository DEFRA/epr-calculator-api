using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2A
{
    [JsonPropertyName("totalProducerFeeForCommsCostsWithoutBadDebtProvision")]
    public string? TotalProducerFeeForCommsCostsWithoutBadDebtProvision { get; set; }

    [JsonPropertyName("badDebtProvisionFor2a")]
    public string? BadDebtProvisionFor2a { get; set; }

    [JsonPropertyName("totalProducerFeeForCommsCostsWithBadDebtProvision")]
    public string? TotalProducerFeeForCommsCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("englandTotalWithBadDebtProvision")]
    public string? EnglandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("walesTotalWithBadDebtProvision")]
    public string? WalesTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("scotlandTotalWithBadDebtProvision")]
    public string? ScotlandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("northernIrelandTotalWithBadDebtProvision")]
    public string? NorthernIrelandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("percentageOfProducerTonnageVsAllProducers")]
    public string? PercentageOfProducerTonnageVsAllProducers { get; set; }

    public static CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2A From(FeeDetail procucerFeesProducerDisposalFees)
    {
        var costs = procucerFeesProducerDisposalFees.CommsCostsSection2a;
        return new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2A
        {
            TotalProducerFeeForCommsCostsWithoutBadDebtProvision = FormatUtils.FormatCurrency(costs.FeeWithoutBadDebt),
            BadDebtProvisionFor2a                                = FormatUtils.FormatCurrency(costs.BadDebt),
            TotalProducerFeeForCommsCostsWithBadDebtProvision    = FormatUtils.FormatCurrency(costs.ByCountry.Total),
            EnglandTotalWithBadDebtProvision                     = FormatUtils.FormatCurrency(costs.ByCountry.England),
            WalesTotalWithBadDebtProvision                       = FormatUtils.FormatCurrency(costs.ByCountry.Wales),
            ScotlandTotalWithBadDebtProvision                    = FormatUtils.FormatCurrency(costs.ByCountry.Scotland),
            NorthernIrelandTotalWithBadDebtProvision             = FormatUtils.FormatCurrency(costs.ByCountry.NorthernIreland),
            PercentageOfProducerTonnageVsAllProducers            = FormatUtils.FormatPercentage(procucerFeesProducerDisposalFees.ReportedTonnagePercentage)
        };
    }
}
