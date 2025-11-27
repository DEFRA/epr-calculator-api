namespace EPR.Calculator.API.Data.DataModels
{
    public class OrganisationData
    {
        public int? OrganisationId { get; set; }

        public string? SubsidaryId { get; set; }

        public required string OrganisationName { get; set; }

        public string? TradingName { get; set; }

        public required string SubmissionPeriodDesc { get; set; }

        public required DateTime LoadTimestamp { get; set; }

        public string ObligationStatus { get; set; } = string.Empty;

        public Guid? SubmitterId { get; set; }

        public int StatusCode { get; set; }

        public double PartialObligationPercentage { get; set; }

        public string ErrorCodeDesc { get; set; } = string.Empty;
    }
}