
using EPR.Calculator.API.Builder.CommsCost;
using EPR.Calculator.API.Builder.Summary.Common;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.SchemeAdministratorSetupCosts
{
    public static class SchemeAdministratorSetupCostsProducer
    {
        private static readonly int columnIndex = 224;

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

        public static decimal GetProducerOneOffFeeWithoutBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            var oneOffFeeSetupCostsWithoutBadDebtProvision = SchemeAdministratorSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);

            var totalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);
            var totalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(materialCommsCostSummary);

            var total1Plus2ABadDebt = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

            var producerPercentageOfOverallProducerCosts = (totalProducerDisposalFeeWithBadDebtProvision + totalProducerCommsFeeWithBadDebtProvision) / total1Plus2ABadDebt;

            return producerPercentageOfOverallProducerCosts * oneOffFeeSetupCostsWithoutBadDebtProvision;
        }

        public static decimal GetBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                return GetProducerOneOffFeeWithoutBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary) * value / 100;
            }

            return 0;
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
            var oneOffFeeSetupCostsWithoutBadDebtProvision = SchemeAdministratorSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);

            var total1Plus2ABadDebtForAllProducers = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

            var total1Plus2ABadDebtForProducersAndSubsidiaries = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producersAndSubsidiaries, materials, calcResult);

            var producerPercentageOfOverallProducerCosts = total1Plus2ABadDebtForProducersAndSubsidiaries / total1Plus2ABadDebtForAllProducers;

            return producerPercentageOfOverallProducerCosts * oneOffFeeSetupCostsWithoutBadDebtProvision;
        }

        public static decimal GetBadDebtProvisionTotal(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                return GetProducerOneOffFeeWithoutBadDebtProvisionTotal(producers, producersAndSubsidiaries, materials, calcResult) * value / 100;
            }

            return 0;
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
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                var oneOffFeeSetupCostsWithoutBadDebtProvision = SchemeAdministratorSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);

                var paramsOtherCalculated = 1 + (value / 100);

                var totalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);
                var totalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(materialCommsCostSummary);

                var total1Plus2ABadDebt = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

                var producerPercentageOfOverallProducerCosts = ((totalProducerDisposalFeeWithBadDebtProvision + totalProducerCommsFeeWithBadDebtProvision) / total1Plus2ABadDebt) / 100;

                var englandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).EnglandTotal;

                return oneOffFeeSetupCostsWithoutBadDebtProvision * paramsOtherCalculated * producerPercentageOfOverallProducerCosts * englandTotal;
            }

            return 0;
        }

        public static decimal GetEnglandOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                var oneOffFeeSetupCostsWithoutBadDebtProvision = SchemeAdministratorSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);

                var paramsOtherCalculated = 1 + (value / 100);

                var total1Plus2ABadDebtForAllProducers = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

                var total1Plus2ABadDebtForProducersAndSubsidiaries = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producersAndSubsidiaries, materials, calcResult);

                var producerPercentageOfOverallProducerCosts = (total1Plus2ABadDebtForProducersAndSubsidiaries / total1Plus2ABadDebtForAllProducers) / 100;

                var englandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).EnglandTotal;

                return oneOffFeeSetupCostsWithoutBadDebtProvision * paramsOtherCalculated * producerPercentageOfOverallProducerCosts * englandTotal;
            }

            return 0;
        }

        public static decimal GetWalesTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                var oneOffFeeSetupCostsWithoutBadDebtProvision = SchemeAdministratorSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);

                var paramsOtherCalculated = 1 + (value / 100);

                var totalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);
                var totalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(materialCommsCostSummary);

                var total1Plus2ABadDebt = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

                var producerPercentageOfOverallProducerCosts = ((totalProducerDisposalFeeWithBadDebtProvision + totalProducerCommsFeeWithBadDebtProvision) / total1Plus2ABadDebt) / 100;

                var walesTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).WalesTotal;

                return oneOffFeeSetupCostsWithoutBadDebtProvision * paramsOtherCalculated * producerPercentageOfOverallProducerCosts * walesTotal;
            }

            return 0;
        }

        public static decimal GetWalesOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                var oneOffFeeSetupCostsWithoutBadDebtProvision = SchemeAdministratorSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);

                var paramsOtherCalculated = 1 + (value / 100);

                var total1Plus2ABadDebtForAllProducers = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

                var total1Plus2ABadDebtForProducersAndSubsidiaries = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producersAndSubsidiaries, materials, calcResult);

                var producerPercentageOfOverallProducerCosts = (total1Plus2ABadDebtForProducersAndSubsidiaries / total1Plus2ABadDebtForAllProducers) / 100;

                var walesTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).WalesTotal;

                return oneOffFeeSetupCostsWithoutBadDebtProvision * paramsOtherCalculated * producerPercentageOfOverallProducerCosts * walesTotal;
            }

            return 0;
        }

        public static decimal GetScotlandTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                var oneOffFeeSetupCostsWithoutBadDebtProvision = SchemeAdministratorSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);

                var paramsOtherCalculated = 1 + (value / 100);

                var totalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);
                var totalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(materialCommsCostSummary);

                var total1Plus2ABadDebt = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

                var producerPercentageOfOverallProducerCosts = ((totalProducerDisposalFeeWithBadDebtProvision + totalProducerCommsFeeWithBadDebtProvision) / total1Plus2ABadDebt) / 100;

                var scotlandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).ScotlandTotal;

                return oneOffFeeSetupCostsWithoutBadDebtProvision * paramsOtherCalculated * producerPercentageOfOverallProducerCosts * scotlandTotal;
            }

            return 0;
        }

        public static decimal GetScotlandOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                var oneOffFeeSetupCostsWithoutBadDebtProvision = SchemeAdministratorSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);

                var paramsOtherCalculated = 1 + (value / 100);

                var total1Plus2ABadDebtForAllProducers = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

                var total1Plus2ABadDebtForProducersAndSubsidiaries = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producersAndSubsidiaries, materials, calcResult);

                var producerPercentageOfOverallProducerCosts = (total1Plus2ABadDebtForProducersAndSubsidiaries / total1Plus2ABadDebtForAllProducers) / 100;

                var scotlandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).ScotlandTotal;

                return oneOffFeeSetupCostsWithoutBadDebtProvision * paramsOtherCalculated * producerPercentageOfOverallProducerCosts * scotlandTotal;
            }

            return 0;
        }

        public static decimal GetNorthernIrelandTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
            Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                var oneOffFeeSetupCostsWithoutBadDebtProvision = SchemeAdministratorSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);

                var paramsOtherCalculated = 1 + (value / 100);

                var totalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);
                var totalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(materialCommsCostSummary);

                var total1Plus2ABadDebt = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

                var producerPercentageOfOverallProducerCosts = ((totalProducerDisposalFeeWithBadDebtProvision + totalProducerCommsFeeWithBadDebtProvision) / total1Plus2ABadDebt) / 100;

                var northernIrelandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).NorthernIrelandTotal;

                return oneOffFeeSetupCostsWithoutBadDebtProvision * paramsOtherCalculated * producerPercentageOfOverallProducerCosts * northernIrelandTotal;
            }

            return 0;
        }

        public static decimal GetNorthernIrelandOverallTotalWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                var oneOffFeeSetupCostsWithoutBadDebtProvision = SchemeAdministratorSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);

                var paramsOtherCalculated = 1 + (value / 100);

                var total1Plus2ABadDebtForAllProducers = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

                var total1Plus2ABadDebtForProducersAndSubsidiaries = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producersAndSubsidiaries, materials, calcResult);

                var producerPercentageOfOverallProducerCosts = (total1Plus2ABadDebtForProducersAndSubsidiaries / total1Plus2ABadDebtForAllProducers) / 100;

                var northernIrelandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).NorthernIrelandTotal;

                return oneOffFeeSetupCostsWithoutBadDebtProvision * paramsOtherCalculated * producerPercentageOfOverallProducerCosts * northernIrelandTotal;
            }

            return 0;
        }
    }
}
