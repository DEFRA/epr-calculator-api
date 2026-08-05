using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.BackgroundService.Features.Common;

namespace EPR.Calculator.API.BackgroundService.Exceptions;

/// <summary>
///     Thrown at the end of a run if state changes could not be persisted to the database.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[ExcludeFromCodeCoverage]
public class RunDataInitializeException(RunType runType, int runId, Exception innerException, string? message = null)
    : Exception(message ?? "Unable to load/transpose data for run, see inner exception for details.", innerException)
{
    public RunType RunType { get; } = runType;
    public int RunId { get; } = runId;
}
