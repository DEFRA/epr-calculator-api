using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Function.Features.Common;

namespace EPR.Calculator.Service.Function.Exceptions;

/// <summary>
///     Thrown at the end of a run if state changes could not be persisted to the database.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[ExcludeFromCodeCoverage]
public class RunFinalizeException(RunType runType, int runId, Exception innerException, string? message = null)
    : Exception(message ?? "Unable to finalize run, see inner exception for details.", innerException)
{
    public RunType RunType { get; } = runType;
    public int RunId { get; } = runId;
}