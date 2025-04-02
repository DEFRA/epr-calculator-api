using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("lapcap_data_template_master")]
    public class LapcapDataTemplateMaster
    {
        [Column("unique_ref")]
        [Required]
        [Key]
        [StringLength(400)]
        public required string UniqueReference { get; set; }

        [Column("country")]
        [StringLength(400)]
        public required string Country { get; set; }

        [Column("material")]
        [StringLength(400)]
        public required string Material { get; set; }

        [Column("total_cost_from")]
        [Precision(18, 2)]
        public decimal TotalCostFrom { get; set; }

        [Precision(18, 2)]
        [Column("total_cost_to")]
        public decimal TotalCostTo { get; set; }

        public ICollection<LapcapDataDetail> Details { get; } = new List<LapcapDataDetail>();
    }
}
