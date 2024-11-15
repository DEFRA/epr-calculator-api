namespace EPR.Calculator.API.Builder.CommsCost;

public class CalcResultCommsCostCommsCostByMaterial : CalcResultCommsCostOnePlusFourApportionment
{
    public string ProducerReportedWasteTonnage { get; set; }
    public string LateReportingTonnage { get; set; }
    public string ProducerReportedLateReportingTonnage { get; set; }
    public string CommsCostByMaterialPricePerTonne { get; set; }
    public decimal ProducerReportedWasteTonnageValue { get; set; }
    public decimal LateReportingTonnageValue { get; set; }
    public decimal ProducerReportedLateReportingTonnageValue { get; set; }
    public decimal CommsCostByMaterialPricePerTonneValue { get; set; }
}