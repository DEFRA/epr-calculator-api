using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.OnePlus2A2B2C
{
    public static class OnePlus2A2B2CProducer
    {
        public static readonly int ColumnIndex = 242;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = OnePlus2A2B2CHeaders.ProducerTotalWithBadDebtProvision , ColumnIndex = ColumnIndex },
                new CalcResultSummaryHeader { Name = OnePlus2A2B2CHeaders.ProducerPercentageOfOverallProducerCost, ColumnIndex = ColumnIndex + 1 }
            ];
        }

        public static IEnumerable<CalcResultSummaryHeader> GetSummaryHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = OnePlus2A2B2CHeaders.TotalWithBadDebtProvision , ColumnIndex = ColumnIndex }
            ];
        }

        public static void SetValues(CalcResultSummary result)
        {
            result.TotalOnePlus2A2B2CFeeWithBadDebtProvision = GetHeaderTotalFeeWithBadDebtProvision(result);
            foreach (var fee in result.ProducerDisposalFees)
            {
                fee.ProducerTotalOnePlus2A2B2CWithBadDeptProvision = GetTotalWithBadDebtProvision(fee);
                fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C = GetOverallProducerPercentage(fee, result.TotalOnePlus2A2B2CFeeWithBadDebtProvision);
            }
        }

        private static decimal GetHeaderTotalFeeWithBadDebtProvision(CalcResultSummary result)
        {
            return result.TotalFeeforLADisposalCostswithBadDebtprovision1 +
                   result.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A +
                   result.CommsCostHeaderWithBadDebtFor2bTitle +
                   result.TwoCCommsCostsByCountryWithBadDebtProvision;
        }

        private static decimal GetTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.TotalProducerFeeforLADisposalCostswithBadDebtprovision +
                   fee.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision +
                   fee.TotalProducerFeeWithBadDebtFor2bComms +
                   fee.TwoCTotalProducerFeeForCommsCostsWithBadDebt;
        }

        private static decimal GetOverallProducerPercentage(CalcResultSummaryProducerDisposalFees fee, decimal totalOnePlus2A2B2CFeeWithBadDebtProvision)
        {
            return totalOnePlus2A2B2CFeeWithBadDebtProvision == 0
                ? 0
                : (fee.ProducerTotalOnePlus2A2B2CWithBadDeptProvision / totalOnePlus2A2B2CFeeWithBadDebtProvision) * 100;
        }
    }
}
