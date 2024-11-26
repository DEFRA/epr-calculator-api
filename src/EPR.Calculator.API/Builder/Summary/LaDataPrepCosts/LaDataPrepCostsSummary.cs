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

        public static decimal GetLaDataPrepCostsWithoutBadDebtProvision(CalcResult calcResult)
        {
            var dataPrepCharge = calcResult.CalcResultParameterOtherCost.Details.FirstOrDefault(cost => cost.Name == "4 LA Data Prep Charge");

            if (dataPrepCharge != null)
            {
                return dataPrepCharge.TotalValue;
            }

            return 0;
        }

        public static decimal GetBadDebtProvision(CalcResult calcResult)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                return GetLaDataPrepCostsWithoutBadDebtProvision(calcResult) * value / 100;
            }

            return 0;
        }

        public static decimal GetLaDataPrepCostsWithBadDebtProvision(CalcResult calcResult)
        {
            return GetLaDataPrepCostsWithoutBadDebtProvision(calcResult) + GetBadDebtProvision(calcResult);
        }
    }
}
