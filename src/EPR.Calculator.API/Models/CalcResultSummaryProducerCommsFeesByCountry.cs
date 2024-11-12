namespace EPR.Calculator.API.Models
{
    public class CalcResultSummaryProducerCommsFeesByCountry
    {
        public string ProducerId { get; set; }
        
        public decimal TotalProducerCommsFee { get; set; }

        public decimal BadDebtProvision { get; set; }

        public decimal TotalProducerCommsFeeWithBadDebtProvision { get; set; }

        public decimal EnglandTotal { get; set; }

        public decimal WalesTotal { get; set; }

        public decimal ScotlandTotal { get; set; }

        public decimal NorthernIrelandTotal { get; set; }

    }
}
