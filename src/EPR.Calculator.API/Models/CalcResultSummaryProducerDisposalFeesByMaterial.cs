namespace EPR.Calculator.API.Models
{
    public class CalcResultSummaryProducerDisposalFeesByMaterial
    {
        public decimal HouseholdPackagingWasteTonnage { get; set; }

        public decimal ManagedConsumerWasteTonnage { get; set; }

        public decimal NetReportedTonnage { get; set; }

        public decimal PricePerTonne { get; set; }

        public decimal ProducerDisposalFee { get; set; }

        public decimal BadDebtProvision { get; set; }

        public decimal ProducerDisposalFeeWithBadDebtProvision { get; set; }

        public decimal EnglandWithBadDebtProvision { get; set; }

        public decimal WalesWithBadDebtProvision { get; set; }

        public decimal ScotlandWithBadDebtProvision { get; set; }

        public decimal NorthernIrelandWithBadDebtProvision { get; set; }

        public decimal TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision { get; set; }

        public decimal TotalBadDebtProvision { get; set; }

        public decimal TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision { get; set; }

        public decimal EnglandTotalwithBadDebtprovision { get; set; }

        public decimal WalesTotalwithBadDebtprovision { get; set; }

        public decimal ScotlandTotalwithBadDebtprovision { get; set; }

        public decimal NorthernIrelandTotalwithBadDebtprovision { get; set; }
    }
}
