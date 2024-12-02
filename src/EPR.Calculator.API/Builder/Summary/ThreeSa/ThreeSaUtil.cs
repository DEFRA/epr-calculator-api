using EPR.Calculator.API.Builder.Summary.Common;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.ThreeSA
{
    public static class ThreeSaUtil
    {
        public static decimal GetSAOperatingCostsScotlandTotalWithBadDebtProvisionSection3(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialsCostSummary, Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> costSummary, List<MaterialDetail> materials, CalcResult calcResult)
        {
            //1+paramOthers
            Decimal OnePlusOtherParam = 1 + (calcResult.CalcResultParameterOtherCost.BadDebtValue / 100);

            //1+4 apportenmnt

            var oneplause = ConverttoDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Where(x => x.Name == "1 + 4 Apportionment %s").FirstOrDefault().ScotlandDisposalTotal);
            var producerPercentage = GetTotal1Plus2ABadDebtPercentage(CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialsCostSummary), CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(costSummary), materials, calcResult);
            return GetSAOperatingCostsTotalWithoutBadDebtProvisionTitleSection3(calcResult) * ConverttoDecimal(producerPercentage.ToString()) * OnePlusOtherParam * oneplause;
        }

        public static decimal GetSAOperatingCostsWalesTotalWithBadDebtProvisionSection3(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialsCostSummary, Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> costSummary, List<MaterialDetail> materials, CalcResult calcResult)
        {
            //1+paramOthers
            Decimal OnePlusOtherParam = 1 + (calcResult.CalcResultParameterOtherCost.BadDebtValue / 100);

            //1+4 apportenmnt

            var oneplause = ConverttoDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Where(x => x.Name == "1 + 4 Apportionment %s").FirstOrDefault().WalesDisposalTotal);

            var producerPercentage = GetTotal1Plus2ABadDebtPercentage(CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialsCostSummary), CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(costSummary), materials, calcResult);
            return GetSAOperatingCostsTotalWithoutBadDebtProvisionTitleSection3(calcResult) * ConverttoDecimal(producerPercentage.ToString()) * OnePlusOtherParam * oneplause;
        }

        public static decimal GetSAOperatingCostsEnglandTotalWithBadDebtProvisionSection3(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialsCostSummary, Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> costSummary, List<MaterialDetail> materials, CalcResult calcResult)
        {
            //1+paramOthers
            Decimal OnePlusOtherParam = 1 + (calcResult.CalcResultParameterOtherCost.BadDebtValue / 100);

            //1+4 apportenmnt

            var oneplause = ConverttoDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Where(x => x.Name == "1 + 4 Apportionment %s").FirstOrDefault().EnglandDisposalTotal);

            var producerPercentage = GetTotal1Plus2ABadDebtPercentage(CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialsCostSummary), CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(costSummary), materials, calcResult);
            return GetSAOperatingCostsTotalWithoutBadDebtProvisionTitleSection3(calcResult) * ConverttoDecimal(producerPercentage.ToString()) * OnePlusOtherParam * oneplause;
        }

        public static decimal GetSAOperatingCostsTotalWithBadDebtProvisionPrtoducerTotalSection3(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialsCostSummary, Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> costSummary, List<MaterialDetail> materials, CalcResult calcResult)
        {
            return GetSAOperatingCostsTotalWithoutBadDebtProvisionPrtoducerTotalSection3(materialsCostSummary, costSummary, materials, calcResult) + GetBadDebtProvisionPrtoducerTotalSection3(materialsCostSummary, costSummary, materials, calcResult);
        }

        public static decimal GetSAOperatingCostsTotalWithoutBadDebtProvisionTitleSection3(CalcResult calcResult)
        {
            return calcResult.CalcResultParameterOtherCost.SaOperatingCost.OrderByDescending(t => t.OrderId).FirstOrDefault().TotalValue; ;
        }

        public static decimal GetSAOperatingCostsBadDebtProvisionTitleSection3(decimal sacostvalue, CalcResult calcResult)
        {
            return sacostvalue * (calcResult.CalcResultParameterOtherCost.BadDebtValue / 100);
        }

        public static decimal GetSAOperatingCostsTotalWithBadDebtProvisionTitleSection3(CalcResultSummary result) => result.SAOperatingCostsWoTitleSection3 + result.BadDebtProvisionTitleSection3;

        public static decimal GetSAOperatingCostsNITotalWithBadDebtProvisionSection3(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialsCostSummary, Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> costSummary, List<MaterialDetail> materials, CalcResult calcResult)
        {
            //1+paramOthers
            Decimal OnePlusOtherParam = 1 + (calcResult.CalcResultParameterOtherCost.BadDebtValue / 100);

            //1+4 apportenmnt

            var oneplause = ConverttoDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Where(x => x.Name == "1 + 4 Apportionment %s").FirstOrDefault().NorthernIrelandDisposalTotal);

            var producerPercentage = GetTotal1Plus2ABadDebtPercentage(CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialsCostSummary), CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(costSummary), materials, calcResult);
            return GetSAOperatingCostsTotalWithoutBadDebtProvisionTitleSection3(calcResult) * ConverttoDecimal(producerPercentage.ToString()) * OnePlusOtherParam * oneplause;
        }

        public static decimal GetTotal1Plus2ABadDebtPercentage(decimal totalLaDisposal, decimal total2aCommsCost, List<MaterialDetail> materials, CalcResult calcResult)
        {
            var total = CalcResultSummaryBuilder.GetTotal1Plus2ABadDebt(materials, calcResult);

            if (total == 0) return 0;

            return Math.Round((totalLaDisposal + total2aCommsCost) / total * 100, 8);

        }

        public static decimal GetBadDebtProvisionPrtoducerTotalSection3(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialsCostSummary, Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> costSummary, List<MaterialDetail> materials, CalcResult calcResult)
        {
            decimal ThreesalTotalCost = GetSAOperatingCostsTotalWithoutBadDebtProvisionPrtoducerTotalSection3(materialsCostSummary, costSummary, materials, calcResult);

            return ThreesalTotalCost * (calcResult.CalcResultParameterOtherCost.BadDebtValue / 100);
        }

        public static decimal ConverttoDecimal(string parameter)
        {
            var isConversionSuccessful = decimal.TryParse(parameter.Replace("%", string.Empty), out decimal value);

            return isConversionSuccessful ? value / 100 : 0;
        }

        public static decimal GetSAOperatingCostsTotalWithoutBadDebtProvisionPrtoducerTotalSection3(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialsCostSummary, Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> costSummary, List<MaterialDetail> materials, CalcResult calcResult)
        {
            decimal Total1Plus2ABadDebtPercentage = CalcResultSummaryBuilder.GetTotal1Plus2ABadDebtPercentage(CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialsCostSummary), CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(costSummary), materials, calcResult);
            decimal TotalProvisioncost = GetSAOperatingCostsTotalWithoutBadDebtProvisionTitleSection3(calcResult);

            return Total1Plus2ABadDebtPercentage * TotalProvisioncost / 100; ;
        }
    }
}