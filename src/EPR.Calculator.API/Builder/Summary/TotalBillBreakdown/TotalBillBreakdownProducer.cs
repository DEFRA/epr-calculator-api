using EPR.Calculator.API.Builder.Summary.Common;
using EPR.Calculator.API.Builder.Summary.CommsCostTwoBTotalBill;
using EPR.Calculator.API.Builder.Summary.LaDataPrepCosts;
using EPR.Calculator.API.Builder.Summary.SaSetupCosts;
using EPR.Calculator.API.Builder.Summary.ThreeSA;
using EPR.Calculator.API.Builder.Summary.TotalProducerBillBreakdown;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.TotalBillBreakdown
{
    public static class TotalBillBreakdownProducer
    {
        public static readonly int ColumnIndex = 231;
    
        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.TotalProducerBillwoBadDebtProvision, ColumnIndex = ColumnIndex },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.BadDebtProvisionforTotalProducerBill, ColumnIndex = ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.TotalProducerBillwithBadDebtProvision, ColumnIndex = ColumnIndex + 2 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.EnglandTotalWithBadDebtProvisionSectionTB, ColumnIndex = ColumnIndex + 3 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.WalesTotalWithBadDebtProvisionSectionTB, ColumnIndex = ColumnIndex + 4 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.ScotlandTotalWithBadDebtProvisionSectionTB, ColumnIndex = ColumnIndex + 5 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.NorthernIrelandTotalWithBadDebtProvisionSectionTB, ColumnIndex = ColumnIndex + 6 }
            ];
        }
        
        public static decimal GetTotalProducerBillWithoutBadDebtProvision(IEnumerable<ProducerDetail> producers,
           //IEnumerable<ProducerDetail> producersAndSubsidiaries,
           IEnumerable<MaterialDetail> materials,
           ProducerDetail producer,
           CalcResult calcResult,
           IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults,
           Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
           Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            var total1 = CalcResultSummaryUtil.GetTotalProducerDisposalFee(materialCostSummary);
            var total2a = CalcResultSummaryUtil.GetTotalProducerCommsFee(commsCostSummary);
            var total2b = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, allResults);
            
            var total3 = ThreeSaUtil.GetSAOperatingCostsTotalWithoutBadDebtProvisionPrtoducerTotalSection3(materialCostSummary, commsCostSummary, (List<MaterialDetail>)materials, calcResult);
            //var total2b = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2bTotalsRow(calcResult, producersAndSubsidiaries, allResults);
            //var total4 = LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotal(producers, producersAndSubsidiaries, materials, calcResult);
            //var total5 = SaSetupCostsProducer.GetProducerOneOffFeeWithoutBadDebtProvisionTotal(producers, producersAndSubsidiaries, materials, calcResult);
            var total4 = LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvision(producers, materials, calcResult, materialCostSummary, commsCostSummary);
            var total5 = SaSetupCostsProducer.GetProducerOneOffFeeWithoutBadDebtProvision(producers, materials, calcResult, materialCostSummary, commsCostSummary);
                
            return total1 + total2a + total2b + total3 + total4 + total5;
        }

        public static decimal GetBadDebtProvisionforTotalProducerBill(IEnumerable<ProducerDetail> producers,
           IEnumerable<MaterialDetail> materials,
           CalcResult calcResult,
           Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
           Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            /*var badDebtTotal1 = CalcResultSummaryUtil.GetTotalBadDebtProvision(materialCostSummary);
            var badDebtTotal2a = CalcResultSummaryUtil.GetTotalBadDebtProvision(commsCostSummary);
            var badDebtTotal2b = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2b(calcResult, producer, runProducerMaterialDetails),
             */  
                
            var total1 = CalcResultSummaryUtil.GetTotalProducerDisposalFee(materialCostSummary);
            var total2a = CalcResultSummaryUtil.GetTotalProducerCommsFee(commsCostSummary);
            //var total2b = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, allResults);
            var total2b = 0;
            var total3 = ThreeSaUtil.GetSAOperatingCostsTotalWithoutBadDebtProvisionPrtoducerTotalSection3(materialCostSummary, commsCostSummary, (List<MaterialDetail>)materials, calcResult);
            //var total2b = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2bTotalsRow(calcResult, producersAndSubsidiaries, allResults);
            //var total4 = LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotal(producers, producersAndSubsidiaries, materials, calcResult);
            //var total5 = SaSetupCostsProducer.GetProducerOneOffFeeWithoutBadDebtProvisionTotal(producers, producersAndSubsidiaries, materials, calcResult);
            var total4 = LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvision(producers, materials, calcResult, materialCostSummary, commsCostSummary);
            var total5 = SaSetupCostsProducer.GetProducerOneOffFeeWithoutBadDebtProvision(producers, materials, calcResult, materialCostSummary, commsCostSummary);

            return total1 + total2a + total2b + total3 + total4 + total5;
        }

        public static decimal GetTotalProducerBillWithBadDebtProvision(IEnumerable<ProducerDetail> producers,
           IEnumerable<MaterialDetail> materials,
           CalcResult calcResult,
           Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
           Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return 301.01M;
        }

        public static decimal GetEnglandTotalwithBadDebtprovision(IEnumerable<ProducerDetail> producers,
           IEnumerable<MaterialDetail> materials,
           CalcResult calcResult,
           Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
           Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return 401.01M;
        }

        public static decimal GetWalesTotalwithBadDebtprovision(IEnumerable<ProducerDetail> producers,
           IEnumerable<MaterialDetail> materials,
           CalcResult calcResult,
           Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
           Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return 501.01M;
        }

        public static decimal GetScotlandTotalwithBadDebtprovision(IEnumerable<ProducerDetail> producers,
           IEnumerable<MaterialDetail> materials,
           CalcResult calcResult,
           Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
           Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return 601.01M;
        }

        public static decimal GetNorthernIrelandTotalwithBadDebtprovision(IEnumerable<ProducerDetail> producers,
           IEnumerable<MaterialDetail> materials,
           CalcResult calcResult,
           Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary,
           Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCommsCostSummary)
        {
            return 701.01M;
        }

        public static decimal GetBadDebtProvisionforTotalProducerBillTotal(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            return 10000.01M;
        }

        public static decimal GetTotalProducerBillWithoutBadDebtProvisionTotal(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            var oneOffFeeSetupCostsWithoutBadDebtProvision = SaSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);

            //var totalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);
            //var totalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(materialCommsCostSummary);

            var total1Plus2ABadDebt = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult);

            //var producerPercentageOfOverallProducerCosts = (totalProducerDisposalFeeWithBadDebtProvision + totalProducerCommsFeeWithBadDebtProvision) / total1Plus2ABadDebt;

            return total1Plus2ABadDebt;
        }

        public static decimal GetTotalProducerBillWithBadDebtProvisionTotal(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            return 30000.01M;
        }

        public static decimal GetEnglandTotalwithBadDebtprovisionTotal(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            return 40000.01M;
        }

        public static decimal GetWalesTotalwithBadDebtprovisionTotal(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            return 50000.01M;
        }
        
        public static decimal GetScotlandTotalwithBadDebtprovisionTotal(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            return 60000.01M;
        }

        public static decimal GetNorthernIrelandTotalwithBadDebtprovisionTotal(IEnumerable<ProducerDetail> producers,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            return 70000.01M;
        }
    }
}