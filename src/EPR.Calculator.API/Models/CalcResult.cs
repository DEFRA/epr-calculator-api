namespace EPR.Calculator.API.Models
{
    public class CalcResult
    {
        public CalcResultDetail CalcResultDetail { get; set; } = new();

        public required CalcResultLapcapData CalcResultLapcapData { get; set; } = new()
        {
            Name = string.Empty,
            CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>()
        };

        public CalcResultCommsCost CalcResultCommsCostReportDetail { get; set; } = new ();

        public required CalcResultLateReportingTonnage CalcResultLateReportingTonnageData { get; set; } =
            new()
            {
                Name = string.Empty,
                CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>(),
                MaterialHeading = string.Empty,
                TonnageHeading = string.Empty
            };

        public CalcResultParameterCommunicationCost CalcResultParameterCommunicationCost { get; set; }
            = new()
            {
                Name = string.Empty
            };

        public required CalcResultParameterOtherCost CalcResultParameterOtherCost { get; set; } =
            new()
            {
                BadDebtProvision = new KeyValuePair<string, string>(),
                Name = string.Empty,
                Details = new List<CalcResultParameterOtherCostDetail>(),
                Materiality = new List<CalcResultMateriality>(),
                SaOperatingCost = new List<CalcResultParameterOtherCostDetail>(),
                SchemeSetupCost = new CalcResultParameterOtherCostDetail()
            };

        public CalcResultOnePlusFourApportionment CalcResultOnePlusFourApportionment {  get; set; }
            = new()
            {
                Name = string.Empty
            };

        public CalcResultLaDisposalCostData CalcResultLaDisposalCostData { get; set; }
            = new()
            {
                Name = string.Empty,
                CalcResultLaDisposalCostDetails = [],
            };

        public CalcResultSummary CalcResultSummary { get; set; } = new();
    }
}
