namespace EPR.Calculator.API.Data.DataModels
{
    public class CalculatorRunCsvFileMetadata
    {
        public int Id { get; set; }

        public required string FileName { get; set; }

        public required string BlobUri { get; set; }

        public required int CalculatorRunId { get; set; }

        public virtual CalculatorRun? CalculatorRun { get; set; }
    }
}