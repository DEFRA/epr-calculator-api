namespace EPR.Calculator.API.Data.DataModels;

/// <summary>
///     Every organisation/subsidiary/submitter seen in a calculator run's Synapse pull, deduped but
///     not filtered by obligation status or POM match - the full population a run's data was drawn
///     from, for consumers that need to see organisations that never became a <see cref="ProducerDetail" />.
/// </summary>
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
