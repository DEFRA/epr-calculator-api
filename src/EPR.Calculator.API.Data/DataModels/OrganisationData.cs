namespace EPR.Calculator.API.Data.DataModels
{
    public class OrganisationData
    {
        public int OrganisationId { get; set; }

        public string? SubsidiaryId { get; set; }

        public required string OrganisationName { get; set; }

        public string? TradingName { get; set; }

        public required DateTime LoadTimestamp { get; set; }

        public string ObligationStatus { get; set; } = string.Empty;

        public Guid? SubmitterId { get; set; }

        public string? StatusCode { get; set; }

        public int? DaysObligated { get; set; }

        public string? ErrorCode { get; set; } = string.Empty;

        public string? JoinerDate { get; set; }

        public string? LeaverDate { get; set; }
    }
}