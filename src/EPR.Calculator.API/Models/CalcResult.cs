namespace EPR.Calculator.API.Models
{
    public class CalcResult
    {
        public required CalcResultDetail CalcResultDetail { get; set; }

        public required CalcResultLapcapData CalcResultLapcapData { get; set; }

        public required CalcResultLateReportingTonnage CalcResultLateReportingTonnageDetail { get; set; }

        public required CalcResultParameterCommunicationCost CalcResultParameterCommunicationCost { get; set; }
    }
}
