namespace EPR.CommonDataService.DataApi.CommonDataApi.Alignment;

public sealed record AlignedProducer
{
    public required int OrganisationId { get; init; }
    public string? SubsidiaryId { get; init; }
    public string? TradingName { get; init; }
    public required string ProducerName { get; init; }
    public required IReadOnlyList<AlignedReportedMaterial> ReportedMaterials { get; init; }
}
