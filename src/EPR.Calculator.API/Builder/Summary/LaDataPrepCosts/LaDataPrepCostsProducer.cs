using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.LaDataPrepCosts
{
    public static class LaDataPrepCostsProducer
    {
        private static readonly int columnIndex = 223;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.TotalProducerFeeWithoutBadDebtProvision , ColumnIndex = columnIndex },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.BadDebtProvision, ColumnIndex = columnIndex + 1 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.TotalProducerFeeWithBadDebtProvision, ColumnIndex = columnIndex + 2 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.EnglandTotalWithBadDebtProvision, ColumnIndex = columnIndex + 3 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.WalesTotalWithBadDebtProvision, ColumnIndex = columnIndex + 4 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.ScotlandTotalWithBadDebtProvision, ColumnIndex = columnIndex + 5 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.NorthernIrelandTotalWithBadDebtProvision, ColumnIndex = columnIndex + 6 }
            ];
        }

        public static decimal GetBadDebtProvision1(CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            decimal materialBadDebtProvision = Util.GetBadDebtProvisionForMaterials(materialCostSummary);
            decimal materialCommsCostBadDebtProvision = Util.GetBadDebtProvisionForMaterialsCommsCost(materialCommsCostSummary);




            var totalOnePlus2AFeeWithBadDebtProvision = Util.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);

            var abc = calcResult.CalcResultSummary.ProducerDisposalFees.Where(x => x.Level == "Totals").FirstOrDefault().TotalOnePlus2AFeeWithBadDebtProvision;

            // var result = (totalBadDebtProvision + totalCommsBadDebtProvision) / ()

            return 112;
        }

        public static decimal GetBadDebtProvision()
        {
            return 112;
        }

        public static decimal GetBadDebtProvisionTotal()
        {
            return 112;
        }

        public static decimal GetProducerFeeWithoutBadDebtProvision(CalcResult calcResult)
        {


            return 113;
        }

        public static decimal GetProducerFeeWithoutBadDebtProvisionTotal()
        {
            return 113;
        }

        public static decimal GetProducerFeeWithBadDebtProvision()
        {
            return 114;
        }

        public static decimal GetProducerFeeWithBadDebtProvisionTotal()
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
