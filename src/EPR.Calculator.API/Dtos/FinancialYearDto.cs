namespace EPR.Calculator.API.Dtos
{
    public record FinancialYearDto
    {
        public required string Name { get; init; }

        public string? Description { get; init; }
    }
}
