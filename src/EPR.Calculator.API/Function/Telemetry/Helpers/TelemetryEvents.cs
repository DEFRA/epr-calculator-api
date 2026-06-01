using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Models;
using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Service.Function.Telemetry.Helpers;

[ExcludeFromCodeCoverage]
public static class TelemetryEvents
{
    public static EventTelemetry RunInit() => new("RunInit");

    public static EventTelemetry RunStarted(MessageBase runContext)
    {
        return new EventTelemetry("RunStarted")
            .WithRunContext(runContext);
    }

    public static EventTelemetry RunCompleted(MessageBase runContext, TimeSpan duration)
    {
        return new EventTelemetry("RunCompleted")
            .WithRunContext(runContext)
            .WithDuration(duration);
    }

    public static EventTelemetry RunFailed(MessageBase runContext, TimeSpan duration, string reason)
    {
        return new EventTelemetry("RunFailed")
            .WithRunContext(runContext)
            .WithDuration(duration)
            .WithProperty("FailureReason", reason);
    }
}
