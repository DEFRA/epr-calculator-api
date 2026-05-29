using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary.OneAndTwoA
{
    public static class CalcResultOneAndTwoAUtil
    {
        public static decimal GetTotalDisposalCostswoBadDebtprovision1(IReadOnlyList<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.TotalProducerDisposalFee);
        }

        public static decimal GetTotalBadDebtprovision1(IReadOnlyList<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.LocalAuthorityDisposalCostsSectionOne?.BadDebtProvision ?? 0);
        }

        public static decimal GetTotalDisposalCostswithBadDebtprovision1(IReadOnlyList<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.TotalProducerDisposalFeeWithBadDebtProvision);
        }

        public static decimal GetTotalCommsCostswoBadDebtprovision2A(IReadOnlyList<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.TotalProducerCommsFee);
        }

        public static decimal GetTotalBadDebtprovision2A(IReadOnlyList<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.CommunicationCostsSectionTwoA?.BadDebtProvision ?? 0);
        }

        public static decimal GetTotalCommsCostswithBadDebtprovision2A(IReadOnlyList<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.TotalProducerCommsFeeWithBadDebtProvision);
        }

        public static decimal GetTotalFee(IReadOnlyList<CalcResultSummaryProducerDisposalFees>? producerDisposalFees, Func<CalcResultSummaryProducerDisposalFees, decimal> selector)
        {
            if (producerDisposalFees == null)
            {
                return 0m;
            }

            var totalFee = producerDisposalFees
                .FirstOrDefault(t => t.LeaverDate == CommonConstants.Totals);

            if (totalFee is null)
            {
                return 0m;
            }

            return selector(totalFee);
        }
    }
}
