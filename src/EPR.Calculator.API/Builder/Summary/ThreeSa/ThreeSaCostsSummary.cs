using EPR.Calculator.API.Builder.Summary.SaSetupCosts;
using EPR.Calculator.API.Builder.Summary.ThreeSa;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.ThreeSA
{
    public static class ThreeSaCostsSummary
    {
        public static readonly int ColumnIndex = 210;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = $"{ThreeSaCostHeader.SaOperatingCostsWithoutBadDebtProvisionTitleSection3}", ColumnIndex = ColumnIndex },
                new CalcResultSummaryHeader { Name = $"{ThreeSaCostHeader.BadDebtProvisionTitleSection3}", ColumnIndex = ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = $"{ThreeSaCostHeader.SaOperatingCostsWithBadDebtProvisionTitleSection3}", ColumnIndex = ColumnIndex + 2 }
            ];
        }

        public static decimal GetThreeSaCostsWithoutBadDebtProvision(CalcResult calcResult)
        {
            return calcResult.CalcResultParameterOtherCost.SaOperatingCost.OrderByDescending(t => t.OrderId).FirstOrDefault().TotalValue;
        }

        public static decimal GetBadDebtProvisionSection3(CalcResult calcResult)
        {
            return GetThreeSaCostsWithoutBadDebtProvision(calcResult) * calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
        }

        public static decimal GetThreeSaCostsWithBadDebtProvision(CalcResult calcResult)
        {
            return GetThreeSaCostsWithoutBadDebtProvision(calcResult) + GetBadDebtProvisionSection3(calcResult);
        }


        public static decimal GetSetUpBadDebtProvision(CalcResult calcResult)
        {
            return calcResult.CalcResultParameterOtherCost.BadDebtValue;
        }
    }
}