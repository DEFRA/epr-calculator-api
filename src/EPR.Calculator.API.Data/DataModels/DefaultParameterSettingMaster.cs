using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.Data.DataModels;

public class DefaultParameterSettingMaster
{
    public int Id { get; set; }
    public RelativeYear RelativeYear { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string ParameterFileName { get; set; } = string.Empty;

    #region EF navigational properties

    public virtual ICollection<DefaultParameterSettingDetail> Details { get; } = [];
    public virtual ICollection<CalculatorRun> RunDetails { get; } = [];

    #endregion
}
