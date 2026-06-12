using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.Data.DataModels;

public class CalculatorRunRelativeYear
{
    public required RelativeYear Value { get; set; }
    public string? Description { get; set; }

    public override string ToString() => Value.ToString();
}
