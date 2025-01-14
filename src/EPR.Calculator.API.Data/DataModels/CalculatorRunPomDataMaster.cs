using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("calculator_run_pom_data_master")]
    public class CalculatorRunPomDataMaster
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("calendar_year")]
        public required string CalendarYear { get; set; }

        [Column("effective_from")]
        public required DateTime EffectiveFrom { get; set; }

        [Column("effective_to")]
        public DateTime? EffectiveTo { get; set; }

        [Column("created_by")]
        public required string CreatedBy { get; set; }

        [Column("created_at")]
        public required DateTime CreatedAt { get; set; }

        public virtual ICollection<CalculatorRunPomDataDetail> Details { get; } = new List<CalculatorRunPomDataDetail>();

        public ICollection<CalculatorRun>? RunDetails { get; }
    }
}
