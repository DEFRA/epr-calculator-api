using EPR.Calculator.API.CommsCost;

namespace EPR.Calculator.API.Models
{
    public class CalcResult
    {
        public CalcResultDetail CalcResultDetail { get; set; }

        public CalcResultLapcapData CalcResultLapcapData { get; set; }

        public CalcResultCommsCost CalcResultLateReportingTonnageDetail { get; set; }

        public CalcResultParameterCommunicationCost CalcResultParameterCommunicationCost { get; set; }

        public CalcResultParameterOtherCost CalcResultParameterOtherCost { get; set; }

        public CalcResultOnePlusFourApportionment CalcResultOnePlusFourApportionment {  get; set; }

        public CalcResultLaDisposalCostData CalcResultLaDisposalCostData { get; set; }
    }
}
