using EPR.Calculator.API.Builder.Summary.TotalProducerBillBreakdown;
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

        public static decimal GetBadDebtProvisionForTotalProducerBill(CalcResultSummaryProducerDisposalFees result)
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

        public static decimal GetEnglandTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees result)
        {
            var total = result.EnglandTotalWithBadDebtProvision +
                        result.EnglandTotalWithBadDebtProvision2A +
                        result.EnglandTotalWithBadDebtFor2bComms +
                        result.TwoCEnglandTotalWithBadDebt +
                        
                        result.EnglandTotalWithBadDebtProvision3 +
                        result.LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4 +
                        result.EnglandTotalWithBadDebtProvisionSection5;

            return total;
        }

        public static decimal GetWalesTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees result)
        {
            var total = result.WalesTotalWithBadDebtProvision +
                        result.WalesTotalWithBadDebtProvision2A +
                        result.WalesTotalWithBadDebtFor2bComms +
                        result.TwoCWalesTotalWithBadDebt +
                        
                        result.WalesTotalWithBadDebtProvision3 +
                        result.LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4 +
                        result.WalesTotalWithBadDebtProvisionSection5;

            return total;
        }

        public static decimal GetScotlandTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees result)
        {
            var total = result.ScotlandTotalWithBadDebtProvision +
                        result.ScotlandTotalWithBadDebtProvision2A +
                        result.ScotlandTotalWithBadDebtFor2bComms +
                        result.TwoCScotlandTotalWithBadDebt +
                        
                        result.ScotlandTotalWithBadDebtProvision3 +
                        result.LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4 +
                        result.ScotlandTotalWithBadDebtProvisionSection5;

            return total;
        }

        public static decimal GetNorthernIrelandTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees result)
        {
            var total = result.NorthernIrelandTotalWithBadDebtProvision +
                        result.NorthernIrelandTotalWithBadDebtProvision2A +
                        result.NorthernIrelandTotalWithBadDebtFor2bComms +
                        result.TwoCNorthernIrelandTotalWithBadDebt +
                        
                        result.NorthernIrelandTotalWithBadDebtProvision3 +
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

        public static decimal GetBadDebtProvisionForTotalProducerBillTotal(CalcResultSummaryProducerDisposalFees totalRow)
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

        public static decimal GetEnglandTotalWithBadDebtProvisionTotal(CalcResultSummaryProducerDisposalFees totalRow)
        {
            var total = totalRow.EnglandTotalWithBadDebtProvision +
                        totalRow.EnglandTotalWithBadDebtProvision2A +
                        totalRow.EnglandTotalWithBadDebtFor2bComms +
                        totalRow.TwoCEnglandTotalWithBadDebt + 

                        totalRow.EnglandTotalWithBadDebtProvision3 +
                        totalRow.LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4 +
                        totalRow.EnglandTotalWithBadDebtProvisionSection5;

            return total;
        }

        public static decimal GetWalesTotalWithBadDebtProvisionTotal(CalcResultSummaryProducerDisposalFees totalRow)
        {
            var total = totalRow.WalesTotalWithBadDebtProvision +
                        totalRow.WalesTotalWithBadDebtProvision2A +
                        totalRow.WalesTotalWithBadDebtFor2bComms +
                        totalRow.TwoCWalesTotalWithBadDebt +

                        totalRow.WalesTotalWithBadDebtProvision3 +
                        totalRow.LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4 +
                        totalRow.WalesTotalWithBadDebtProvisionSection5;

            return total;
        }

        public static decimal GetScotlandTotalWithBadDebtProvisionTotal(CalcResultSummaryProducerDisposalFees totalRow)
        {
            var total = totalRow.ScotlandTotalWithBadDebtProvision +
                        totalRow.ScotlandTotalWithBadDebtProvision2A +
                        totalRow.ScotlandTotalWithBadDebtFor2bComms +
                        totalRow.TwoCScotlandTotalWithBadDebt+

                        totalRow.ScotlandTotalWithBadDebtProvision3 +
                        totalRow.LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4 +
                        totalRow.ScotlandTotalWithBadDebtProvisionSection5;

            return total;
        }
        
        public static decimal GetNorthernIrelandTotalWithBadDebtProvisionTotal(CalcResultSummaryProducerDisposalFees totalRow)
        {
            var total = totalRow.NorthernIrelandTotalWithBadDebtProvision +
                        totalRow.NorthernIrelandTotalWithBadDebtProvision2A +
                        totalRow.NorthernIrelandTotalWithBadDebtFor2bComms +
                        totalRow.TwoCNorthernIrelandTotalWithBadDebt +

                        totalRow.NorthernIrelandTotalWithBadDebtProvision3 +
                        totalRow.LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4 +
                        totalRow.NorthernIrelandTotalWithBadDebtProvisionSection5;

            return total;
        }
        #endregion Total Row
    }
}