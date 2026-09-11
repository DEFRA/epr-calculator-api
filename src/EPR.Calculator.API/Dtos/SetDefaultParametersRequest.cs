using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.Dtos;

public record SetDefaultParametersRequest
{
    public string? Filename { get; init; }
    public RelativeYear? RelativeYear { get; init; }
    public ImmutableList<ParameterValue>? Parameters { get; init; }

    public record ParameterValue
    {
        public string? Id { get; init; }
        public string? Value { get; init; }
    }
}
