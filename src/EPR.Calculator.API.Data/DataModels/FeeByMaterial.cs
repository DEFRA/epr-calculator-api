namespace EPR.Calculator.API.Data.DataModels;

public class MaterialFee
{
    public required string MaterialCode { get; set; }

    public required DisposalFee DisposalFee { get; set; }

    public required CommsFee CommFee { get; set; }

}
