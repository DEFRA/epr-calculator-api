using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Data.DataModels
{
    [ExcludeFromCodeCoverage]
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
        [StringLength(400)]
        public required string PackagingTonnage { get; set; }

        //public required virtual ProducerDetail ProducerDetail { get; set; }

        //public required virtual Material Material { get; set; }
    }
}