namespace EPR.Calculator.API.Builder.CommsCost;

public class CalcResultCommsCostCommsCostByMaterial : CalcResultCommsCostOnePlusFourApportionment
{
    public string ProducerReportedWasteTonnage { get; set; }
    public string LateReportingTonnage { get; set; }
    public string ProducerReportedLateReportingTonnage { get; set; }
    public string CommsCostByMaterialPricePerTonne { get; set; }
    public string ProducerReportedWasteTonnageValue { get; set; }
    public string LateReportingTonnageValue { get; set; }
    public string ProducerReportedLateReportingTonnageValue { get; set; }
    public string CommsCostByMaterialPricePerTonneValue { get; set; }
}