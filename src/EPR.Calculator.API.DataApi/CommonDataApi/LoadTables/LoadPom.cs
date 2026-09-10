namespace EPR.CommonDataService.DataApi.CommonDataApi.LoadTables;

/// <summary>
///     A row of <c>data_api_load_poms</c>: the raw POM line-item columns selected by
///     <see cref="StreamPomsRequestHandler" />, staged so a run's two dedup passes read a fast local
///     table instead of querying the RPD source twice.
/// </summary>
public sealed class LoadPom
{
    public long Id { get; set; }

    public int? OrganisationId { get; set; }
    public string? SubsidiaryId { get; set; }
    public string? SubmitterId { get; set; }
    public string? SubmissionPeriod { get; set; }
    public string? SubmissionPeriodDescription { get; set; }
    public string? PackagingActivity { get; set; }
    public string? PackagingType { get; set; }
    public string? PackagingClass { get; set; }
    public string? PackagingMaterial { get; set; }
    public string? PackagingMaterialSubtype { get; set; }
    public double? PackagingMaterialWeight { get; set; }
    public string? RamRagRating { get; set; }
    public string? FileName { get; set; }
    public bool IsResubmission { get; set; }
    public DateTime? CreatedDateTime { get; set; }

    public DateTime LoadTs { get; set; }
}
