namespace EPR.Calculator.API.Models
{
    public record BillingFileGenerationMessage: MessageBase
    {
        public string ApprovedBy { get; set; } = null!;
    }
}



