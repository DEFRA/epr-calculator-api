using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.LaDataPrepCosts
{
    public static class LaDataPrepCostsSummary
    {
        public static readonly int ColumnIndex = 217;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = $"{LaDataPrepCostsHeaders.LaDataPrepCostsWithoutBadDebtProvisionTitle}", ColumnIndex = ColumnIndex },
                new CalcResultSummaryHeader { Name = $"{LaDataPrepCostsHeaders.BadDebtProvisionTitle}", ColumnIndex = ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = $"{LaDataPrepCostsHeaders.LaDataPrepCostsWithBadDebtProvisionTitle}", ColumnIndex = ColumnIndex + 2 }
            ];
        }

        public static decimal GetLaDataPrepCostsWithoutBadDebtProvision(CalcResult calcResult)
        {
            var dataPrepCharge = calcResult.CalcResultParameterOtherCost.Details.FirstOrDefault(
                cost => cost.Name == OnePlus4ApportionmentColumnHeaders.LADataPrepCharge);

            if (dataPrepCharge != null)
            {
                return dataPrepCharge.TotalValue;
            }

            return 0;
        }

        public static decimal GetBadDebtProvision(CalcResult calcResult)
        {
            return GetLaDataPrepCostsWithoutBadDebtProvision(calcResult) * calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
        }

        public static decimal GetLaDataPrepCostsWithBadDebtProvision(CalcResult calcResult)
        {
            return GetLaDataPrepCostsWithoutBadDebtProvision(calcResult) + GetBadDebtProvision(calcResult);
        }
    }
}
