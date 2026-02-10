namespace EPR.Calculator.API.Dtos
{
    public record RelativeYearDto
    {
        public required int Value { get; init; }

        public string? Description { get; init; }
    }
}
