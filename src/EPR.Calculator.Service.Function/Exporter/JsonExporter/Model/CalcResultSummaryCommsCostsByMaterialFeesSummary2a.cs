using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalcResultSummaryCommsCostsByMaterialFeesSummary2A
{
    [JsonPropertyName("totalProducerFeeForCommsCostsWithoutBadDebtProvision2a")]
    public required string  TotalProducerFeeForCommsCostsWithoutBadDebtProvision2a { get; set; }

    [JsonPropertyName("totalBadDebtProvision")]
    public required string TotalBadDebtProvision { get; set; }

    [JsonPropertyName("totalProducerFeeForCommsCostsWithBadDebtProvision2a")]
    public required string TotalProducerFeeForCommsCostsWithBadDebtProvision2a { get; set; }

    [JsonPropertyName("englandTotalWithBadDebtProvision")]
    public required string EnglandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("walesTotalWithBadDebtProvision")]
    public required string WalesTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("scotlandTotalWithBadDebtProvision")]
    public required string ScotlandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("northernIrelandTotalWithBadDebtProvision")]
    public required string NorthernIrelandTotalWithBadDebtProvision { get; set; }

    public static CalcResultSummaryCommsCostsByMaterialFeesSummary2A From(FeeDetail procucerFeesProducerDisposalFees)
    {
        var costs = procucerFeesProducerDisposalFees.CommsCostsSection2a;
        return new CalcResultSummaryCommsCostsByMaterialFeesSummary2A
        {
            TotalProducerFeeForCommsCostsWithoutBadDebtProvision2a = FormatUtils.FormatCurrency(costs.FeeWithoutBadDebt),
            TotalBadDebtProvision                                  = FormatUtils.FormatCurrency(costs.BadDebt),
            TotalProducerFeeForCommsCostsWithBadDebtProvision2a    = FormatUtils.FormatCurrency(costs.ByCountry.Total),
            EnglandTotalWithBadDebtProvision                       = FormatUtils.FormatCurrency(costs.ByCountry.England),
            WalesTotalWithBadDebtProvision                         = FormatUtils.FormatCurrency(costs.ByCountry.Wales),
            ScotlandTotalWithBadDebtProvision                      = FormatUtils.FormatCurrency(costs.ByCountry.Scotland),
            NorthernIrelandTotalWithBadDebtProvision               = FormatUtils.FormatCurrency(costs.ByCountry.NorthernIreland)
        };
    }
}
