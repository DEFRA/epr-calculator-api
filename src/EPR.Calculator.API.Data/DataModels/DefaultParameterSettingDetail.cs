using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("default_parameter_setting_detail")]
    public class DefaultParameterSettingDetail
    {
        public int Id { get; set; }

        [Column("default_parameter_setting_master_id")]
        public int DefaultParameterSettingMasterId { get; set; }

        public DefaultParameterSettingMaster? DefaultParameterSettingMaster { get; set; }

        [Column("parameter_unique_ref")]
        [StringLength(450)]
        public required string ParameterUniqueReferenceId { get; set; }

        public DefaultParameterTemplateMaster? ParameterUniqueReference { get; set; }

        [Column("parameter_value")]
        [Precision(18, 3)]
        public decimal ParameterValue { get; set; }
    }
}
