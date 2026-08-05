using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.BackgroundService.Features.Common;

namespace EPR.Calculator.API.BackgroundService.Exceptions;

/// <summary>
///     Thrown before a run starts if a valid context cannot be constructed (likely due to invalid state or missing data).
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[ExcludeFromCodeCoverage]
public class RunContextException(RunType runType, int runId, string message)
    : Exception(message)
{
    public RunType RunType { get; } = runType;
    public int RunId { get; } = runId;
}
