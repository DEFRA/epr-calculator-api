using EPR.Calculator.API.Builder.CommsCost;
using EPR.Calculator.API.Builder.Summary.Common;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Utils;

namespace EPR.Calculator.API.Builder.Summary.LaDataPrepCosts
{
    public static class LaDataPrepCostsProducer
    {
        public static readonly int ColumnIndex = 217;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.TotalProducerFeeWithoutBadDebtProvision , ColumnIndex = ColumnIndex },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.BadDebtProvision, ColumnIndex = ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.TotalProducerFeeWithBadDebtProvision, ColumnIndex = ColumnIndex + 2 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.EnglandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 3 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.WalesTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 4 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.ScotlandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 5 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.NorthernIrelandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 6 }
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
            return GetLaDataPrepCostsProducerFeeWithoutBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary) *
                calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
        }

        public static decimal GetLaDataPrepCostsProducerFeeWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return GetLaDataPrepCostsProducerFeeWithoutBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary) +
                   GetLaDataPrepCostsBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary);
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
            return GetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotal(producers, producersAndSubsidiaries, materials, calcResult) *
                calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
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
            return GetCountryTotalWithBadDebtProvision(producers, materials, calcResult, materialCostSummary,
                materialCommsCostSummary, Countries.England);
        }

        public static decimal GetLaDataPrepCostsWalesTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return GetCountryTotalWithBadDebtProvision(producers, materials, calcResult, materialCostSummary,
                materialCommsCostSummary, Countries.Wales);
        }

        public static decimal GetLaDataPrepCostsScotlandTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return GetCountryTotalWithBadDebtProvision(producers, materials, calcResult, materialCostSummary,
                materialCommsCostSummary, Countries.Scotland);
        }

        public static decimal GetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return GetCountryTotalWithBadDebtProvision(producers, materials, calcResult, materialCostSummary,
                materialCommsCostSummary, Countries.NorthernIreland);
        }

        public static decimal GetLaDataPrepCostsEnglandOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            return GetCountryOverallTotalWithBadDebtProvision(producers, producersAndSubsidiaries, materials,
                calcResult, Countries.England);
        }

        public static decimal GetLaDataPrepCostsWalesOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            return GetCountryOverallTotalWithBadDebtProvision(producers, producersAndSubsidiaries, materials,
                calcResult, Countries.Wales);
        }

        public static decimal GetLaDataPrepCostsScotlandOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            return GetCountryOverallTotalWithBadDebtProvision(producers, producersAndSubsidiaries, materials,
                calcResult, Countries.Scotland);
        }

        public static decimal GetLaDataPrepCostsNorthernIrelandOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            return GetCountryOverallTotalWithBadDebtProvision(producers, producersAndSubsidiaries, materials,
                calcResult, Countries.NorthernIreland);
        }

        private static decimal GetCountryTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary,
            Countries country)
        {
            var laDataPrepCostsProducerFeeWithoutBadDebtProvision = LaDataPrepCostsSummary.GetLaDataPrepCostsWithoutBadDebtProvision(calcResult);

            var paramsOtherCalculated = 1 + (calcResult.CalcResultParameterOtherCost.BadDebtValue / 100);

            var totalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);
            var totalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(materialCommsCostSummary);

            var total1Plus2ABadDebt = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

            var producerPercentageOfOverallProducerCosts = ((totalProducerDisposalFeeWithBadDebtProvision + totalProducerCommsFeeWithBadDebtProvision) / total1Plus2ABadDebt) / 100;

            return laDataPrepCostsProducerFeeWithoutBadDebtProvision *
                   paramsOtherCalculated *
                   producerPercentageOfOverallProducerCosts *
                   CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, country);
        }

        private static decimal GetCountryOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult, Countries country)
        {
            var laDataPrepCostsProducerFeeWithoutBadDebtProvision = LaDataPrepCostsSummary.GetLaDataPrepCostsWithoutBadDebtProvision(calcResult);

            var paramsOtherCalculated = 1 + (calcResult.CalcResultParameterOtherCost.BadDebtValue / 100);

            var total1Plus2ABadDebtForAllProducers = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

            var total1Plus2ABadDebtForProducersAndSubsidiaries = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producersAndSubsidiaries, materials, calcResult);

            var producerPercentageOfOverallProducerCosts = (total1Plus2ABadDebtForProducersAndSubsidiaries / total1Plus2ABadDebtForAllProducers) / 100;

            return laDataPrepCostsProducerFeeWithoutBadDebtProvision *
                   paramsOtherCalculated *
                   producerPercentageOfOverallProducerCosts *
                       CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, country);
        }
    }
}
