using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("default_parameter_template_master")]
    public class DefaultParameterTemplateMaster
    {
        [Column("parameter_unique_ref")]
        [Key]
        [NotNull]
        [StringLength(450)]
        public required string ParameterUniqueReferenceId { get; set; }

        [Column("parameter_type")]
        [NotNull]
        [StringLength(250)]
        public required string ParameterType { get; set; }

        [Column("parameter_category")]
        [StringLength(250)]
        [NotNull]
        public required string ParameterCategory { get; set; }

        [Column("valid_Range_from", TypeName = "decimal(18, 3)")]
        [NotNull]
        public decimal ValidRangeFrom { get; set; }

        [Column("valid_Range_to")]
        [NotNull]
        public decimal ValidRangeTo { get; set; }
    }
}
