namespace EPR.Calculator.API.Models
{
    public class BillingFileGenerationMessage
    {
        public int CalculatorRunId { get; set; }

        public string ApprovedBy { get; set; } = null!;

        public string MessageType { get; set; } = null!;
    }
}
