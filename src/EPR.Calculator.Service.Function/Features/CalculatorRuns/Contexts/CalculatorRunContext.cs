using EPR.Calculator.Service.Function.Features.Common;

namespace EPR.Calculator.Service.Function.Features.CalculatorRuns.Contexts;

/// <summary>
///     A context used exclusively for calculator runs.
/// </summary>
public record CalculatorRunContext : RunContext
{
    /// <inheritdoc />
    public override RunType RunType => RunType.Calculator;
}
