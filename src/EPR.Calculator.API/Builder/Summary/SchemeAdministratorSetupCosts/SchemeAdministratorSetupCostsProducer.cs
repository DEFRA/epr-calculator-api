
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.SchemeAdministratorSetupCosts
{
    public static class SchemeAdministratorSetupCostsProducer
    {
        private static readonly int columnIndex = 223;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.ProducerOneOffFeeWithoutBadDebtProvision, ColumnIndex = columnIndex },
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.BadDebtProvision, ColumnIndex = columnIndex + 1 },
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.ProducerOneOffFeeWithBadDebtProvision, ColumnIndex = columnIndex + 2 },
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.EnglandTotalWithBadDebtProvision, ColumnIndex = columnIndex + 3 },
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.WalesTotalWithBadDebtProvision, ColumnIndex = columnIndex + 4 },
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.ScotlandTotalWithBadDebtProvision, ColumnIndex = columnIndex + 5 },
                new CalcResultSummaryHeader { Name = SchemeAdministratorSetupCostsHeaders.NorthernIrelandTotalWithBadDebtProvision, ColumnIndex = columnIndex + 6 }
            ];
        }

        public static decimal GetBadDebtProvision()
        {
            return 112;
        }
        public static decimal GetBadDebtProvisionTotal()
        {
            return 112;
        }


        public static decimal GetProducerOneOffFeeWithoutBadDebtProvision()
        {
            return 113;
        }

        public static decimal GetOneOffFeeWithoutBadDebtProvisionTotal()
        {
            return 113;
        }

        public static decimal GetProducerOneOffFeeWithBadDebtProvision()
        {
            return 114;
        }

        public static decimal GetOneOffFeeWithBadDebtProvisionTotal()
        {
            return 115;
        }

        public static decimal GetEnglandTotalWithBadDebtProvision()
        {
            return 116;
        }

        public static decimal GetEnglandOverallTotalWithBadDebtProvision()
        {
            return 116;
        }

        public static decimal GetWalesTotalWithBadDebtProvision()
        {
            return 116;
        }

        public static decimal GetWalesOverallTotalWithBadDebtProvision()
        {
            return 116;
        }

        public static decimal GetScotlandTotalWithBadDebtProvision()
        {
            return 117;
        }

        public static decimal GetScotlandOverallTotalWithBadDebtProvision()
        {
            return 117;
        }

        public static decimal GetNorthernIrelandTotalWithBadDebtProvision()
        {
            return 118;
        }

        public static decimal GetNorthernIrelandOverallTotalWithBadDebtProvision()
        {
            return 118;
        }
    }
}
