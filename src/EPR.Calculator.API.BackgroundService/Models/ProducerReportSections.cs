namespace EPR.Calculator.API.BackgroundService.Models;

/// <summary>
///     Deferred loaders for the two large per-producer report sections. A CSV export calls each one
///     immediately before the sub-exporter that writes it and lets the result fall out of scope
///     straight after, so H1/H2 projected producers, scaled-up producers and the fee-row stream are
///     never all resident at once - a run's file downloads otherwise hold the whole
///     <see cref="CalcResult" /> graph for the duration of the export.
/// </summary>
public sealed class ProducerReportSections
{
    public required Func<Task<CalcResultProjectedProducers>> LoadProjectedProducers { get; init; }

    public required Func<Task<CalcResultScaledupProducers>> LoadScaledupProducers { get; init; }
}
