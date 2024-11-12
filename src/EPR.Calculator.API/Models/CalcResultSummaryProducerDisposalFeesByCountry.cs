namespace EPR.Calculator.API.Models
{
    public class CalcResultSummaryProducerDisposalFeesByCountry
    {
        public decimal TotalProducerDisposalFee { get; set; }

        public decimal BadDebtProvision { get; set; }

        public decimal TotalProducerDisposalFeeWithBadDebtProvision { get; set; }

        public decimal EnglandTotal { get; set; }

        public decimal WalesTotal { get; set; }

        public decimal ScotlandTotal { get; set; }

        public decimal NorthernIrelandTotal { get; set; }

        public decimal TotalProducerCommsFee { get; set; }

        public decimal BadDebtProvisionForComms { get; set; }

        public decimal TotalProducerCommsFeeWithBadDebtProvision { get; set; }

        public decimal EnglandTotalForComms { get; set; }

        public decimal WalesTotalForComms { get; set; }

        public decimal ScotlandTotalForComms { get; set; }

        public decimal NorthernIrelandTotalForComms { get; set; }
    }
}
