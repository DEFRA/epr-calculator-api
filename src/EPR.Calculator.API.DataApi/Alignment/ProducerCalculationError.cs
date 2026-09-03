namespace EPR.CommonDataService.DataApi.Alignment;

/// <summary>
///     A calculation-blocking error or informational warning raised against a specific org/subsidiary
///     while detecting registration/POM data issues. Errors exclude the org/subsidiary from
///     <see cref="AlignedProducer" /> output; warnings do not - the org/subsidiary still gets a matching
///     <see cref="AlignedProducer" /> alongside the warning.
/// </summary>
public sealed record ProducerCalculationError
{
    public required int OrganisationId { get; init; }
    public string? SubsidiaryId { get; init; }
    public required string ErrorCode { get; init; }
    public required string LeaverCode { get; init; }
    public required bool IsWarning { get; init; }
}
