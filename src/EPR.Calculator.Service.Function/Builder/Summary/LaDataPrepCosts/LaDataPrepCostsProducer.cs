using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary.LaDataPrepCosts
{
    public static class LaDataPrepCostsProducer
    {
        public static void SetValues(CalcResult calcResult, CalcResultSummary result)
        {
            result.LaDataPrepCostsTitleSection4 = GetLaDataPrepCostsWithoutBadDebtProvision(calcResult);
            result.LaDataPrepCostsBadDebtProvisionTitleSection4 = GetLaDataPrepCostsBadDebtProvision(calcResult);
            result.LaDataPrepCostsWithBadDebtProvisionTitleSection4 = GetLaDataPrepCostsWithBadDebtProvision(calcResult);

            foreach (var fee in result.ProducerDisposalFees)
            {
                var totalProducerFeeWithoutBadDebtProvision = GetTotalWithoutBadDebtProvision(result, fee);
                var badDebtProvision = GetBadDebtProvision(calcResult, totalProducerFeeWithoutBadDebtProvision);

                fee.LocalAuthorityDataPreparationCosts = new CalcResultSummaryBadDebtProvision
                {
                    TotalProducerFeeWithoutBadDebtProvision = totalProducerFeeWithoutBadDebtProvision,
                    BadDebtProvision = badDebtProvision,
                    TotalProducerFeeWithBadDebtProvision = totalProducerFeeWithoutBadDebtProvision + badDebtProvision,
                    EnglandTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, result.LaDataPrepCostsTitleSection4, fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.England),
                    WalesTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, result.LaDataPrepCostsTitleSection4, fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Wales),
                    ScotlandTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, result.LaDataPrepCostsTitleSection4, fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Scotland),
                    NorthernIrelandTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, result.LaDataPrepCostsTitleSection4, fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.NorthernIreland)
                };
            }
        }

        public static decimal GetLaDataPrepCostsWithoutBadDebtProvision(CalcResult calcResult)
        {
            return calcResult.CalcResultParameterOtherCost.LaDataPrepCharge?.Total ?? 0m;
        }

        private static decimal GetLaDataPrepCostsBadDebtProvision(CalcResult calcResult)
        {
            return GetLaDataPrepCostsWithoutBadDebtProvision(calcResult)
                * calcResult.CalcResultParameterOtherCost.BadDebtValue
                / 100;
        }

        private static decimal GetBadDebtProvision(CalcResult calcResult, decimal totalProducerFeeWithoutBadDebtProvision)
        {
            return totalProducerFeeWithoutBadDebtProvision
                * calcResult.CalcResultParameterOtherCost.BadDebtValue
                / 100;
        }

        private static decimal GetLaDataPrepCostsWithBadDebtProvision(CalcResult calcResult)
        {
            return GetLaDataPrepCostsWithoutBadDebtProvision(calcResult) + GetLaDataPrepCostsBadDebtProvision(calcResult);
        }

        private static decimal GetTotalWithoutBadDebtProvision(CalcResultSummary result, CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C
                * result.LaDataPrepCostsTitleSection4
                / 100;
        }

        public static decimal GetCountryTotalWithBadDebtProvision(CalcResult calcResult,
            decimal laDataPrepCostsTitleSection4,
            decimal producerOverallPercentageOfCostsForOnePlus2A2B2C,
            Countries country)
        {
            var onePlusBadDebtProvision = 1 + (calcResult.CalcResultParameterOtherCost.BadDebtValue / 100);

            return laDataPrepCostsTitleSection4
                * onePlusBadDebtProvision
                * (producerOverallPercentageOfCostsForOnePlus2A2B2C / 100)
                * CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(calcResult, country)
                / 100;
        }
    }
}
