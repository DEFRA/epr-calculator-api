using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("country_apportionment")]
    public class CountryApportionment
    {
        [Column("id")]
        [Required]
        public int Id { get; set; }

        [Column("apportionment")]
        [Required]
        [Precision(18, 2)]
        public decimal Apportionment { get; set; }

        [Column("country_id")]
        public required int CountryId { get; set; }

        [Column("cost_type_id")]
        public required int CostTypeId { get; set; }

        [Column("calculator_run_id")]
        public required int CalculatorRunId { get; set; }

        public virtual Country? Country { get; set; }

        public virtual CostType? CostType { get; set; }

        public virtual CalculatorRun? CalculatorRun { get; set; }
    }
}