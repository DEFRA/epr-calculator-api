using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.LaDataPrepCosts
{
    public static class LaDataPrepCostsSummary
    {
        private static readonly int columnIndex = 216;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = $"{LaDataPrepCostsHeaders.LaDataPrepCostsWithoutBadDebtProvisionTitle}", ColumnIndex = columnIndex },
                new CalcResultSummaryHeader { Name = $"{LaDataPrepCostsHeaders.BadDebtProvisionTitle}", ColumnIndex = columnIndex + 1 },
                new CalcResultSummaryHeader { Name = $"{LaDataPrepCostsHeaders.LaDataPrepCostsWithBadDebtProvisionTitle}", ColumnIndex = columnIndex + 2 }
            ];
        }

        public static decimal GetBadDebtProvision()
        {
            return 109;
        }

        public static decimal GetLaDataPrepCostsWithoutBadDebtProvision()
        {
            return 110;
        }

        public static decimal GetLaDataPrepCostsWithBadDebtProvision()
        {
            return 111;
        }
    }
}
