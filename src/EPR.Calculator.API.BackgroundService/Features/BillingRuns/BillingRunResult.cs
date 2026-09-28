using EPR.Calculator.API.BackgroundService.Features.Common;

namespace EPR.Calculator.API.BackgroundService.Features.BillingRuns;

public sealed record BillingRunResult : RunResult
{
    public override bool Succeeded => true;
}
