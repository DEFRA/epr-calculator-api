namespace EPR.CommonDataService.DataApi.Alignment;

/// <summary>
///     Everything DataApi knows about one organisation/subsidiary/submitter this run: its identity,
///     any hard error or warnings raised against it, and the packaging data it reported (if any).
/// </summary>
/// <remarks>
///     A non-empty <see cref="Errors" /> and a non-empty <see cref="ReportedMaterials" /> are mutually
///     exclusive - a hard error always excludes the org/subsidiary from calculation. <see cref="Warnings" />
///     are not exclusive - a warning can be raised against an org/subsidiary that still reported
///     packaging data.
/// </remarks>
public sealed record ProducerRecord
{
    public required int OrganisationId { get; init; }
    public string? SubsidiaryId { get; init; }
    public required string ProducerName { get; init; }
    public string? TradingName { get; init; }
    public int? DaysObligated { get; init; }
    public string? JoinerDate { get; init; }
    public string? LeaverDate { get; init; }
    public string? StatusCode { get; init; }

    /// <summary>The organisation's own raw registration error code, if any - distinct from <see cref="Errors" />/<see cref="Warnings" />, which are DataApi's computed rule results.</summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    ///     Hard (non-warning) errors. Rare but possible to have more than one, e.g. an "E"-status
    ///     organisation whose POM data also fails the missing-registration check. When non-empty, this
    ///     org/subsidiary was excluded from calculation and <see cref="ReportedMaterials" /> is always
    ///     empty.
    /// </summary>
    public required IReadOnlyList<ProducerCalculationError> Errors { get; init; }

    /// <summary>
    ///     Soft errors/warnings. Unlike <see cref="Errors" />, these don't exclude the org/subsidiary
    ///     from calculation - <see cref="ReportedMaterials" /> may still be non-empty alongside them.
    /// </summary>
    public required IReadOnlyList<ProducerCalculationError> Warnings { get; init; }

    public required IReadOnlyList<AlignedReportedMaterial> ReportedMaterials { get; init; }

    /// <summary>
    ///     Whether this org/subsidiary was excluded from calculation by a hard error - the two-way
    ///     split consumers should use to tell a genuine calculation participant (obligated, with or
    ///     without a warning) apart from a row that exists only to carry error/warning data for the
    ///     error report. <see cref="Warnings" /> don't affect this - a warned org/subsidiary is still
    ///     valid - but a non-empty <see cref="Errors" /> always does.
    /// </summary>
    public bool IsError => Errors.Count > 0;
}
