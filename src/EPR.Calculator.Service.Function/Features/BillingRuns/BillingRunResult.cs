using EPR.Calculator.Service.Function.Features.BillingRuns.Outputs;
using EPR.Calculator.Service.Function.Features.Common;

namespace EPR.Calculator.Service.Function.Features.BillingRuns;

public sealed record BillingRunResult : RunResult
{
    public override bool Succeeded => true;
    public required BillingFileResult ExportResult { get; init; }
}
