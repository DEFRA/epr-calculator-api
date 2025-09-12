namespace EPR.Calculator.API.Dtos
{
    public class CalculatorRunDto
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

        public bool? IsBillingFileGenerating { get; set; }

        public bool IsBillingFileGeneratedLatest { get; set; }
    }
}
