namespace EPR.Calculator.API.BackgroundService.Telemetry.Helpers;

public record TelemetryScope
{
    public string? Label { get; init; }
    public required KeyValuePair<string, object?>[] ActivityTags { get; init; }
    public required KeyValuePair<string, object?>[] MetricTags { get; init; }
}
