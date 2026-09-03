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

    /// <summary>
    ///     Whether this org/subsidiary had a matching POM row this run. False means DataApi raised this
    ///     purely on the organisation's own status/leaver-code data with no current-year submission to
    ///     back it up - the caller should only surface it if it has other reason to (e.g. the
    ///     organisation was invoiced in a previous run this financial year), since DataApi has no
    ///     visibility into billing history.
    /// </summary>
    public required bool HasPomMatch { get; init; }
}
