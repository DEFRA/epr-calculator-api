namespace EPR.Calculator.API.Models
{
    public class CalcResultLateReportingTonnage
    {
        public string Name { get; set; } = string.Empty;
        public required IEnumerable<CalcResultLateReportingTonnageDetail>? CalcResultLateReportingTonnageDetails { get; set; }
    }
}