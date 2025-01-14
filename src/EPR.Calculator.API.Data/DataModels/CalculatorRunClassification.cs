using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("calculator_run_classification")]
    public class CalculatorRunClassification
    {
        [Column("id")]
        [Required]
        public int Id { get; set; }

        [Column("status")]
        [StringLength(250)]
        [Required]
        public required string Status { get; set; }

        [Column("created_by")]
        [StringLength(400)]
        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public ICollection<CalculatorRun> CalculatorRunDetails { get; } = new List<CalculatorRun>();
    }
}
