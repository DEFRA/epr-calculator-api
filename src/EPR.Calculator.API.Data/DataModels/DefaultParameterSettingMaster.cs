using System.ComponentModel.DataAnnotations.Schema;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Data.DataModels
{
    public class DefaultParameterSettingMaster
    {
        public int Id { get; set; }

        public int RelativeYearValue { get; private set; }

        [NotMapped]
        public RelativeYear RelativeYear
        {
            get => new(RelativeYearValue);
            set => RelativeYearValue = value.Value;
        }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string ParameterFileName { get; set; } = string.Empty;

        public virtual ICollection<DefaultParameterSettingDetail> Details { get; } = new List<DefaultParameterSettingDetail>();

        public ICollection<CalculatorRun>? RunDetails { get; }
    }
}
