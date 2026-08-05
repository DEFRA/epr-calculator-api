using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalcResultsCommsCostsWithBadDebtProvision2C
{
    [JsonPropertyName("totalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision")]
    public string? TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision { get; set; }

    [JsonPropertyName("badDebProvisionFor2c")]
    public string? BadDebtProvisionFor2c { get; set; }

    [JsonPropertyName("totalProducerFeeForCommsCostsByCountryWithBadDebtProvision")]
    public string? TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision { get; set; }


    [JsonPropertyName("englandTotalWithBadDebtProvision")]
    public string? EnglandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("walesTotalWithBadDebtProvision")]
    public string? WalesTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("scotlandTotalWithBadDebtProvision")]
    public string? ScotlandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("northernIrelandTotalWithBadDebtProvision")]
    public string? NorthernIrelandTotalWithBadDebtProvision { get; set; }

    public static CalcResultsCommsCostsWithBadDebtProvision2C From(FeeDetail procucerFeesProducerDisposalFees)
    {
        return new CalcResultsCommsCostsWithBadDebtProvision2C
        {
            TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision = FormatUtils.FormatCurrency(procucerFeesProducerDisposalFees.CommsCostsSection2c.FeeWithoutBadDebt),
            BadDebtProvisionFor2c                                         = FormatUtils.FormatCurrency(procucerFeesProducerDisposalFees.CommsCostsSection2c.BadDebt),
            TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision    = FormatUtils.FormatCurrency(procucerFeesProducerDisposalFees.CommsCostsSection2c.ByCountry.Total),
            EnglandTotalWithBadDebtProvision                              = FormatUtils.FormatCurrency(procucerFeesProducerDisposalFees.CommsCostsSection2c.ByCountry.England),
            WalesTotalWithBadDebtProvision                                = FormatUtils.FormatCurrency(procucerFeesProducerDisposalFees.CommsCostsSection2c.ByCountry.Wales),
            ScotlandTotalWithBadDebtProvision                             = FormatUtils.FormatCurrency(procucerFeesProducerDisposalFees.CommsCostsSection2c.ByCountry.Scotland),
            NorthernIrelandTotalWithBadDebtProvision                      = FormatUtils.FormatCurrency(procucerFeesProducerDisposalFees.CommsCostsSection2c.ByCountry.NorthernIreland)
        };
    }

}
