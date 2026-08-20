using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.App;
using OpenTelemetry;

namespace EPR.Calculator.API.Filters;

/// <summary>
///     For use with OpenTelemetry; filters out activities related to various endpoint probes.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class EndpointProbeActivityFilter : BaseProcessor<Activity>
{
    private static readonly HashSet<string> IgnoredPaths = new (StringComparer.OrdinalIgnoreCase)
    {
        "/robots933456.txt", // Azure App Service dummy
        WebAppConfiguration.HealthCheckPath
    };

    public override void OnEnd(Activity activity)
    {
        // ASP.NET Core incoming request spans are Server activities.
        if (activity.Kind != ActivityKind.Server)
            return;

        // ASP.NET Core should have populated this tag since we're hooking into OnEnd.
        var path = activity.GetTagItem("url.path")?.ToString();

        if (path is not null && IgnoredPaths.Contains(path))
            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
    }
}
