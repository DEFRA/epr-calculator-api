namespace EPR.Calculator.API.Models
{
    public class CalcResultLateReportingTonnage
    {
        public string Name { get; set; }
        public required IEnumerable<CalcResultLateReportingTonnageDetail>? CalcResultLateReportingTonnageDetails { get; set; } = new List<CalcResultLateReportingTonnageDetail>();
    }
}
