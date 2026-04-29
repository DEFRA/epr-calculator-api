using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.API.Enums;

namespace EPR.Calculator.API.Dtos;

public record CalculatorRunDto
{
    public required int RunId { get; init; }
    public required RelativeYear RelativeYear { get; init; }
    public required string RunName { get; init; }
    public required RunClassification RunClassification { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string CreatedBy { get; init; }
    public required DateTime? UpdatedAt { get; init; }
    public required string? UpdatedBy { get; init; }
    public required BillingRunStatus BillingRunStatus { get; init; }
    public required DateTime? BillingRunStartedAt { get; init; }
    public required BillingMetadataDto? CompletedBillingRun { get; init; }

    public record BillingMetadataDto
    {
        public required string CsvFileName { get; init; }
        public required string JsonFileName { get; init; }
        public required DateTime CreatedAt { get; init; }
        public required string CreatedBy { get; init; }
        public required DateTime? AuthorisedAt { get; init; }
        public required string? AuthorisedBy { get; init; }
    }
}
