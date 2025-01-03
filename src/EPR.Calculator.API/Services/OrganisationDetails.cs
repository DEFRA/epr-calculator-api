namespace EPR.Calculator.API.Services
{
    public partial class TransposePomAndOrgDataService
    {
        public class OrganisationDetails
        {
            public int? OrganisationId { get; set; }
            public required string OrganisationName { get; set; }
            public string? SubmissionPeriod { get; set; }
            public string? SubmissionPeriodDescription { get; set; }
            public string? SubsidaryId { get; set; }
        }
        public class SubmissionDetails
        {
            public string? SubmissionPeriod { get; set; }
            public string? SubmissionPeriodDesc { get; set; }
        }
    }
}
