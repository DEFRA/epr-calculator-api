using EPR.Calculator.API.Builder.CommsCost;
using EPR.Calculator.API.Builder.Summary.Common;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Utils;

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

        public static decimal GetLaDataPrepCostsProducerFeeWithoutBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            var laDataPrepCostsWithoutBadDebtProvision = LaDataPrepCostsSummary.GetLaDataPrepCostsWithoutBadDebtProvision(calcResult);

            var totalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);
            var totalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(materialCommsCostSummary);

            var total1Plus2ABadDebt = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

            var producerPercentageOfOverallProducerCosts = (totalProducerDisposalFeeWithBadDebtProvision + totalProducerCommsFeeWithBadDebtProvision) / total1Plus2ABadDebt;

            return producerPercentageOfOverallProducerCosts * laDataPrepCostsWithoutBadDebtProvision;
        }

        public static decimal GetLaDataPrepCostsBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                return GetLaDataPrepCostsProducerFeeWithoutBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary) * value / 100;
            }

            return 0;
        }

        public static decimal GetLaDataPrepCostsProducerFeeWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return GetLaDataPrepCostsProducerFeeWithoutBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary) + GetLaDataPrepCostsBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary);
        }

        public static decimal GetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotal(CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return 114;
        }

        public static decimal GetLaDataPrepCostsBadDebtProvisionTotal()
        {
            return 112;
        }

        public static decimal GetLaDataPrepCostsProducerFeeWithBadDebtProvisionTotal()
        {
            return 115;
        }

        public static decimal GetLaDataPrepCostsEnglandTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                var englandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).EnglandTotal;

                return LaDataPrepCostsSummary.GetLaDataPrepCostsWithBadDebtProvision(calcResult) *
                    value *
                    GetLaDataPrepCostsProducerFeeWithBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary) *
                    englandTotal;
            }

            return 0;
        }

        public static decimal GetLaDataPrepCostsEnglandOverallTotalWithBadDebtProvision()
        {
            return 116;
        }

        public static decimal GetLaDataPrepCostsWalesTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                var walesTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).WalesTotal;

                return LaDataPrepCostsSummary.GetLaDataPrepCostsWithBadDebtProvision(calcResult) *
                    value *
                    GetLaDataPrepCostsProducerFeeWithBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary) *
                    walesTotal;
            }

            return 0;
        }

        public static decimal GetLaDataPrepCostsWalesOverallTotalWithBadDebtProvision()
        {
            return 116;
        }

        public static decimal GetLaDataPrepCostsScotlandTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                var scotlandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).ScotlandTotal;

                return LaDataPrepCostsSummary.GetLaDataPrepCostsWithBadDebtProvision(calcResult) *
                    value *
                    GetLaDataPrepCostsProducerFeeWithBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary) *
                    scotlandTotal;
            }

            return 0;
        }

        public static decimal GetLaDataPrepCostsScotlandOverallTotalWithBadDebtProvision()
        {
            return 117;
        }

        public static decimal GetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                var northernIrelandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).NorthernIrelandTotal;

                return LaDataPrepCostsSummary.GetLaDataPrepCostsWithBadDebtProvision(calcResult) *
                    value *
                    GetLaDataPrepCostsProducerFeeWithBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary) *
                    northernIrelandTotal;
            }

            return 0;
        }

        public static decimal GetLaDataPrepCostsNorthernIrelandOverallTotalWithBadDebtProvision()
        {
            return 118;
        }
    }
}
