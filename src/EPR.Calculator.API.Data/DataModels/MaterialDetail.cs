namespace EPR.Calculator.API.Data.DataModels;

public record MaterialDetail
{
    public required int Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
}
