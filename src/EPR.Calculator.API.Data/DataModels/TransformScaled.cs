namespace EPR.Calculator.API.Data.DataModels;

public class TransformScaled
{
    public int Id { get; set; }
    public required int CalculatorRunId { get; set; }
    public required int ProducerId { get; set; }
    public string? SubsidiaryId { get; set; }
    public string? ProducerName { get; set; }
    public string? TradingName { get; set; }
    public required string SubmissionPeriodCode { get; set; }
    public required string Level { get; set; }
    public required bool IsSubTotal { get; set; }
    public required int DaysInSubmissionPeriod { get; set; }
    public required int DaysInWholePeriod { get; set; }
    public required decimal ScaleupFactor { get; set; }
    public required int MaterialId { get; set; }
    public required string PackagingType { get; set; }
    public required decimal Tonnage { get; set; }
    public required decimal ScaledTonnage { get; set; }
}
