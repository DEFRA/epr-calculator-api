using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Outputs;

public record CalculatorFileResult
{
    public required CalculatorRunCsvFileMetadata CsvMetadata { get; init; }
}
