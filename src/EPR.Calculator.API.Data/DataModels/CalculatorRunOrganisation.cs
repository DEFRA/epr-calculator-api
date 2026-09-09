namespace EPR.Calculator.API.Data.DataModels;

/// <summary>
///     Every organisation/subsidiary/submitter seen in a calculator run's Synapse pull, deduped but
///     not filtered by obligation status or POM match - the full population a run's data was drawn
///     from, for consumers that need to see organisations that never became a <see cref="ProducerDetail" />.
/// </summary>
/// <remarks>
///     This table must keep receiving the *unfiltered* organisation population, not just those that
///     survived into <see cref="ProducerDetail" /> - several consumers rely on it as a per-run name/status
///     snapshot for producers that have since stopped submitting POM data but are still relevant (a
///     cancelled/lapsed producer still being billed, say): <c>InvoicedProducerService</c>'s
///     cross-run "latest org name" lookup, <c>BillingFileService</c>'s explicit fallback for producers
///     "deleted in the pom data" but not in <c>ProducerDetail</c>, and the rejected/scaled-up-producer
///     and error-report builders. If DataApi is ever extracted to its own external service, its response
///     contract (<c>ProducerCalculationData.Organisations</c> today) must keep returning this full
///     population alongside the aligned producers - narrowing it to "just the producers" would silently
///     break every one of those lookups with no compile error and no obvious test failure.
/// </remarks>
public class CalculatorRunOrganisation
{
    public int Id { get; set; }
    public int CalculatorRunId { get; set; }
    public int OrganisationId { get; set; }
    public string? SubsidiaryId { get; set; }
    public Guid? SubmitterId { get; set; }
    public required string OrganisationName { get; set; }
    public string? TradingName { get; set; }
    public string ObligationStatus { get; set; } = string.Empty;
    public int? DaysObligated { get; set; }
    public string? JoinerDate { get; set; }
    public string? LeaverDate { get; set; }
    public string? StatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public bool HasH1 { get; set; }
    public bool HasH2 { get; set; }

    #region EF navigational properties

    public virtual CalculatorRun CalculatorRun { get; set; } = null!;

    #endregion
}
