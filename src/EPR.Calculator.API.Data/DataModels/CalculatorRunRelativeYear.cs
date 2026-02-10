namespace EPR.Calculator.API.Data.DataModels
{
    public record CalculatorRunRelativeYear
    {
        public required int Value { get; set; }

        public string? Description { get; set; }

        public override string ToString() => Value.ToString();
    }
}
