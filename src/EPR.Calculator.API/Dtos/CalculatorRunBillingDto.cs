using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.Dtos
{
    public class CalculatorRunBillingDto
    {
        public int RunId { get; set; }

        public DateTime CreatedAt { get; set; }

        public required string RunName { get; set; }

        public required string FileExtension { get; set; }

        public string? UpdatedBy { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int RunClassificationId { get; set; }

        public required string RunClassificationStatus { get; set; }

        public string FinancialYear { get; set; }

        public int? BillingFileId { get; set; }

        public string? BillingCsvFileName { get; set; }

        public string? BillingJsonFileName { get; set; }

        public DateTime? BillingFileCreatedDate { get; set; }

        public string? BillingFileCreatedBy { get; set; }

        public DateTime? BillingFileAuthorisedDate { get; set; }

        public string? BillingFileAuthorisedBy { get; set; }
    }
}
