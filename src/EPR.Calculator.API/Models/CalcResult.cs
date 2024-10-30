namespace EPR.Calculator.API.Models
{
    public class CalcResult
    {
        public CalcResultDetail CalcResultDetail { get; set; }

        public CalcResultLapcapData CalcResultLapcapData { get; set; }

        public CalcResultLateReportingTonnage CalcResultLateReportingTonnageDetail { get; set; }

        public CalcResultParameterCommunicationCost CalcResultParameterCommunicationCost { get; set; }

        public CalcResultParameterOtherCost CalcResultParameterOtherCost { get; set; }
    }
}
