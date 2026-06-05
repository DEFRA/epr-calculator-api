using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Features.CalculatorRun.Outputs;

public record CalculatorFileResult
{
    public required CalculatorRunCsvFileMetadata CsvMetadata { get; init; }
}
