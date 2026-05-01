namespace EPR.Calculator.API.Data.DataModels
{
    public class CalculatorRunBillingFileMetadata
    {
        public int Id { get; set; }
        public int CalculatorRunId { get; set; }
        public CalculatorRun CalculatorRun { get; set; } = null!;
        public required string BillingCsvFileName { get; set; }
        public required string BillingJsonFileName { get; set; }
        public required DateTime BillingFileCreatedDate { get; set; }
        public required string BillingFileCreatedBy { get; set; }
        public DateTime? BillingFileAuthorisedDate { get; set; }
        public string? BillingFileAuthorisedBy { get; set; }
    }
}
