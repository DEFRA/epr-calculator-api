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
    }
}
