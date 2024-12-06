using EPR.Calculator.API.Builder.CommsCost;
using EPR.Calculator.API.Builder.Summary.Common;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.SaSetupCosts
{
    public static class SaSetupCostsProducer
    {
        public static readonly int ColumnIndex = 224;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.ProducerOneOffFeeWithoutBadDebtProvision, ColumnIndex = ColumnIndex },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.BadDebtProvision, ColumnIndex = ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.ProducerOneOffFeeWithBadDebtProvision, ColumnIndex = ColumnIndex + 2 },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.EnglandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 3 },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.WalesTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 4 },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.ScotlandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 5 },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.NorthernIrelandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 6 }
            ];
        }

        public static decimal GetProducerOneOffFeeWithoutBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            var oneOffFeeSetupCostsWithoutBadDebtProvision = SaSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);

            var totalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);
            var totalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(materialCommsCostSummary);

            var total1Plus2ABadDebt = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

            if (total1Plus2ABadDebt == 0)
            {
                return 0;
            }

            var producerPercentageOfOverallProducerCosts = (totalProducerDisposalFeeWithBadDebtProvision + totalProducerCommsFeeWithBadDebtProvision) / total1Plus2ABadDebt;

            return producerPercentageOfOverallProducerCosts * oneOffFeeSetupCostsWithoutBadDebtProvision;
        }

        public static decimal GetBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return GetProducerOneOffFeeWithoutBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary) *
                calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
        }

        public static decimal GetProducerOneOffFeeWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return GetProducerOneOffFeeWithoutBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary) +
                GetBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary);
        }

        public static decimal GetProducerOneOffFeeWithoutBadDebtProvisionTotal(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            var oneOffFeeSetupCostsWithoutBadDebtProvision = SaSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);

            var total1Plus2ABadDebtForAllProducers = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

            var total1Plus2ABadDebtForProducersAndSubsidiaries = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producersAndSubsidiaries, materials, calcResult);

            if (total1Plus2ABadDebtForAllProducers == 0)
            {
                return 0;
            }

            var producerPercentageOfOverallProducerCosts = total1Plus2ABadDebtForProducersAndSubsidiaries / total1Plus2ABadDebtForAllProducers;

            return producerPercentageOfOverallProducerCosts * oneOffFeeSetupCostsWithoutBadDebtProvision;
        }

        public static decimal GetBadDebtProvisionTotal(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            return GetProducerOneOffFeeWithoutBadDebtProvisionTotal(producers, producersAndSubsidiaries, materials, calcResult) *
                calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
        }

        public static decimal GetProducerOneOffFeeWithBadDebtProvisionTotal(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            return GetProducerOneOffFeeWithoutBadDebtProvisionTotal(producers, producersAndSubsidiaries, materials, calcResult) +
                GetBadDebtProvisionTotal(producers, producersAndSubsidiaries, materials, calcResult);
        }

        public static decimal GetEnglandTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return GetCountryTotalWithBadDebtProvision(producers, materials, calcResult, materialCostSummary,
                materialCommsCostSummary, Countries.England);
        }

        public static decimal GetWalesTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return GetCountryTotalWithBadDebtProvision(producers, materials, calcResult, materialCostSummary,
                materialCommsCostSummary, Countries.Wales);
        }

        public static decimal GetScotlandTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return GetCountryTotalWithBadDebtProvision(producers, materials, calcResult, materialCostSummary,
                materialCommsCostSummary, Countries.Scotland);
        }

        public static decimal GetNorthernIrelandTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return GetCountryTotalWithBadDebtProvision(producers, materials, calcResult, materialCostSummary,
                materialCommsCostSummary, Countries.NorthernIreland);
        }

        public static decimal GetEnglandOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            return GetCountryOverallTotalWithBadDebtProvision(producers, producersAndSubsidiaries, materials,
                calcResult, Countries.England);
        }

        public static decimal GetWalesOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            return GetCountryOverallTotalWithBadDebtProvision(producers, producersAndSubsidiaries, materials,
                calcResult, Countries.Wales);
        }

        public static decimal GetScotlandOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            return GetCountryOverallTotalWithBadDebtProvision(producers, producersAndSubsidiaries, materials,
                calcResult, Countries.Scotland);
        }

        public static decimal GetNorthernIrelandOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
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
            var oneOffFeeSetupCostsWithoutBadDebtProvision = SaSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);

            var paramsOtherCalculated = 1 + (calcResult.CalcResultParameterOtherCost.BadDebtValue / 100);

            var totalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);
            var totalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(materialCommsCostSummary);

            var total1Plus2ABadDebt = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

            if (total1Plus2ABadDebt == 0)
            {
                return 0;
            }

            var producerPercentageOfOverallProducerCosts = ((totalProducerDisposalFeeWithBadDebtProvision + totalProducerCommsFeeWithBadDebtProvision) / total1Plus2ABadDebt) / 100;

            return oneOffFeeSetupCostsWithoutBadDebtProvision *
                   paramsOtherCalculated *
                   producerPercentageOfOverallProducerCosts *
                   GetCountryOnePlusFourApportionment(calcResult, country);
        }

        private static decimal GetCountryOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult, Countries country)
        {
            var oneOffFeeSetupCostsWithoutBadDebtProvision = SaSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);

            var paramsOtherCalculated = 1 + (calcResult.CalcResultParameterOtherCost.BadDebtValue / 100);

            var total1Plus2ABadDebtForAllProducers = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

            var total1Plus2ABadDebtForProducersAndSubsidiaries = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producersAndSubsidiaries, materials, calcResult);

            if (total1Plus2ABadDebtForAllProducers == 0)
            {
                return 0;
            }

            var producerPercentageOfOverallProducerCosts = (total1Plus2ABadDebtForProducersAndSubsidiaries / total1Plus2ABadDebtForAllProducers) / 100;

            return oneOffFeeSetupCostsWithoutBadDebtProvision *
                   paramsOtherCalculated *
                   producerPercentageOfOverallProducerCosts *
                   GetCountryOnePlusFourApportionment(calcResult, country);
        }

        private static decimal GetCountryOnePlusFourApportionment(CalcResult calcResult, Countries country)
        {
            var onePlusFourApportionment = calcResult.CalcResultOnePlusFourApportionment
                .CalcResultOnePlusFourApportionmentDetails
                .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment);

            switch (country)
            {
                case Countries.England:
                    return onePlusFourApportionment.EnglandTotal;
                case Countries.Wales:
                    return onePlusFourApportionment.WalesTotal;
                case Countries.Scotland:
                    return onePlusFourApportionment.ScotlandTotal;
                case Countries.NorthernIreland:
                    return onePlusFourApportionment.NorthernIrelandTotal;
            }

            return 0;
        }
    }
}
