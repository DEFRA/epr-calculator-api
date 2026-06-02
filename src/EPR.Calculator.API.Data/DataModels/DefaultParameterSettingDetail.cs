namespace EPR.Calculator.API.Data.DataModels;

public class DefaultParameterSettingDetail
{
    public int Id { get; set; }
    public int DefaultParameterSettingMasterId { get; set; }
    public required string ParameterUniqueReferenceId { get; set; }
    public decimal ParameterValue { get; set; }

    #region EF navigational properties

    public virtual DefaultParameterSettingMaster DefaultParameterSettingMaster { get; set; } = null!;
    public virtual DefaultParameterTemplateMaster ParameterUniqueReference { get; set; } = null!;

    #endregion
}
