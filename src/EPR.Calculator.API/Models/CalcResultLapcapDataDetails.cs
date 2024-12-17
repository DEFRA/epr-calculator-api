namespace EPR.Calculator.API.Models
{
    public class CalcResultLapcapDataDetails
    {
        public required string Name { get; set; }

        public string EnglandDisposalCost { get; set; } = string.Empty;
        public string WalesDisposalCost { get; set; } = string.Empty;
        public string ScotlandDisposalCost { get; set; } = string.Empty;
        public string NorthernIrelandDisposalCost { get; set; } = string.Empty;
        public string TotalDisposalCost { get; set; } = string.Empty;
        public decimal EnglandCost { get; set; }
        public decimal WalesCost { get; set; }
        public decimal ScotlandCost { get; set; }
        public decimal NorthernIrelandCost { get; set; }
        public decimal TotalCost { get; set; }
        public int OrderId { get; set; }
    }
}
