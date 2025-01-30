using EPR.Calculator.API.Builder.Summary.Common;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.LaDataPrepCosts
{
    public static class LaDataPrepCostsProducer
    {
        public static readonly int ColumnIndex = 234;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.TotalProducerFeeWithoutBadDebtProvision , ColumnIndex = ColumnIndex },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.BadDebtProvision, ColumnIndex = ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.TotalProducerFeeWithBadDebtProvision, ColumnIndex = ColumnIndex + 2 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.EnglandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 3 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.WalesTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 4 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.ScotlandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 5 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.NorthernIrelandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 6 }
            ];
        }

        public static IEnumerable<CalcResultSummaryHeader> GetSummaryHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.LaDataPrepCostsWithoutBadDebtProvisionTitle, ColumnIndex = ColumnIndex },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.BadDebtProvisionTitle, ColumnIndex = ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.LaDataPrepCostsWithBadDebtProvisionTitle, ColumnIndex = ColumnIndex + 2 }
            ];
        }

        public static void SetValues(CalcResult calcResult, CalcResultSummary result)
        {
            result.LaDataPrepCostsTitleSection4 = GetLaDataPrepCostsWithoutBadDebtProvision(calcResult);
            result.LaDataPrepCostsBadDebtProvisionTitleSection4 = GetBadDebtProvision(calcResult);
            result.LaDataPrepCostsWithBadDebtProvisionTitleSection4 = GetLaDataPrepCostsWithBadDebtProvision(calcResult);

            foreach (var fee in result.ProducerDisposalFees)
            {
                fee.LaDataPrepCostsTotalWithoutBadDebtProvisionSection4 = GetTotalWithoutBadDebtProvision(result, fee);

                fee.LaDataPrepCostsBadDebtProvisionSection4 = GetBadDebtProvision(calcResult, fee);

                fee.LaDataPrepCostsTotalWithBadDebtProvisionSection4 = GetTotalWithBadDebtProvision(fee);

                fee.LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4 =
                    GetCountryTotalWithBadDebtProvision(calcResult,
                        result.LaDataPrepCostsTitleSection4,
                        fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.England);

                fee.LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4 =
                    GetCountryTotalWithBadDebtProvision(calcResult,
                        result.LaDataPrepCostsTitleSection4,
                        fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Wales);

                fee.LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4 =
                    GetCountryTotalWithBadDebtProvision(calcResult,
                        result.LaDataPrepCostsTitleSection4,
                        fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Scotland);

                fee.LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4 =
                    GetCountryTotalWithBadDebtProvision(calcResult,
                        result.LaDataPrepCostsTitleSection4,
                        fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.NorthernIreland);
            }
        }

        private static decimal GetLaDataPrepCostsWithoutBadDebtProvision(CalcResult calcResult)
        {
            var dataPrepCharge = calcResult.CalcResultParameterOtherCost.Details.FirstOrDefault(
                cost => cost.Name == OnePlus4ApportionmentColumnHeaders.LADataPrepCharge);

            if (dataPrepCharge != null)
            {
                return dataPrepCharge.TotalValue;
            }

            return 0;
        }

        private static decimal GetBadDebtProvision(CalcResult calcResult)
        {
            return GetLaDataPrepCostsWithoutBadDebtProvision(calcResult) * calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
        }

        private static decimal GetLaDataPrepCostsWithBadDebtProvision(CalcResult calcResult)
        {
            return GetLaDataPrepCostsWithoutBadDebtProvision(calcResult) + GetBadDebtProvision(calcResult);
        }

        private static decimal GetTotalWithoutBadDebtProvision(CalcResultSummary result, CalcResultSummaryProducerDisposalFees fee)
        {
            return (fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C * result.LaDataPrepCostsTitleSection4) / 100;
        }

        private static decimal GetBadDebtProvision(CalcResult calcResult, CalcResultSummaryProducerDisposalFees fee)
        {
            return (fee.LaDataPrepCostsTotalWithoutBadDebtProvisionSection4 *
                    calcResult.CalcResultParameterOtherCost.BadDebtValue) / 100;
        }

        private static decimal GetTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.LaDataPrepCostsTotalWithoutBadDebtProvisionSection4 +
                   fee.LaDataPrepCostsBadDebtProvisionSection4;
        }

        public static decimal GetCountryTotalWithBadDebtProvision(CalcResult calcResult,
            decimal laDataPrepCostsTitleSection4,
            decimal producerOverallPercentageOfCostsForOnePlus2A2B2C,
            Countries country)
        {
            var onePlusBadDebtProvision = 1 + (calcResult.CalcResultParameterOtherCost.BadDebtValue / 100);

            if (producerOverallPercentageOfCostsForOnePlus2A2B2C == 0)
            {
                return 0;
            }

            var result = laDataPrepCostsTitleSection4 * onePlusBadDebtProvision *
                         (producerOverallPercentageOfCostsForOnePlus2A2B2C / 100) *
                         CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(calcResult, country);

            return result == 0 ? 0 : result / 100;
        }
    }
}
