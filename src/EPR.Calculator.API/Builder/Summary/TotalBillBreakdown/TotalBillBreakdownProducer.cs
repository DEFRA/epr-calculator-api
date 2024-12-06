using EPR.Calculator.API.Builder.Summary.Common;
using EPR.Calculator.API.Builder.Summary.CommsCostTwoBTotalBill;
using EPR.Calculator.API.Builder.Summary.LaDataPrepCosts;
using EPR.Calculator.API.Builder.Summary.SaSetupCosts;
using EPR.Calculator.API.Builder.Summary.ThreeSA;
using EPR.Calculator.API.Builder.Summary.TotalProducerBillBreakdown;
using EPR.Calculator.API.Builder.Summary.TwoCCommsCost;
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

        public static decimal GetTotalProducerBillWithoutBadDebtProvision(CalcResultSummaryProducerDisposalFees result)
        {
            var total = result.TotalProducerFeeforLADisposalCostswoBadDebtprovision +
                        result.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision +
                        result.TotalProducerFeeWithoutBadDebtFor2bComms +
                        result.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt +
                        
                        result.Total3SAOperatingCostwoBadDebtprovision +
                        result.LaDataPrepCostsTotalWithoutBadDebtProvisionSection4 +
                        result.TotalProducerFeeWithoutBadDebtProvisionSection5;

            return total;
        }
        public static decimal GetBadDebtProvisionforTotalProducerBill(CalcResultSummaryProducerDisposalFees result)
        {
            var total = result.BadDebtProvisionFor1 +
                        result.BadDebtProvisionFor2A +
                        result.BadDebtProvisionFor2bComms +
                        result.TwoCBadDebtProvision +
                        
                        result.BadDebtProvisionFor3 +
                        result.LaDataPrepCostsBadDebtProvisionSection4 +
                        result.BadDebtProvisionSection5;

            return total;
        }
        public static decimal GetTotalProducerBillWithBadDebtProvision(CalcResultSummaryProducerDisposalFees result)
        {
            var total = result.TotalProducerFeeforLADisposalCostswithBadDebtprovision +
                        result.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision +
                        result.TotalProducerFeeWithBadDebtFor2bComms +
                        result.TwoCTotalProducerFeeForCommsCostsWithBadDebt +
                        
                        result.Total3SAOperatingCostswithBadDebtprovision +
                        result.LaDataPrepCostsTotalWithBadDebtProvisionSection4 +
                        result.TotalProducerFeeWithBadDebtProvisionSection5;

            return total;
        }
        public static decimal GetEnglandTotalwithBadDebtprovision(CalcResultSummaryProducerDisposalFees result)
        {
            var total = result.EnglandTotalwithBadDebtprovision +
                        result.EnglandTotalwithBadDebtprovision2A +
                        result.EnglandTotalWithBadDebtFor2bComms +
                        result.TwoCEnglandTotalWithBadDebt +
                        
                        result.EnglandTotalwithBadDebtprovision3 +
                        result.LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4 +
                        result.EnglandTotalWithBadDebtProvisionSection5;

            return total;
        }

        public static decimal GetWalesTotalwithBadDebtprovision(CalcResultSummaryProducerDisposalFees result)
        {
            var total = result.WalesTotalwithBadDebtprovision +
                        result.WalesTotalwithBadDebtprovision2A +
                        result.WalesTotalWithBadDebtFor2bComms +
                        result.TwoCWalesTotalWithBadDebt +
                        
                        result.WalesTotalwithBadDebtprovision3 +
                        result.LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4 +
                        result.WalesTotalWithBadDebtProvisionSection5;

            return total;
        }

        public static decimal GetScotlandTotalwithBadDebtprovision(CalcResultSummaryProducerDisposalFees result)
        {
            var total = result.ScotlandTotalwithBadDebtprovision +
                        result.ScotlandTotalwithBadDebtprovision2A +
                        result.ScotlandTotalWithBadDebtFor2bComms +
                        result.TwoCScotlandTotalWithBadDebt +
                        
                        result.ScotlandTotalwithBadDebtprovision3 +
                        result.LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4 +
                        result.ScotlandTotalWithBadDebtProvisionSection5;

            return total;
        }
        public static decimal GetNorthernIrelandTotalwithBadDebtprovision(CalcResultSummaryProducerDisposalFees result)
        {
            var total = result.NorthernIrelandTotalwithBadDebtprovision +
                        result.NorthernIrelandTotalwithBadDebtprovision2A +
                        result.NorthernIrelandTotalWithBadDebtFor2bComms +
                        result.TwoCNorthernIrelandTotalWithBadDebt +
                        
                        result.NorthernIrelandTotalwithBadDebtprovision3 +
                        result.LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4 +
                        result.NorthernIrelandTotalWithBadDebtProvisionSection5;

            return total;
        }
        
        #region Total Row
        public static decimal GetTotalProducerBillWithoutBadDebtProvisionTotal(CalcResultSummaryProducerDisposalFees totalRow)
        {
            var total = totalRow.TotalProducerFeeforLADisposalCostswoBadDebtprovision +
                        totalRow.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision +
                        totalRow.TotalProducerFeeWithoutBadDebtFor2bComms +
                        totalRow.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt +

                        totalRow.Total3SAOperatingCostwoBadDebtprovision +
                        totalRow.LaDataPrepCostsTotalWithoutBadDebtProvisionSection4 +
                        totalRow.TotalProducerFeeWithoutBadDebtProvisionSection5;

            return total;
        }

        public static decimal GetBadDebtProvisionforTotalProducerBillTotal(CalcResultSummaryProducerDisposalFees totalRow)
        {
            var total = totalRow.BadDebtProvisionFor1 +
                        totalRow.BadDebtProvisionFor2A +
                        totalRow.BadDebtProvisionFor2bComms +
                        totalRow.TwoCBadDebtProvision +

                        totalRow.BadDebtProvisionFor3 +
                        totalRow.LaDataPrepCostsBadDebtProvisionSection4 +
                        totalRow.BadDebtProvisionSection5;

            return total;
        }
        public static decimal GetTotalProducerBillWithBadDebtProvisionTotal(CalcResultSummaryProducerDisposalFees totalRow)
        {
            var total = totalRow.TotalProducerFeeforLADisposalCostswithBadDebtprovision +
                        totalRow.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision +
                        totalRow.TotalProducerFeeWithBadDebtFor2bComms +
                        totalRow.TwoCTotalProducerFeeForCommsCostsWithBadDebt +

                        totalRow.Total3SAOperatingCostswithBadDebtprovision +
                        totalRow.LaDataPrepCostsTotalWithBadDebtProvisionSection4 +
                        totalRow.TotalProducerFeeWithBadDebtProvisionSection5;

            return total;
        }
        public static decimal GetEnglandTotalwithBadDebtprovisionTotal(CalcResultSummaryProducerDisposalFees totalRow)
        {
            var total = totalRow.EnglandTotalwithBadDebtprovision +
                        totalRow.EnglandTotalwithBadDebtprovision2A +
                        totalRow.EnglandTotalWithBadDebtFor2bComms +
                        totalRow.TwoCEnglandTotalWithBadDebt + 

                        totalRow.EnglandTotalwithBadDebtprovision3 +
                        totalRow.LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4 +
                        totalRow.EnglandTotalWithBadDebtProvisionSection5;

            return total;
        }
        public static decimal GetWalesTotalwithBadDebtprovisionTotal(CalcResultSummaryProducerDisposalFees totalRow)
        {
            var total = totalRow.WalesTotalwithBadDebtprovision +
                        totalRow.WalesTotalwithBadDebtprovision2A +
                        totalRow.WalesTotalWithBadDebtFor2bComms +
                        totalRow.TwoCWalesTotalWithBadDebt +

                        totalRow.WalesTotalwithBadDebtprovision3 +
                        totalRow.LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4 +
                        totalRow.WalesTotalWithBadDebtProvisionSection5;

            return total;
        }
        public static decimal GetScotlandTotalwithBadDebtprovisionTotal(CalcResultSummaryProducerDisposalFees totalRow)
        {
            var total = totalRow.ScotlandTotalwithBadDebtprovision +
                        totalRow.ScotlandTotalwithBadDebtprovision2A +
                        totalRow.ScotlandTotalWithBadDebtFor2bComms +
                        totalRow.TwoCScotlandTotalWithBadDebt+

                        totalRow.ScotlandTotalwithBadDebtprovision3 +
                        totalRow.LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4 +
                        totalRow.ScotlandTotalWithBadDebtProvisionSection5;

            return total;
        }
        public static decimal GetNorthernIrelandTotalwithBadDebtprovisionTotal(CalcResultSummaryProducerDisposalFees totalRow)
        {
            var total = totalRow.NorthernIrelandTotalwithBadDebtprovision +
                        totalRow.NorthernIrelandTotalwithBadDebtprovision2A +
                        totalRow.NorthernIrelandTotalWithBadDebtFor2bComms +
                        totalRow.TwoCNorthernIrelandTotalWithBadDebt +

                        totalRow.NorthernIrelandTotalwithBadDebtprovision3 +
                        totalRow.LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4 +
                        totalRow.NorthernIrelandTotalWithBadDebtProvisionSection5;

            return total;
        }
        #endregion Total Row
    }
}