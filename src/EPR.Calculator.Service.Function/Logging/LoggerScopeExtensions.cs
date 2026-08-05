using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Function.Features.Common;

namespace EPR.Calculator.Service.Function.Logging;

[ExcludeFromCodeCoverage]
public static class LoggerScopeExtensions
{
    public static IDisposable? BeginRunScope(this ILogger logger, RunContext runContext)
    {
        return logger.BeginScope(new Dictionary<string, object>
        {
            { "RunType", runContext.RunType.ToString() },
            { "RunId", runContext.RunId },
            { "RunName", runContext.RunName }
        });
    }
}
