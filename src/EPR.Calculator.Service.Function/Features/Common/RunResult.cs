namespace EPR.Calculator.Service.Function.Features.Common;

public abstract record RunResult
{
    public abstract bool Succeeded { get; }
}

public sealed record BadResult : RunResult
{
    public override bool Succeeded { get; } = false;
    public required Exception Exception { get; init; }
}
