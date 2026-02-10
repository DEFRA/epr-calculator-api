using System.ComponentModel.DataAnnotations.Schema;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Data.DataModels
{
    public class LapcapDataMaster
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

        public string LapcapFileName { get; set; } = string.Empty;

        public ICollection<LapcapDataDetail> Details { get; } = new List<LapcapDataDetail>();

        public ICollection<CalculatorRun>? RunDetails { get; }
    }
}
