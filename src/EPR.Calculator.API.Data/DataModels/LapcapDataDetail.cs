namespace EPR.Calculator.API.Data.DataModels;

public class LapcapDataDetail
{
    public int Id { get; set; }
    public int LapcapDataMasterId { get; set; }
    public string UniqueReference { get; set; } = null!;
    public decimal TotalCost { get; set; }

    #region EF navigational properties

    public virtual LapcapDataMaster LapcapDataMaster { get; set; } = null!;
    public virtual LapcapDataTemplateMaster LapcapDataTemplateMaster { get; set; } = null!;

    #endregion
}
