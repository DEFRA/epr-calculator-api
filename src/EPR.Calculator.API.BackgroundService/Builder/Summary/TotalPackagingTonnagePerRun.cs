namespace EPR.Calculator.API.BackgroundService.Builder.Summary;

public record TotalPackagingTonnagePerRun
{
    public int ProducerId { get; set; }
    public string? SubsidiaryId { get; set; }
    public decimal TotalPackagingTonnage { get; set; }
}
