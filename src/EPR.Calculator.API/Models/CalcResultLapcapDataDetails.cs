namespace EPR.Calculator.API.Models
{
    public class CalcResultLapcapDataDetails
    {
        public string Name { get; set; }

        public string EnglandDisposalCost { get; set; }
        public string WalesDisposalCost { get; set; }
        public string ScotlandDisposalCost { get; set; }
        public string NorthernIrelandDisposalCost { get; set; }
        public string TotalDisposalCost { get; set; }
        public decimal EnglandCost { get; set; }
        public decimal WalesCost { get; set; }
        public decimal ScotlandCost { get; set; }
        public decimal NorthernIrelandCost { get; set; }
        public decimal TotalCost { get; set; }
        public int OrderId { get; set; }
    }
}
