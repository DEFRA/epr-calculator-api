namespace EPR.Calculator.API.Models
{
    public class CalcResultLateReportingTonnage
    {
        public required IEnumerable<CalcResultLateReportingTonnageDetail>? CalcResultLateReportingTonnageDetails { get; set; } = new List<CalcResultLateReportingTonnageDetail>();
    }
}
