namespace EPR.Calculator.Service.Function.Models
{
    public record MaterialDetail
    {
        public required int Id { get; init; }
        public required string Code { get; init; }
        public required string Name { get; init; }
    }
}
