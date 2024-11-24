
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.SchemeAdministratorSetupCosts
{
    public class SchemeAdministratorSetupCostsProducer
    {
        public decimal BadDebtProvision { get; set; }
        public decimal OneOffFeeSetupCostsTotalWithBadDebtProvision { get; set; }
        public decimal OneOffFeeSetupCostsTotalWithoutBadDebtProvision { get; set; }
        public decimal EnglandTotalWithBadDebtProvision { get; set; }
        public decimal WalesTotalWithBadDebtProvision { get; set; }
        public decimal ScotlandTotalWithBadDebtProvision { get; set; }
        public decimal NorthernIrelandTotalWithBadDebtProvision { get; set; }

        public SchemeAdministratorSetupCostsProducer()
        {
            this.Calculate();
        }

        public IEnumerable<CalcResultSummaryHeader> GetTitleHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.OneOffFeeSetupCostsWithoutBadDebtProvisionTitle, order = 1 },
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.BadDebtProvisionTitle, order = 2 },
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.OneOffFeeSetupCostsWithBadDebtProvisionTitle, order = 3 }
            ];
        }

        public IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.ProducerOneOffFeeWithoutBadDebtProvision, order = 1 },
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.BadDebtProvision, order = 2 },
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.ProducerOneOffFeeWithBadDebtProvision, order = 3 },
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.EnglandTotalWithBadDebtProvision, order = 4 },
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.WalesTotalWithBadDebtProvision, order = 5 },
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.ScotlandTotalWithBadDebtProvision, order = 6 },
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.NorthernIrelandTotalWithBadDebtProvision, order = 7 }
            ];
        }

        private void Calculate()
        {
            BadDebtProvision = GetBadDebtProvision();
            OneOffFeeSetupCostsTotalWithBadDebtProvision = GetOneOffFeeSetupCostsTotalWithBadDebtProvision();
            OneOffFeeSetupCostsTotalWithoutBadDebtProvision = GetOneOffFeeSetupCostsTotalWithoutBadDebtProvision();
            EnglandTotalWithBadDebtProvision = GetEnglandTotalWithBadDebtProvision();
            WalesTotalWithBadDebtProvision = GetWalesTotalWithBadDebtProvision();
            ScotlandTotalWithBadDebtProvision = GetScotlandTotalWithBadDebtProvision();
            NorthernIrelandTotalWithBadDebtProvision = GetNorthernIrelandTotalWithBadDebtProvision();
        }

        private decimal GetBadDebtProvision()
        {
            return 112;
        }

        private decimal GetOneOffFeeSetupCostsTotalWithBadDebtProvision()
        {
            return 113;
        }

        private decimal GetOneOffFeeSetupCostsTotalWithoutBadDebtProvision()
        {
            return 114;
        }

        private decimal GetEnglandTotalWithBadDebtProvision()
        {
            return 115;
        }

        private decimal GetWalesTotalWithBadDebtProvision()
        {
            return 116;
        }

        private decimal GetScotlandTotalWithBadDebtProvision()
        {
            return 117;
        }

        private decimal GetNorthernIrelandTotalWithBadDebtProvision()
        {
            return 118;
        }
    }
}
