using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.Dtos;

public record CreateLapcapDataRequest
{
    public string? Filename { get; init; }
    public RelativeYear? RelativeYear { get; init; }
    public ImmutableList<LapcapValue>? Values { get; init; }

    public record LapcapValue
    {
        public string? Country { get; init; }
        public string? Material { get; init; }
        public decimal? TotalCost { get; init; }
    }
}
