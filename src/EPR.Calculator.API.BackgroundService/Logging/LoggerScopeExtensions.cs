using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.BackgroundService.Features.Common;

namespace EPR.Calculator.API.BackgroundService.Logging;

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
