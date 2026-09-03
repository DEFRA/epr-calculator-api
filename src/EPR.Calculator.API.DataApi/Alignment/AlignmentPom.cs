namespace EPR.CommonDataService.DataApi.Alignment;

public sealed record AlignmentPom
{
    public int? OrganisationId { get; init; }
    public string? SubsidiaryId { get; init; }
    public Guid? SubmitterId { get; init; }
    public string? PackagingMaterial { get; init; }
    public string? PackagingType { get; init; }
    public string? SubmissionPeriod { get; init; }
    public double? PackagingMaterialWeight { get; init; }

    /// <summary>
    ///     RAG rating code, matching <c>RagRatingExtensions.ToDbValue()</c>: "R", "A", "G", "R-M", "A-M", "G-M".
    /// </summary>
    public string? RamRagRating { get; init; }
}
