namespace EPR.CommonDataService.DataApi.Alignment;

/// <summary>
///     A <see cref="ProducerCalculationError" /> keyed by the org/subsidiary it was raised against. Used
///     only where a flat, keyed error list is still needed - detection output, and re-flattening a
///     <see cref="ProducerRecord" />'s errors/warnings for persistence.
/// </summary>
public sealed record OrganisationCalculationError
{
    public required int OrganisationId { get; init; }
    public string? SubsidiaryId { get; init; }
    public required ProducerCalculationError Error { get; init; }
}
