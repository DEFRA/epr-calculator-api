namespace EPR.Calculator.API.Data.DataModels
{
    public class CalculatorRunOrganisationDataDetail
    {
        public int Id { get; set; }

        public int? OrganisationId { get; set; }

        public string? SubsidiaryId { get; set; }

        public required string OrganisationName { get; set; }

        public string? TradingName { get; set; }

        public DateTime LoadTimeStamp { get; set; }

        public int CalculatorRunOrganisationDataMasterId { get; set; }

        public virtual CalculatorRunOrganisationDataMaster? CalculatorRunOrganisationDataMaster { get; set; }

        public string ObligationStatus { get; set; } = string.Empty;

        public Guid? SubmitterId { get; set; }

        public string? StatusCode { get; set; }

        public int? DaysObligated { get; set; }

        public string? ErrorCode { get; set; } = string.Empty;
    }
}
