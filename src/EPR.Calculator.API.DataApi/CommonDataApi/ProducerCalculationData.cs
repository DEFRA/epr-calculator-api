using EPR.CommonDataService.DataApi.Alignment;

namespace EPR.CommonDataService.DataApi.CommonDataApi;

/// <summary>
///     The full result of a calculator run's producer data calculation: the raw organisation
///     population (for callers that need to see organisations that never became a producer), the
///     aligned producers ready for calculation, and every error/warning raised along the way.
/// </summary>
public sealed record ProducerCalculationData
{
    public required IReadOnlyList<AlignmentOrganisation> Organisations { get; init; }
    public required IReadOnlyList<AlignedProducer> Producers { get; init; }
    public required IReadOnlyList<ProducerCalculationError> Errors { get; init; }
}
