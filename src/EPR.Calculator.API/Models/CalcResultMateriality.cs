namespace EPR.Calculator.API.Models
{
    public class CalcResultMateriality
    {
        public required string SevenMateriality { get; set; }
        public string Amount { get; set; } = string.Empty;
        public string Percentage { get; set; } = string.Empty;
        public decimal AmountValue { get; set; }
        public decimal PercentageValue { get; set; }
    }
}