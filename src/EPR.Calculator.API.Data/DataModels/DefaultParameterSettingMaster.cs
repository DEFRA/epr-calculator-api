using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("default_parameter_setting_master")]
    public class DefaultParameterSettingMaster
    {
        public int Id { get; set; }

        [Column("parameter_year")]
        [Required]
        public string ParameterYearId { get; set; }

        public required CalculatorRunFinancialYear ParameterYear { get; set; }

        [Column("effective_from")]
        [Required]
        public DateTime EffectiveFrom { get; set; }

        [Column("effective_to")]
        public DateTime? EffectiveTo { get; set; }

        [Column("created_by")]
        [Required]
        [StringLength(400)]
        public string CreatedBy { get; set; } = string.Empty;

        [Column("created_at")]
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("parameter_filename")]
        [Required]
        [StringLength(256)]
        public string ParameterFileName { get; set; } = string.Empty;

        public virtual ICollection<DefaultParameterSettingDetail> Details { get; } = new List<DefaultParameterSettingDetail>();

        public ICollection<CalculatorRun>? RunDetails { get; }
    }
}
