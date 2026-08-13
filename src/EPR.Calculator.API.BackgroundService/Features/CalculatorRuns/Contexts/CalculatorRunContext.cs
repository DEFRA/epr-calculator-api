using EPR.Calculator.API.BackgroundService.Features.Common;

namespace EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;

/// <summary>
///     A context used exclusively for calculator runs.
/// </summary>
public record CalculatorRunContext : RunContext
{
    /// <inheritdoc />
    public override RunType RunType => RunType.Calculator;
}
