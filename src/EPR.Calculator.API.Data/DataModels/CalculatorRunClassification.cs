namespace EPR.Calculator.API.Data.DataModels
{
    public class CalculatorRunClassification
    {
        public int Id { get; set; }

        public required string Status { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public ICollection<CalculatorRun> CalculatorRunDetails { get; } = new List<CalculatorRun>();
    }
}
