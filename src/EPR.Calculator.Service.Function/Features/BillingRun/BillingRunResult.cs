using EPR.Calculator.Service.Function.Features.BillingRun.Outputs;
using EPR.Calculator.Service.Function.Features.Common;

namespace EPR.Calculator.Service.Function.Features.BillingRun;

public sealed record BillingRunResult : RunResult
{
    public override bool Succeeded => true;
    public required BillingFileResult ExportResult { get; init; }
}
