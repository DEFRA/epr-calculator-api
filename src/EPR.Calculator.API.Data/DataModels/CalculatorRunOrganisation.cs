namespace EPR.Calculator.API.Data.DataModels;

/// <summary>
///     Every organisation/subsidiary/submitter this run that DataApi returned a <c>ProducerRecord</c>
///     for: every obligated ("O") organisation - whether or not it has POM data of its own - plus every
///     "E"-status organisation and any org/subsidiary an error/warning was raised against. It is
///     <b>not</b> the full unfiltered Synapse population - a non-obligated organisation with no
///     error/warning gets no row here.
/// </summary>
/// <remarks>
///     A row here for an obligated organisation with no <see cref="ProducerDetail" /> of its own (e.g.
///     a holding company whose subsidiaries submit all the POM data) is relied on directly, in the same
///     run, by <c>ProducerFeesBuilder</c> and <c>CalcResultScaledupProducersBuilder</c> to find that
///     parent's identity. Several other consumers rely on this table as a name/status snapshot for
///     producers that have since stopped being obligated but are still relevant (a cancelled/lapsed
///     producer still being billed, say) - <c>InvoicedProducerService</c>'s cross-run "latest org name"
///     lookup, and <c>BillingFileService</c>'s explicit fallback for producers "deleted in the pom data"
///     but not in <see cref="ProducerDetail" /> - by searching earlier runs this financial year, not by
///     requiring every run to carry a row for every organisation regardless of relevance.
/// </remarks>
public class CalculatorRunOrganisation
{
    public int Id { get; set; }
    public int CalculatorRunId { get; set; }
    public int OrganisationId { get; set; }
    public string? SubsidiaryId { get; set; }
    public required string OrganisationName { get; set; }
    public string? TradingName { get; set; }
    public int? DaysObligated { get; set; }
    public string? JoinerDate { get; set; }
    public string? LeaverDate { get; set; }
    public string? StatusCode { get; set; }
    public string? ErrorCode { get; set; }

    /// <summary>
    ///     Whether this row was excluded from calculation by a hard error - <c>ProducerRecord.IsError</c>
    ///     at persistence time. Use this to tell a genuine calculation participant (obligated, with or
    ///     without a warning) apart from a row that exists only to carry error/warning data for the
    ///     error report.
    /// </summary>
    public bool IsError { get; set; }

    #region EF navigational properties

    public virtual CalculatorRun CalculatorRun { get; set; } = null!;

    #endregion
}
