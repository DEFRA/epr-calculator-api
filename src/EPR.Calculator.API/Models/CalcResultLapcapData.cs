namespace EPR.Calculator.API.Models
{
    public class CalcResultLapcapData
    {
        public string Name { get; set; } = string.Empty;
        public required IEnumerable<CalcResultLapcapDataDetails>? CalcResultLapcapDataDetails { get; set; }
    }
}
