using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary.ThreeSa
{
    public static class ThreeSaCostsProducer
    {
        public static void GetProducerSetUpCostsSection3(CalcResult calcResult, CalcResultSummary summary)
        {
            summary.SaOperatingCostsWoTitleSection3 = calcResult.CalcResultParameterOtherCost.SaOperatingCost.Total;
            summary.BadDebtProvisionTitleSection3 = (summary.SaOperatingCostsWoTitleSection3 * calcResult.CalcResultParameterOtherCost.BadDebtValue) / 100;
            summary.SaOperatingCostsWithTitleSection3 = summary.BadDebtProvisionTitleSection3 + summary.SaOperatingCostsWoTitleSection3;

            foreach (var item in summary.ProducerDisposalFees)
            {
                var totalProducerFeeWithoutBadDebtProvision = GetTotalProducerFeeWithoutBadDebtProvision(summary, item);
                var badDebtProvision = GetBadDebtProvision(calcResult, totalProducerFeeWithoutBadDebtProvision);

                item.SchemeAdministratorOperatingCosts = new CalcResultSummaryBadDebtProvision
                {
                    TotalProducerFeeWithoutBadDebtProvision = totalProducerFeeWithoutBadDebtProvision,
                    BadDebtProvision = badDebtProvision,
                    TotalProducerFeeWithBadDebtProvision = totalProducerFeeWithoutBadDebtProvision + badDebtProvision,
                    EnglandTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaOperatingCostsWoTitleSection3, item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.England),
                    WalesTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaOperatingCostsWoTitleSection3, item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Wales),
                    ScotlandTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaOperatingCostsWoTitleSection3, item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Scotland),
                    NorthernIrelandTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaOperatingCostsWoTitleSection3, item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.NorthernIreland)
                };
            }
        }

        private static decimal GetBadDebtProvision(CalcResult calcResult, decimal totalProducerFeeWithoutBadDebtProvision)
        {
            return (totalProducerFeeWithoutBadDebtProvision * calcResult.CalcResultParameterOtherCost.BadDebtValue) / 100;
        }

        private static decimal GetTotalProducerFeeWithoutBadDebtProvision(CalcResultSummary summary, CalcResultSummaryProducerDisposalFees item)
        {
            return (item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C * summary.SaOperatingCostsWoTitleSection3) / 100;
        }

        public static decimal GetCountryTotalWithBadDebtProvision(CalcResult calcResult, decimal threeSaCostsWithoutBadDebtProvision, decimal ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries country)
        {
            var badDebt = calcResult.CalcResultParameterOtherCost.BadDebtValue;
            var countryTotal = (CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, country)) / 100;
            return threeSaCostsWithoutBadDebtProvision * (1 + (badDebt / 100)) * (ProducerOverallPercentageOfCostsForOnePlus2A2B2C / 100) * countryTotal;
        }
    }
}
