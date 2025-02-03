namespace EPR.Calculator.API.Builder.Summary.TonnageVsAllProducer;

public record TotalPackagingTonnagePerRun
{
    public int ProducerId { get; set; }
    public required string SubsidiaryId { get; set; }
    public decimal TotalPackagingTonnage { get; set; }
}