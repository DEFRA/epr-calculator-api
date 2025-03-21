using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("lapcap_data_detail")]
    public class LapcapDataDetail
    {
        [Column("id")]
        [Required]
        public int Id { get; set; }

        [Column("lapcap_data_master_id")]
        public int LapcapDataMasterId { get; set; }

        [Column("lapcap_data_template_master_unique_ref")]
        [StringLength(400)]
        public required string UniqueReference { get; set; }

        [Column("total_cost")]
        public decimal TotalCost { get; set; }

        public required LapcapDataMaster LapcapDataMaster { get; set; }

        public virtual LapcapDataTemplateMaster? LapcapDataTemplateMaster { get; set; }
    }
}
