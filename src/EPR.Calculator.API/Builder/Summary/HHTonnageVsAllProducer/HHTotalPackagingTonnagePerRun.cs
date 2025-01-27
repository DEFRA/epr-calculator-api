namespace EPR.Calculator.API.Builder.Summary.HHTonnageVsAllProducer;

public record HHTotalPackagingTonnagePerRun
{
    public int ProducerId { get; set; }
    public required string SubsidiaryId { get; set; }
    public decimal TotalPackagingTonnage { get; set; }
}