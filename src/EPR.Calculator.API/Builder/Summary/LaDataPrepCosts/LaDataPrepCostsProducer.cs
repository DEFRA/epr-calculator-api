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

        public static decimal GetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotal(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            var laDataPrepCostsWithoutBadDebtProvision = LaDataPrepCostsSummary.GetLaDataPrepCostsWithoutBadDebtProvision(calcResult);

            var total1Plus2ABadDebtForAllProducers = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

            var total1Plus2ABadDebtForProducersAndSubsidiaries = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producersAndSubsidiaries, materials, calcResult);

            var producerPercentageOfOverallProducerCosts = total1Plus2ABadDebtForProducersAndSubsidiaries / total1Plus2ABadDebtForAllProducers;

            return producerPercentageOfOverallProducerCosts * laDataPrepCostsWithoutBadDebtProvision;
        }

        public static decimal GetLaDataPrepCostsBadDebtProvisionTotal(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                return GetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotal(producers, producersAndSubsidiaries, materials, calcResult) * value / 100;
            }

            return 0;
        }

        public static decimal GetLaDataPrepCostsProducerFeeWithBadDebtProvisionTotal(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            return GetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotal(producers, producersAndSubsidiaries, materials, calcResult) +
                GetLaDataPrepCostsBadDebtProvisionTotal(producers, producersAndSubsidiaries, materials, calcResult);
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
                var laDataPrepCostsProducerFeeWithoutBadDebtProvision = LaDataPrepCostsSummary.GetLaDataPrepCostsWithoutBadDebtProvision(calcResult);

                var paramsOtherCalculated = 1 + (value / 100);

                var totalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);
                var totalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(materialCommsCostSummary);

                var total1Plus2ABadDebt = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

                var producerPercentageOfOverallProducerCosts = ((totalProducerDisposalFeeWithBadDebtProvision + totalProducerCommsFeeWithBadDebtProvision) / total1Plus2ABadDebt) / 100;

                var englandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).EnglandTotal;

                return laDataPrepCostsProducerFeeWithoutBadDebtProvision * paramsOtherCalculated * producerPercentageOfOverallProducerCosts * englandTotal;
            }

            return 0;
        }

        public static decimal GetLaDataPrepCostsEnglandOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                var laDataPrepCostsProducerFeeWithoutBadDebtProvision = LaDataPrepCostsSummary.GetLaDataPrepCostsWithoutBadDebtProvision(calcResult);

                var paramsOtherCalculated = 1 + (value / 100);

                var total1Plus2ABadDebtForAllProducers = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

                var total1Plus2ABadDebtForProducersAndSubsidiaries = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producersAndSubsidiaries, materials, calcResult);

                var producerPercentageOfOverallProducerCosts = (total1Plus2ABadDebtForProducersAndSubsidiaries / total1Plus2ABadDebtForAllProducers) / 100;

                var englandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).EnglandTotal;

                return laDataPrepCostsProducerFeeWithoutBadDebtProvision * paramsOtherCalculated * producerPercentageOfOverallProducerCosts * englandTotal;
            }

            return 0;
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
                var laDataPrepCostsProducerFeeWithoutBadDebtProvision = LaDataPrepCostsSummary.GetLaDataPrepCostsWithoutBadDebtProvision(calcResult);

                var paramsOtherCalculated = 1 + (value / 100);

                var totalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);
                var totalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(materialCommsCostSummary);

                var total1Plus2ABadDebt = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

                var producerPercentageOfOverallProducerCosts = ((totalProducerDisposalFeeWithBadDebtProvision + totalProducerCommsFeeWithBadDebtProvision) / total1Plus2ABadDebt) / 100;

                var walesTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).WalesTotal;

                return laDataPrepCostsProducerFeeWithoutBadDebtProvision * paramsOtherCalculated * producerPercentageOfOverallProducerCosts * walesTotal;
            }

            return 0;
        }

        public static decimal GetLaDataPrepCostsWalesOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                var laDataPrepCostsProducerFeeWithoutBadDebtProvision = LaDataPrepCostsSummary.GetLaDataPrepCostsWithoutBadDebtProvision(calcResult);

                var paramsOtherCalculated = 1 + (value / 100);

                var total1Plus2ABadDebtForAllProducers = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

                var total1Plus2ABadDebtForProducersAndSubsidiaries = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producersAndSubsidiaries, materials, calcResult);

                var producerPercentageOfOverallProducerCosts = (total1Plus2ABadDebtForProducersAndSubsidiaries / total1Plus2ABadDebtForAllProducers) / 100;

                var walesTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).WalesTotal;

                return laDataPrepCostsProducerFeeWithoutBadDebtProvision * paramsOtherCalculated * producerPercentageOfOverallProducerCosts * walesTotal;
            }

            return 0;
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
                var laDataPrepCostsProducerFeeWithoutBadDebtProvision = LaDataPrepCostsSummary.GetLaDataPrepCostsWithoutBadDebtProvision(calcResult);

                var paramsOtherCalculated = 1 + (value / 100);

                var totalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);
                var totalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(materialCommsCostSummary);

                var total1Plus2ABadDebt = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

                var producerPercentageOfOverallProducerCosts = ((totalProducerDisposalFeeWithBadDebtProvision + totalProducerCommsFeeWithBadDebtProvision) / total1Plus2ABadDebt) / 100;

                var scotlandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).ScotlandTotal;

                return laDataPrepCostsProducerFeeWithoutBadDebtProvision * paramsOtherCalculated * producerPercentageOfOverallProducerCosts * scotlandTotal;
            }

            return 0;
        }

        public static decimal GetLaDataPrepCostsScotlandOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                var laDataPrepCostsProducerFeeWithoutBadDebtProvision = LaDataPrepCostsSummary.GetLaDataPrepCostsWithoutBadDebtProvision(calcResult);

                var paramsOtherCalculated = 1 + (value / 100);

                var total1Plus2ABadDebtForAllProducers = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

                var total1Plus2ABadDebtForProducersAndSubsidiaries = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producersAndSubsidiaries, materials, calcResult);

                var producerPercentageOfOverallProducerCosts = (total1Plus2ABadDebtForProducersAndSubsidiaries / total1Plus2ABadDebtForAllProducers) / 100;

                var scotlandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).ScotlandTotal;

                return laDataPrepCostsProducerFeeWithoutBadDebtProvision * paramsOtherCalculated * producerPercentageOfOverallProducerCosts * scotlandTotal;
            }

            return 0;
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
                var laDataPrepCostsProducerFeeWithoutBadDebtProvision = LaDataPrepCostsSummary.GetLaDataPrepCostsWithoutBadDebtProvision(calcResult);

                var paramsOtherCalculated = 1 + (value / 100);

                var totalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);
                var totalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(materialCommsCostSummary);

                var total1Plus2ABadDebt = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

                var producerPercentageOfOverallProducerCosts = ((totalProducerDisposalFeeWithBadDebtProvision + totalProducerCommsFeeWithBadDebtProvision) / total1Plus2ABadDebt) / 100;

                var northernIrelandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).NorthernIrelandTotal;

                return laDataPrepCostsProducerFeeWithoutBadDebtProvision * paramsOtherCalculated * producerPercentageOfOverallProducerCosts * northernIrelandTotal;
            }

            return 0;
        }

        public static decimal GetLaDataPrepCostsNorthernIrelandOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                var laDataPrepCostsProducerFeeWithoutBadDebtProvision = LaDataPrepCostsSummary.GetLaDataPrepCostsWithoutBadDebtProvision(calcResult);

                var paramsOtherCalculated = 1 + (value / 100);

                var total1Plus2ABadDebtForAllProducers = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

                var total1Plus2ABadDebtForProducersAndSubsidiaries = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producersAndSubsidiaries, materials, calcResult);

                var producerPercentageOfOverallProducerCosts = (total1Plus2ABadDebtForProducersAndSubsidiaries / total1Plus2ABadDebtForAllProducers) / 100;

                var northernIrelandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).NorthernIrelandTotal;

                return laDataPrepCostsProducerFeeWithoutBadDebtProvision * paramsOtherCalculated * producerPercentageOfOverallProducerCosts * northernIrelandTotal;
            }

            return 0;
        }
    }
}
