﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Data.DataModels
{
    [ExcludeFromCodeCoverage]
    [Table("default_parameter_template_master")]
    public class DefaultParameterTemplateMaster
    {

        [Column("parameter_unique_ref")]
        [Key]
        [NotNull]
        [StringLength(450)]
        public string ParameterUniqueReferenceId { get; set; }


        [Column("parameter_type")]
        [NotNull]
        [StringLength(250)]
        public string ParameterType { get; set; }

        [Column("parameter_category")]
        [StringLength(250)]
        [NotNull]
        public string ParameterCategory { get; set; }

        [Column("valid_Range_from", TypeName = "decimal(18, 3)")]
        [NotNull]
        public decimal ValidRangeFrom { get; set; }

        [Column("valid_Range_to")]
        [NotNull]
        public decimal ValidRangeTo { get; set; }
    }
}
