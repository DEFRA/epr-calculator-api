using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Function.Features.Common;

namespace EPR.Calculator.Service.Function.Exceptions;

/// <summary>
///     Thrown during run processing (likely due to invalid/missing state).
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[ExcludeFromCodeCoverage]
public class RunProcessingException(RunContext runContext, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public RunProcessingException(RunContext runContext, Exception innerException)
        : this(runContext, "Unable to process run, see inner exception for details.", innerException)
    {
    }

    public RunType RunType { get; } = runContext.RunType;
    public int RunId { get; } = runContext.RunId;
    public string RunName { get; } = runContext.RunName;
}