namespace EPR.Calculator.API.Models
{
    public class BillingFileGenerationMessage
    {
        public int CalculatorRunId { get; set; }

        public string ApprovedBy { get; set; }

        public string MessageType { get; set; }
    }
}
