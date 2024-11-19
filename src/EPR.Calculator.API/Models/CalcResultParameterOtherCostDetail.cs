namespace EPR.Calculator.API.Models
{
    public class CalcResultParameterOtherCostDetail
    {
        public string Name { get; set; }
        public string England { get; set; }
        public string Wales { get; set; }
        public string Scotland { get; set; }
        public string NorthernIreland { get; set; }
        public string Total { get; set; }
        public decimal EnglandValue { get; set; }
        public decimal WalesValue { get; set; }
        public decimal ScotlandValue { get; set; }
        public decimal NorthernIrelandValue { get; set; }
        public decimal TotalValue { get; set; }
        public int OrderId { get; set; }
    }
}
