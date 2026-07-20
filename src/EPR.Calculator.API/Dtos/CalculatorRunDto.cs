using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Enums;

namespace EPR.Calculator.API.Dtos;

public record CalculatorRunDto
{
    public required int RunId { get; init; }
    public required RunClassification RunClassification { get; init; }
    public required RelativeYear RelativeYear { get; init; }
    public required string RunName { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string CreatedBy { get; init; }
    public required DateTime? UpdatedAt { get; init; }
    public required string? UpdatedBy { get; init; }
    public required BillingRunStatus BillingRunStatus { get; init; }
    public required DateTime? BillingRunStartedAt { get; init; }
    public required BillingFileDto? BillingFile { get; init; }

    public record BillingFileDto
    {
        public required int Id { get ; init; }
        public required bool IsLatest { get ; init; }
        public required bool HasBeenSentToFss { get; init; }
        public required string CsvFileName { get; init; }
        public required string JsonFileName { get; init; }
        public required DateTime CreatedAt { get; init; }
        public required string CreatedBy { get; init; }
        public required DateTime? SentAt { get; init; }
        public required string? SentBy { get; init; }
    }
}
