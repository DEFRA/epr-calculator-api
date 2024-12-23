namespace EPR.Calculator.API.Models
{
    public class CalcResultParameterCommunicationCostDetail1
    {
        public required string Name { get; set; }
        public required string England { get; set; }
        public required string Wales { get; set; }
        public required string Scotland { get; set; }
        public required string NorthernIreland { get; set; }
        public required string Total { get; set; }
        public int OrderId { get; set; }
    }
}
