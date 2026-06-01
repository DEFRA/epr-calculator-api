using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Function.Messaging;
using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Service.Function.Telemetry.Helpers;

[ExcludeFromCodeCoverage]
public static class TelemetryExtensions
{
    public static TTelemetry WithProperty<TTelemetry>(this TTelemetry telemetry, string key, string value)
        where TTelemetry : ISupportProperties
    {
        telemetry.Properties[key] = value;
        return telemetry;
    }

    public static TTelemetry WithRunContext<TTelemetry>(this TTelemetry telemetry, MessageBase message)
        where TTelemetry : ISupportProperties
    {
        foreach (var (key, value) in message.Summary)
            telemetry.WithProperty(key, value?.ToString() ?? string.Empty);

        return telemetry;
    }

    public static TTelemetry WithDuration<TTelemetry>(this TTelemetry telemetry, TimeSpan elapsed)
        where TTelemetry : ISupportProperties
    {
        return telemetry
            .WithProperty("DurationMs", elapsed.TotalMilliseconds.ToString("F0"));
    }
}
