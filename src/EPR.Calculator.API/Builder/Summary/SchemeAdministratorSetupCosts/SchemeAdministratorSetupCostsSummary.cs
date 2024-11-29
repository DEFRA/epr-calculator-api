using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.SchemeAdministratorSetupCosts
{
    public static class SchemeAdministratorSetupCostsSummary
    {
        public static readonly int columnIndex = 224;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return[
                new CalcResultSummaryHeader { Name = $"{SchemeAdministratorSetupCostsHeaders.OneOffFeeSetupCostsWithoutBadDebtProvisionTitle}", ColumnIndex = columnIndex },
                new CalcResultSummaryHeader { Name = $"{SchemeAdministratorSetupCostsHeaders.BadDebtProvisionTitle}", ColumnIndex = columnIndex + 1 },
                new CalcResultSummaryHeader { Name = $"{SchemeAdministratorSetupCostsHeaders.OneOffFeeSetupCostsWithBadDebtProvisionTitle}", ColumnIndex = columnIndex + 2 }
            ];
        }

        public static decimal GetOneOffFeeSetupCostsWithoutBadDebtProvision(CalcResult calcResult)
        {
            return calcResult.CalcResultParameterOtherCost.SchemeSetupCost.TotalValue;
        }

        public static decimal GetBadDebtProvision(CalcResult calcResult)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                return GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult) * value / 100;
            }

            return 0;
        }

        public static decimal GetOneOffFeeSetupCostsWithBadDebtProvision(CalcResult calcResult)
        {
            return GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult) + GetBadDebtProvision(calcResult);
        }
    }
}
