using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary.OnePlus2A2B2C
{
    public static class OnePlus2A2B2CProducer
    {
        public static ImmutableList<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = OnePlus2A2B2CHeaders.ProducerTotalWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = OnePlus2A2B2CHeaders.ProducerPercentageOfOverallProducerCost }
            ];
        }

        public static ImmutableList<CalcResultSummaryHeader> GetSummaryHeaders(int columnIndex)
        {
            return [
                new CalcResultSummaryHeader { Name = OnePlus2A2B2CHeaders.TotalWithBadDebtProvision , ColumnIndex = columnIndex }
            ];
        }

        public static void SetValues(CalcResultSummary result)
        {
            result.TotalOnePlus2A2B2CFeeWithBadDebtProvision = GetHeaderTotalFeeWithBadDebtProvision(result);
            foreach (var fee in result.ProducerDisposalFees)
            {
                fee.ProducerTotalOnePlus2A2B2CWithBadDeptProvision = GetTotalWithBadDebtProvision(fee) ?? 0;
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

        private static decimal? GetTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.LocalAuthorityDisposalCostsSectionOne?.TotalProducerFeeWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoA?.TotalProducerFeeWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoB?.TotalProducerFeeWithBadDebtProvision +
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
