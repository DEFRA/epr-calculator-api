namespace EPR.Calculator.API.Models;

public record CalculatorRunMessage
{
    public string MessageType { get; } = "Result";
    public required int CalculatorRunId { get; init; }
    public required string CreatedBy { get; init; }
}
