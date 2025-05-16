using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.Dtos
{
    public record CalculatorRunBillingDto
    {
        public int RunId { get; init; }

        public DateTime CreatedAt { get; init; }

        public required string RunName { get; init; }

        public required string FileExtension { get; init; }

        public string? UpdatedBy { get; init; }

        public string CreatedBy { get; init; }

        public DateTime? UpdatedAt { get; init; }

        public int RunClassificationId { get; init; }

        public required string RunClassificationStatus { get; init; }

        public string FinancialYear { get; init; }

        public int? BillingFileId { get; init; }

        public string? BillingCsvFileName { get; init; }

        public string? BillingJsonFileName { get; init; }

        public DateTime? BillingFileCreatedDate { get; init; }

        public string? BillingFileCreatedBy { get; init; }

        public DateTime? BillingFileAuthorisedDate { get; init; }

        public string? BillingFileAuthorisedBy { get; init; }
    }
}
