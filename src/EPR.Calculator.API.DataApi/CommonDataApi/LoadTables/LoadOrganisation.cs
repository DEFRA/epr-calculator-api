namespace EPR.CommonDataService.DataApi.CommonDataApi.LoadTables;

/// <summary>
///     A row of <c>data_api_load_organisations</c>: a faithful snapshot of an organisation row as
///     <see cref="StreamOrganisationsRequestHandler" /> yielded it, staged so a run can read it back
///     without a second query against the slow RPD source. The RPD handler leaves the computed columns
///     (<c>obligation_status</c>, <c>has_h1</c>/<c>has_h2</c>, …) unset - they are filled downstream -
///     but they are stored so the round-trip loses nothing.
/// </summary>
public sealed class LoadOrganisation
{
    public int Id { get; set; }

    public int? OrganisationId { get; set; }
    public string? SubsidiaryId { get; set; }
    public string? SubmitterId { get; set; }
    public string? OrganisationName { get; set; }
    public string? TradingName { get; set; }
    public string? StatusCode { get; set; }
    public string? LeaverDate { get; set; }
    public string? JoinerDate { get; set; }
    public string? RegulatorStatus { get; set; }
    public string? ObligationStatus { get; set; }
    public short? NumDaysObligated { get; set; }
    public string? ErrorCode { get; set; }
    public int? SubmissionPeriodYear { get; set; }
    public bool HasH1 { get; set; }
    public bool HasH2 { get; set; }
    public string? FileName { get; set; }
    public bool IsResubmission { get; set; }
    public DateTime? CreatedDateTime { get; set; }

    public DateTime LoadTs { get; set; }
}
