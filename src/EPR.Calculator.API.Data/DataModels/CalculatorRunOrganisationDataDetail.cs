namespace EPR.Calculator.API.Data.DataModels
{
    public class CalculatorRunOrganisationDataDetail
    {
        public int Id { get; set; }

        public int? OrganisationId { get; set; }

        public string? SubsidaryId { get; set; }

        public required string OrganisationName { get; set; }

        public string? TradingName { get; set; }

        public required string? SubmissionPeriodDesc { get; set; }

        public DateTime LoadTimeStamp { get; set; }

        public int CalculatorRunOrganisationDataMasterId { get; set; }

        public bool IsValid { get; set; } = true;

        public string? LeaverCode { get; set; }

        public string? LeaverDate { get; set; }

        public string? JoinerDate { get; set; }

        public string? SubmitterOrgId { get; set; }

        public virtual CalculatorRunOrganisationDataMaster? CalculatorRunOrganisationDataMaster { get; set; }
    }
}
