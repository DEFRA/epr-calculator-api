namespace EPR.Calculator.API.Models
{
    public class CalcResultLapcapData
    {
        public string Name { get; set; }
        public required IEnumerable<CalcResultParameterCostDetail>? CalcResultLapcapDataDetails { get; set; }
    }
}
