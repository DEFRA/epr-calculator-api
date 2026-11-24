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
    public required bool IsTotal { get; set; }
    public required int DaysInSubmissionPeriod { get; set; }
    public required int DaysInWholePeriod { get; set; }
    public required decimal ScaleupFactor { get; set; }
    public required string MaterialCode { get; set; }
    public required decimal ReportedHouseholdPackagingWasteTonnage { get; set; }
    public required decimal ReportedPublicBinTonnage { get; set; }
    public required decimal HouseholdDrinksContainersTonnageGlass { get; set; }
    public required decimal TotalReportedTonnage { get; set; }
    public required decimal ReportedSelfManagedConsumerWasteTonnage { get; set; }
    public required decimal NetReportedTonnage { get; set; }
    public required decimal ScaledupReportedHouseholdPackagingWasteTonnage { get; set; }
    public required decimal ScaledupReportedPublicBinTonnage { get; set; }
    public required decimal ScaledupHouseholdDrinksContainersTonnageGlass { get; set; }
    public required decimal ScaledupTotalReportedTonnage { get; set; }
    public required decimal ScaledupReportedSelfManagedConsumerWasteTonnage { get; set; }
    public required decimal ScaledupNetReportedTonnage { get; set; }
}
