using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.SchemeAdministratorSetupCosts
{
    public static class SchemeAdministratorSetupCostsSummary
    {
        private static readonly int columnIndex = 223;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return[
                new CalcResultSummaryHeader { Name = $"{SchemeAdministratorSetupCostsHeaders.OneOffFeeSetupCostsWithoutBadDebtProvisionTitle}", ColumnIndex = columnIndex },
                new CalcResultSummaryHeader { Name = $"{SchemeAdministratorSetupCostsHeaders.BadDebtProvisionTitle}", ColumnIndex = columnIndex + 1 },
                new CalcResultSummaryHeader { Name = $"{SchemeAdministratorSetupCostsHeaders.OneOffFeeSetupCostsWithBadDebtProvisionTitle}", ColumnIndex = columnIndex + 2 }
            ];
        }

        public static decimal GetBadDebtProvision()
        {
            return 109;
        }

        public static decimal GetOneOffFeeSetupCostsWithBadDebtProvision()
        {
            return 110;
        }

        public static decimal GetOneOffFeeSetupCostsWithoutBadDebtProvision()
        {
            return 111;
        }
    }
}
