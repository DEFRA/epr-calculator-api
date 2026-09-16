using EPR.Calculator.API.Data.Enums;

namespace EPR.Calculator.API.Data.DataModels;

public class CalculatorRunClassification
{
    public RunClassification Id { get; set; }
    public required string Status { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
