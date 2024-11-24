using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.SchemeAdministratorSetupCosts
{
    public class SchemeAdministratorSetupCostsSummary
    {
        private static readonly int columnIndex = 223;

        public decimal BadDebtProvision { get; set; }
        public decimal OneOffFeeSetupCostsWithBadDebtProvision { get; set; }
        public decimal OneOffFeeSetupCostsWithoutBadDebtProvision { get; set; }

        public SchemeAdministratorSetupCostsSummary()
        {
            this.Calculate();
        }

        public IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return[
                new CalcResultSummaryHeader { Name = $"{OneOffFeeSetupCostsWithoutBadDebtProvision}", ColumnIndex = columnIndex },
                new CalcResultSummaryHeader { Name = $"{BadDebtProvision}" },
                new CalcResultSummaryHeader { Name = $"{OneOffFeeSetupCostsWithBadDebtProvision}" }
            ];
        }

        private void Calculate()
        {
            BadDebtProvision = GetBadDebtProvision();
            OneOffFeeSetupCostsWithBadDebtProvision = GetOneOffFeeSetupCostsWithBadDebtProvision();
            OneOffFeeSetupCostsWithoutBadDebtProvision = GetOneOffFeeSetupCostsWithoutBadDebtProvision();
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
