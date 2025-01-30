namespace EPR.Calculator.API.Models
{
    public class CalcResultSummaryProducerDisposalFeesByMaterial
    {
        public decimal HouseholdPackagingWasteTonnage { get; set; }

        public decimal PublicBinTonnage { get; set; }

        public decimal HouseholdDrinksContainersTonnage { get; set; }

        public decimal TotalReportedTonnage { get; set; }

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
    }
}
