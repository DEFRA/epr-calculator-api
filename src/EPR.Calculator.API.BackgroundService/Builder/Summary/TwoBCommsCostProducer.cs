using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Builder.Summary;

public static class TwoBCommsCostProducer
{
    public static void SetValues(CalcResult calcResult, ProducerFees producerFees)
    {
        var withoutbadDebtProvision = calcResult.CalcResultCommsCostReportDetail.CommsCostUkWide;
        var badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue / 100 * withoutbadDebtProvision;
        producerFees.Total.CommsCostsSection2b = new FeeWithBadDebt
        {
            FeeWithoutBadDebt = withoutbadDebtProvision.Total,
            BadDebt           = badDebtProvision.Total,
            ByCountry         = withoutbadDebtProvision + badDebtProvision
        };
    }
}
