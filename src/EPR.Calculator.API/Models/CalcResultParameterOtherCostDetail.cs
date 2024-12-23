namespace EPR.Calculator.API.Models
{
    public class CalcResultParameterOtherCostDetail
    {
        public string? Name { get; set; }
        public string England { get; set; } = string.Empty;
        public string Wales { get; set; } = string.Empty;
        public string Scotland { get; set; } = string.Empty;
        public string NorthernIreland { get; set; } = string.Empty;
        public string Total { get; set; } = string.Empty;
        public decimal EnglandValue { get; set; }
        public decimal WalesValue { get; set; }
        public decimal ScotlandValue { get; set; }
        public decimal NorthernIrelandValue { get; set; }
        public decimal TotalValue { get; set; }
        public int OrderId { get; set; }
    }
}
