namespace EPR.Calculator.API.Models
{
    public class CalcResult
    {
        public CalcResultDetail CalcResultDetail { get; set; }
            = new CalcResultDetail();

        public required CalcResultLapcapData CalcResultLapcapData { get; set; }

        public CalcResultCommsCost CalcResultCommsCostReportDetail { get; set; }
            = new CalcResultCommsCost();

        public required CalcResultLateReportingTonnage CalcResultLateReportingTonnageData { get; set; }

        public CalcResultParameterCommunicationCost CalcResultParameterCommunicationCost { get; set; }
            = new CalcResultParameterCommunicationCost
            {
                Name = string.Empty
            };

        public required CalcResultParameterOtherCost CalcResultParameterOtherCost { get; set; }

        public CalcResultOnePlusFourApportionment CalcResultOnePlusFourApportionment {  get; set; }
            = new CalcResultOnePlusFourApportionment();

        public CalcResultLaDisposalCostData CalcResultLaDisposalCostData { get; set; }
            = new CalcResultLaDisposalCostData
            {
                Name = string.Empty,
                CalcResultLaDisposalCostDetails = [],
            };

        public CalcResultSummary CalcResultSummary { get; set; }
            = new CalcResultSummary();
    }
}
