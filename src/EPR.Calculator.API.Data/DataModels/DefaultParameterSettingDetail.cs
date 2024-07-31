using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("default_parameter_setting_detail")]
    public class DefaultParameterSettingDetail
    {
        public int Id { get; set; }

        [Column("default_parameter_setting_master_id")]
        public int DefaultParameterSettingMasterId { get; set; }

        public DefaultParameterSettingMaster DefaultParameterSettingMaster { get; set; }

        [Column("parameter_unique_ref")]
        [StringLength(450)]
        public string ParameterUniqueReferenceId { get; set; }

        public DefaultParameterTemplateMaster ParameterUniqueReference {  get; set; }

        [Column("parameter_value")]
        [Precision(18, 2)]
        public decimal ParameterValue { get; set; }
    }
}
