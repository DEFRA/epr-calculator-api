namespace EPR.Calculator.API.Data.DataModels
{
    public class CalculatorRunBillingFileMetadata
    {
        public int Id { get; set; }

        public required string BillingCsvFileName { get; set; }

        public required string BillingJsonFileName { get; set; }

        public required DateTime BillingFileCreatedDate { get; set; } = DateTime.Now;

        public required string BillingFileCreatedBy { get; set; }

        public DateTime? BillingFileAuthorisedDate { get; set; }

        public string? BillingFileAuthorisedBy { get; set; }

        public required int CalculatorRunId { get; set; }

        public virtual CalculatorRun? CalculatorRun { get; set; }
    }
}
