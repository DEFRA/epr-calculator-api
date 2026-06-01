namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummaryBadDebtProvision
    {
        public decimal TotalProducerFeeWithoutBadDebtProvision { get; set; }

        public decimal BadDebtProvision { get; set; }

        public decimal TotalProducerFeeWithBadDebtProvision { get; set; }

        public decimal EnglandTotalWithBadDebtProvision { get; set; }

        public decimal WalesTotalWithBadDebtProvision { get; set; }

        public decimal ScotlandTotalWithBadDebtProvision { get; set; }

        public decimal NorthernIrelandTotalWithBadDebtProvision { get; set; }
    }
}
