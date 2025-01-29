namespace EPR.Calculator.API.Models;

public class CalcResultCommsCostCommsCostByMaterial : CalcResultCommsCostOnePlusFourApportionment
{
    public string ProducerReportedHouseholdPackagingWasteTonnage { get; set; } = string.Empty;
    public string LateReportingTonnage { get; set; } = string.Empty;
    public string ProducerReportedHouseholdPlusLateReportingTonnage { get; set; } = string.Empty;
    public string CommsCostByMaterialPricePerTonne { get; set; } = string.Empty;
    public decimal ProducerReportedHouseholdPackagingWasteTonnageValue { get; set; }
    public decimal LateReportingTonnageValue { get; set; }
    public decimal ReportedPublicBinTonnageValue { get; set; }
    public decimal HouseholdDrinksContainersValue { get; set; }
    public string? ReportedPublicBinTonnage { get; set; } = string.Empty;
    public string? HouseholdDrinksContainers { get; set; } = string.Empty;
    public decimal ProducerReportedTotalTonnage  { get; set; }
    public string? TotalReportedTonnage { get; set; }
    public decimal CommsCostByMaterialPricePerTonneValue { get; set; }
}