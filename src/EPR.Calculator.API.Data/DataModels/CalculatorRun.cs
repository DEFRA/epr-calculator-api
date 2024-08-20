using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.API.Data.DataModels
{
    [ExcludeFromCodeCoverage]
    [Table("calculator_run")]
    public class CalculatorRun
    {
        public int Id { get; set; }

        [Column("run_classification_status_id")]
        [Required]
        public int RunClassificationStatusId { get; set; }

        [Column("name")]
        [Required]
        public string Name { get; set; }

        [Column("created_by")]
        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        [Column("created_at")]
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_by")]
        public string UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public CalculatorRunClassification calculatorRunClassification { get; set; }
    }
}
