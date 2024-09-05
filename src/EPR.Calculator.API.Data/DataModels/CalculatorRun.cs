using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Data.DataModels
{
    [ExcludeFromCodeCoverage]
    [Table("calculator_run")]
    public class CalculatorRun
    {
        [Column("id")]
        [Required]
        public int Id { get; set; }

        [Column("calculator_run_classification_id")]
        [Required]
        public int CalculatorRunClassificationId { get; set; }

        [Column("name")]
        [StringLength(250)]
        [Required]
        public string Name { get; set; }

        [Column("financial_year")]
        [StringLength(250)]
        [Required]
        public string Financial_Year { get; set; }

        [Column("created_by")]
        [StringLength(400)]
        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        [Column("created_at")]
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_by")]
        [StringLength(400)]
        public string? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
