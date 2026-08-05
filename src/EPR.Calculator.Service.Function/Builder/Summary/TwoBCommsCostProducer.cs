using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

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
