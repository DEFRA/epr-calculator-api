using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.SaSetupCosts
{
    public static class SaSetupCostsSummary
    {
        public static readonly int ColumnIndex = 258;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = $"{SaSetupCostsHeaders.OneOffFeeSetupCostsWithoutBadDebtProvisionTitle}", ColumnIndex = ColumnIndex },
                new CalcResultSummaryHeader { Name = $"{SaSetupCostsHeaders.BadDebtProvisionTitle}", ColumnIndex = ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = $"{SaSetupCostsHeaders.OneOffFeeSetupCostsWithBadDebtProvisionTitle}", ColumnIndex = ColumnIndex + 2 }
            ];
        }

        public static decimal GetOneOffFeeSetupCostsWithoutBadDebtProvision(CalcResult calcResult)
        {
            return calcResult.CalcResultParameterOtherCost.SchemeSetupCost.TotalValue;
        }

        public static decimal GetBadDebtProvision(CalcResult calcResult)
        {
            return GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult) * calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
        }

        public static decimal GetOneOffFeeSetupCostsWithBadDebtProvision(CalcResult calcResult)
        {
            return GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult) + GetBadDebtProvision(calcResult);
        }

        public static decimal GetSetUpBadDebtProvision(CalcResult calcResult)
        {
            return calcResult.CalcResultParameterOtherCost.BadDebtValue;
        }
    }
}
