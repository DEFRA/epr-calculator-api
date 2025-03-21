using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("producer_reported_material")]
    public class ProducerReportedMaterial
    {
        [Column("id")]
        [Required]
        public int Id { get; set; }

        [Column("material_id")]
        [Required]
        public int MaterialId { get; set; }

        [Column("producer_detail_id")]
        [Required]
        public int ProducerDetailId { get; set; }

        [Column("packaging_type")]
        [StringLength(400)]
        public required string PackagingType { get; set; }

        [Column("packaging_tonnage")]
        [Precision(18, 3)]
        public decimal PackagingTonnage { get; set; }

        public virtual ProducerDetail? ProducerDetail { get; set; }

        public virtual Material? Material { get; set; }
    }
}