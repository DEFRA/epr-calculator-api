namespace EPR.Calculator.API.Models;

public record BillingFileGenerationMessage
{
    public string MessageType { get; } = "Billing";
    public required int CalculatorRunId { get; init; }
    public required string ApprovedBy { get; init; }
}
