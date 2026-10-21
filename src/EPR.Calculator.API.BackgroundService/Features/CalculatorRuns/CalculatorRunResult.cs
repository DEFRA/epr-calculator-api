using EPR.Calculator.API.BackgroundService.Features.Common;

namespace EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;

public sealed record CalculatorRunResult : RunResult
{
    public override bool Succeeded => true;
}
