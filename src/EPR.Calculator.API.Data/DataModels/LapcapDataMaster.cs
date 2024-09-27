using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("lapcap_data_master")]
    public class LapcapDataMaster
    {
        [Column("id")]
        [Required]
        public int Id { get; set; }

        [Column("projection_year")]
        [Required]
        [StringLength(50)]
        public string ProjectionYear { get; set; }

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

        public ICollection<LapcapDataDetail> Details { get; } = new List<LapcapDataDetail>();
    }
}
