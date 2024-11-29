using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.OneAndTwoA;

public static class CalcResultOneAndTwoAUtil
{
    public static decimal GetTotalDisposalCostswoBadDebtprovision1(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
    {
        return GetTotalFee(producerDisposalFees, fee => fee.TotalProducerDisposalFee);
    }

    public static decimal GetTotalBadDebtprovision1(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
    {
        return GetTotalFee(producerDisposalFees, fee => fee.BadDebtProvisionFor1);
    }

    public static decimal GetTotalDisposalCostswithBadDebtprovision1(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
    {
        return GetTotalFee(producerDisposalFees, fee => fee.TotalProducerDisposalFeeWithBadDebtProvision);
    }

    public static decimal GetTotalCommsCostswoBadDebtprovision2A(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
    {
        return GetTotalFee(producerDisposalFees, fee => fee.TotalProducerCommsFee);
    }

    public static decimal GetTotalBadDebtprovision2A(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
    {
        return GetTotalFee(producerDisposalFees, fee => fee.BadDebtProvisionFor2A);
    }

    public static decimal GetTotalCommsCostswithBadDebtprovision2A(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
    {
        return GetTotalFee(producerDisposalFees, fee => fee.TotalProducerCommsFeeWithBadDebtProvision);
    }

    public static decimal GetTotalFee(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees, Func<CalcResultSummaryProducerDisposalFees, decimal?> selector)
    {
        if (producerDisposalFees == null)
        {
            return 0m;
        }

        var totalFee = producerDisposalFees
            .FirstOrDefault(t => t.Level == "Totals");

        return selector(totalFee) ?? 0m;
    }
}