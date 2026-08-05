using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class TwoCCommsCostProducer
{
    public static void SetValues(CalcResult calcResult, ProducerFees producerFees)
    {
        var commsCostByCountry = calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry;
        var badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue / 100 * commsCostByCountry;
        producerFees.Total.CommsCostsSection2c = new FeeWithBadDebt
        {
            FeeWithoutBadDebt = commsCostByCountry.Total,
            BadDebt           = badDebtProvision.Total,
            ByCountry    = commsCostByCountry + badDebtProvision
        };
    }

    public static void UpdateTwoCRows(CalcResult calcResult, FeeDetail result)
    {
        var commsCost = calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry;

        var badDebtProvisionValue =
            calcResult.CalcResultParameterOtherCost.BadDebtValue / 100
            * calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry;

        result.CommsCostsSection2c = new FeeWithBadDebt
        {
            FeeWithoutBadDebt =
                commsCost.Total
                * result.ReportedTonnagePercentage
                / 100,
            BadDebt = badDebtProvisionValue.Total
                * result.ReportedTonnagePercentage
                / 100,
            ByCountry = (commsCost + badDebtProvisionValue)
                * (result.ReportedTonnagePercentage
                / 100)
        };
    }
}
