using EPR.Calculator.API.Builder.Summary.TotalProducerBillBreakdown;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.TotalBillBreakdown
{
    public static class TotalBillBreakdownProducer
    {
        public static readonly int ColumnIndex = 265;
    
        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.TotalProducerBillWithoutBadDebtProvision, ColumnIndex = ColumnIndex },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.BadDebtProvision, ColumnIndex = ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.TotalProducerBillWithBadDebtProvision, ColumnIndex = ColumnIndex + 2 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.EnglandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 3 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.WalesTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 4 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.ScotlandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 5 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.NorthernIrelandTotalWithBadDebtProvision, ColumnIndex = ColumnIndex + 6 }
            ];
        }

        public static IEnumerable<CalcResultSummaryHeader> GetSummaryHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.TotalProducerBillBreakdown, ColumnIndex = ColumnIndex }
            ];
        }

        public static void SetValues(CalcResultSummary result)
        {
            foreach (var fee in result.ProducerDisposalFees)
            {
                fee.TotalProducerBillWithoutBadDebtProvision = GetTotalProducerBillWithoutBadDebtProvision(fee);
                fee.BadDebtProvisionForTotalProducerBill = GetBadDebtProvisionForTotalProducerBill(fee);
                fee.TotalProducerBillWithBadDebtProvision = GetTotalProducerBillWithBadDebtProvision(fee);
                fee.EnglandTotalWithBadDebtProvisionTotalBill = GetEnglandTotalWithBadDebtProvision(fee);
                fee.WalesTotalWithBadDebtProvisionTotalBill = GetWalesTotalWithBadDebtProvision(fee);
                fee.ScotlandTotalWithBadDebtProvisionTotalBill = GetScotlandTotalWithBadDebtProvision(fee);
                fee.NorthernIrelandTotalWithBadDebtProvisionTotalBill = GetNorthernIrelandTotalWithBadDebtProvision(fee);
            }
        }

        private static decimal GetTotalProducerBillWithoutBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.TotalProducerFeeforLADisposalCostswoBadDebtprovision +
                   fee.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision + 
                   fee.TotalProducerFeeWithoutBadDebtFor2bComms +
                   fee.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt +
                   fee.Total3SAOperatingCostwoBadDebtprovision +
                   fee.LaDataPrepCostsTotalWithoutBadDebtProvisionSection4 +
                   fee.TotalProducerFeeWithoutBadDebtProvisionSection5;
        }

        private static decimal GetBadDebtProvisionForTotalProducerBill(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.BadDebtProvisionFor1 + 
                   fee.BadDebtProvisionFor2A +
                   fee.BadDebtProvisionFor2bComms +
                   fee.TwoCBadDebtProvision +
                   fee.BadDebtProvisionFor3 +
                   fee.LaDataPrepCostsBadDebtProvisionSection4 +
                   fee.BadDebtProvisionSection5;
        }

        private static decimal GetTotalProducerBillWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.TotalProducerFeeforLADisposalCostswithBadDebtprovision +
                   fee.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision +
                   fee.TotalProducerFeeWithBadDebtFor2bComms +
                   fee.TwoCTotalProducerFeeForCommsCostsWithBadDebt +
                   fee.Total3SAOperatingCostswithBadDebtprovision +
                   fee.LaDataPrepCostsTotalWithBadDebtProvisionSection4 +
                   fee.TotalProducerFeeWithBadDebtProvisionSection5;
        }

        private static decimal GetEnglandTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.EnglandTotalWithBadDebtProvision +
                   fee.EnglandTotalWithBadDebtProvision2A +
                   fee.EnglandTotalWithBadDebtFor2bComms +
                   fee.TwoCEnglandTotalWithBadDebt +
                   fee.EnglandTotalWithBadDebtProvision3 +
                   fee.LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4 +
                   fee.EnglandTotalWithBadDebtProvisionSection5;
        }

        private static decimal GetWalesTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.WalesTotalWithBadDebtProvision +
                   fee.WalesTotalWithBadDebtProvision2A +
                   fee.WalesTotalWithBadDebtFor2bComms +
                   fee.TwoCWalesTotalWithBadDebt +
                   fee.WalesTotalWithBadDebtProvision3 +
                   fee.LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4 +
                   fee.WalesTotalWithBadDebtProvisionSection5;
        }

        private static decimal GetScotlandTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.ScotlandTotalWithBadDebtProvision +
                   fee.ScotlandTotalWithBadDebtProvision2A +
                   fee.ScotlandTotalWithBadDebtFor2bComms +
                   fee.TwoCScotlandTotalWithBadDebt +
                   fee.ScotlandTotalWithBadDebtProvision3 +
                   fee.LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4 +
                   fee.ScotlandTotalWithBadDebtProvisionSection5;
        }

        private static decimal GetNorthernIrelandTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.NorthernIrelandTotalWithBadDebtProvision +
                   fee.NorthernIrelandTotalWithBadDebtProvision2A +
                   fee.NorthernIrelandTotalWithBadDebtFor2bComms +
                   fee.TwoCNorthernIrelandTotalWithBadDebt +
                   fee.NorthernIrelandTotalWithBadDebtProvision3 +
                   fee.LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4 +
                   fee.NorthernIrelandTotalWithBadDebtProvisionSection5;
        }
    }
}