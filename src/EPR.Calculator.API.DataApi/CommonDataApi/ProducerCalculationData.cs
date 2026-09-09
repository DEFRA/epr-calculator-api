using EPR.CommonDataService.DataApi.Alignment;

namespace EPR.CommonDataService.DataApi.CommonDataApi;

/// <summary>
///     The full result of a calculator run's producer data calculation: the raw organisation
///     population (for callers that need to see organisations that never became a producer), the
///     aligned producers ready for calculation, and every error/warning raised along the way.
/// </summary>
public sealed record ProducerCalculationData
{
    /// <summary>
    ///     Every organisation/subsidiary/submitter seen this run, unfiltered by obligation status or POM
    ///     match - <b>not</b> a superset that can be derived from <see cref="Producers" /> after the
    ///     fact. The caller persists this as-is into <c>CalculatorRunOrganisation</c>, which several
    ///     downstream consumers depend on as a per-run name/status snapshot for producers that never made
    ///     it into <see cref="Producers" /> - e.g. a cancelled producer still being billed, whose latest
    ///     known name has to come from here rather than this run's (empty) aligned output. If this field
    ///     is ever dropped or narrowed to match <see cref="Producers" />, those lookups break silently:
    ///     no compile error, no obvious test failure, just wrong or missing organisation names in billing
    ///     files and reports. See the <c>CalculatorRunOrganisation</c> doc comment for the full list of
    ///     affected consumers.
    /// </summary>
    public required IReadOnlyList<AlignmentOrganisation> Organisations { get; init; }

    public required IReadOnlyList<AlignedProducer> Producers { get; init; }
    public required IReadOnlyList<ProducerCalculationError> Errors { get; init; }
}
