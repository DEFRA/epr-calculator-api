using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Features.BillingRuns.Outputs;

/// <summary>
///     Encapsulates the metadata for the billing files that were written to blob storage.
/// </summary>
public record BillingFileResult
{
    public required CalculatorRunCsvFileMetadata CsvMetadata { get; init; }
    public required CalculatorRunBillingFileMetadata JsonMetadata { get; init; }
}
