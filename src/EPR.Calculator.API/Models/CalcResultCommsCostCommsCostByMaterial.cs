namespace EPR.Calculator.API.Models;

public class CalcResultCommsCostCommsCostByMaterial : CalcResultCommsCostOnePlusFourApportionment
{
    public string ProducerReportedHouseholdPackagingWasteTonnage { get; set; }
    public string LateReportingTonnage { get; set; }
    public string ProducerReportedHouseholdPlusLateReportingTonnage { get; set; }
    public string CommsCostByMaterialPricePerTonne { get; set; }
    public decimal ProducerReportedHouseholdPackagingWasteTonnageValue { get; set; }
    public decimal LateReportingTonnageValue { get; set; }
    public decimal ProducerReportedHouseholdPlusLateReportingTonnageValue { get; set; }
    public decimal CommsCostByMaterialPricePerTonneValue { get; set; }
}