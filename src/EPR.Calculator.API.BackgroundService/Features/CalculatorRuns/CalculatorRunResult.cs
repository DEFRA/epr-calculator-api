using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Outputs;
using EPR.Calculator.API.BackgroundService.Features.Common;

namespace EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;

public sealed record CalculatorRunResult : RunResult
{
    public override bool Succeeded => true;
    public required CalculatorFileResult ExportResult { get; init; }
}
