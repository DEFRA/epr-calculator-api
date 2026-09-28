using System.Diagnostics.CodeAnalysis;

namespace EPR.CommonDataService.DataApi.CommonDataApi.Entities;

[ExcludeFromCodeCoverage]
public record PayCalPom
{
    public int? OrganisationId { get; init; }
    public string? SubsidiaryId { get; init; }
    public string? SubmitterId { get; init; }
    public string? SubmissionPeriod { get; init; }
    public string? SubmissionPeriodDescription { get; init; }
    public string? PackagingActivity { get; init; }
    public string? PackagingType { get; init; }
    public string? PackagingClass { get; init; }
    public string? PackagingMaterial { get; init; }
    public string? PackagingMaterialSubtype { get; init; }
    public double? PackagingMaterialWeight { get; init; }
    public string? RamRagRating { get; init; }

    // File-selection inputs only (see IAcceptedFileSelector) - not carried past that stage.
    public string? FileName { get; init; }
    public bool IsResubmission { get; init; }
    public DateTime? CreatedDateTime { get; init; }
}
