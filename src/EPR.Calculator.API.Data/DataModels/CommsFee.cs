namespace EPR.Calculator.API.Data.DataModels;

public class CommsFee
{
    public decimal HhTonnage { get; set; }
    public decimal PbTonnage { get; set; }
    public decimal HdcTonnage { get; set; }
    public decimal TotalTonnage { get; set; }
    public decimal PricePerTonne { get; set; }
    public required FeeWithBadDebt Costs { get; set; }
}
