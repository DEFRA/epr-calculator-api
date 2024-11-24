using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.SchemeAdministratorSetupCosts
{
    public class SchemeAdministratorSetupCostsSummary
    {
        private static readonly int columnIndex = 223;

        private decimal badDebtProvision;
        private decimal oneOffFeeSetupCostsWithBadDebtProvision;
        private decimal oneOffFeeSetupCostsWithoutBadDebtProvision;
        
        public SchemeAdministratorSetupCostsSummary()
        {
            this.Calculate();
        }

        public IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return[
                new CalcResultSummaryHeader { Name = $"{oneOffFeeSetupCostsWithoutBadDebtProvision}", ColumnIndex = columnIndex },
                new CalcResultSummaryHeader { Name = $"{badDebtProvision}" },
                new CalcResultSummaryHeader { Name = $"{oneOffFeeSetupCostsWithBadDebtProvision}" }
            ];
        }

        private void Calculate()
        {
            badDebtProvision = GetBadDebtProvision();
            oneOffFeeSetupCostsWithBadDebtProvision = GetOneOffFeeSetupCostsWithBadDebtProvision();
            oneOffFeeSetupCostsWithoutBadDebtProvision = GetOneOffFeeSetupCostsWithoutBadDebtProvision();
        }

        private decimal GetBadDebtProvision()
        {
            return 109;
        }

        private decimal GetOneOffFeeSetupCostsWithBadDebtProvision()
        {
            return 110;
        }

        private decimal GetOneOffFeeSetupCostsWithoutBadDebtProvision()
        {
            return 111;
        }
    }
}
