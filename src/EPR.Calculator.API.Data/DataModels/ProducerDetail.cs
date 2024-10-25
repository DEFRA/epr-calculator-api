using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Data.DataModels
{
    [ExcludeFromCodeCoverage]
    [Table("producer_detail")]
    public class ProducerDetail
    {
        [Column("id")]
        [Required]
        public int Id { get; set; }

        [Column("producer_id")]
        [Required]
        public int ProducerId { get; set; }

        [Column("subsidiary_id")]
        public int? SubsidiaryId { get; set; }

        [Column("producer_name")]
        [StringLength(400)]
        public string? ProducerName { get; set; }

        [Column("calculator_run_id")]
        public required int CalculatorRunId { get; set; }

        //public ICollection<ProducerReportedMaterial> ProducerReportedMaterials { get; } = new List<ProducerReportedMaterial>();

       // public required virtual CalculatorRun CalculatorRun { get; set; }
    }
}