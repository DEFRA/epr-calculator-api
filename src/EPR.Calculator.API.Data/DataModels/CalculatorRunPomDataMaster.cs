using System.ComponentModel.DataAnnotations.Schema;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Data.DataModels;

public class CalculatorRunPomDataMaster
{
    public int Id { get; set; }
    public int RelativeYearValue { get; private set; }

    [NotMapped]
    public RelativeYear RelativeYear
    {
        get => new(RelativeYearValue);
        set => RelativeYearValue = value.Value;
    }

    public required DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public required string CreatedBy { get; set; }
    public required DateTime CreatedAt { get; set; }

    #region EF navigational properties

    public virtual ICollection<CalculatorRunPomDataDetail> Details { get; } = [];
    public virtual ICollection<CalculatorRun> Runs { get; } = [];

    #endregion
}
