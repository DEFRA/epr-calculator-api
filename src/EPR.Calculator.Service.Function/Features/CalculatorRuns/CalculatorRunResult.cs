using EPR.Calculator.Service.Function.Features.CalculatorRuns.Outputs;
using EPR.Calculator.Service.Function.Features.Common;

namespace EPR.Calculator.Service.Function.Features.CalculatorRuns;

public sealed record CalculatorRunResult : RunResult
{
    public override bool Succeeded => true;
    public required CalculatorFileResult ExportResult { get; init; }
}
