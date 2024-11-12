using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.Models
{
    public class CalcResultSummaryTwoACommsCostByMaterial
    {
        public decimal HouseholdPackagingWasteTonnage { get; set; }

        public decimal PriceperTonne { get; set; }

        public decimal ProducerTotalCostWithoutBadDebtProvision { get; set; }

        public decimal BadDebtProvision { get; set; }

        public decimal ProducerTotalCostwithBadDebtProvision { get; set; }

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
