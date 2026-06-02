using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.Data.DataModels;

public class CalculatorRunPomDataMaster
{
    public int Id { get; set; }
    public RelativeYear RelativeYear { get; set; }
    public required DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public required string CreatedBy { get; set; }
    public required DateTime CreatedAt { get; set; }

    #region EF navigational properties

    public virtual ICollection<CalculatorRunPomDataDetail> Details { get; } = [];
    public virtual ICollection<CalculatorRun> Runs { get; } = [];

    #endregion
}
