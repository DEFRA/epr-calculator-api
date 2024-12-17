namespace EPR.Calculator.API.Models
{
    public class CalcResult
    {
        public CalcResultDetail? CalcResultDetail { get; set; }

        public required CalcResultLapcapData CalcResultLapcapData { get; set; }

        public CalcResultCommsCost CalcResultCommsCostReportDetail { get; set; }

        public required CalcResultLateReportingTonnage CalcResultLateReportingTonnageData { get; set; }

        public CalcResultParameterCommunicationCost? CalcResultParameterCommunicationCost { get; set; }

        public required CalcResultParameterOtherCost CalcResultParameterOtherCost { get; set; }

        public CalcResultOnePlusFourApportionment CalcResultOnePlusFourApportionment {  get; set; }

        public CalcResultLaDisposalCostData CalcResultLaDisposalCostData { get; set; }

        public CalcResultSummary CalcResultSummary { get; set; }
    }
}
