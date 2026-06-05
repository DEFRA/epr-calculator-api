namespace EPR.Calculator.Service.Function.Models
{
    public abstract class CalcResultSummaryProducerMaterialBase
    {
        public decimal HouseholdPackagingWasteTonnage { get; set; }
        public decimal PublicBinTonnage { get; set; }
        public decimal HouseholdDrinksContainersTonnage { get; set; }
        public decimal TotalReportedTonnage { get; set; }
        public decimal BadDebtProvision { get; set; }
        public decimal EnglandWithBadDebtProvision { get; set; }
        public decimal WalesWithBadDebtProvision { get; set; }
        public decimal ScotlandWithBadDebtProvision { get; set; }
        public decimal NorthernIrelandWithBadDebtProvision { get; set; }
    }
}