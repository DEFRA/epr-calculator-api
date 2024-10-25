using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.Marshalling;

namespace EPR.Calculator.API.Data.DataModels
{
    [ExcludeFromCodeCoverage]
    [Table("country_apportionment")]
    public class CountryApportionment
    {
        [Column("id")]
        [Required]
        public int Id { get; set; }

        [Column("apportionment")]
        [Required]
        public decimal Apportionment { get; set; }

        [Column("country_id")]
        public required int CountryId { get; set; }

        [Column("cost_type_id")]
        public required int CostTypeId { get; set; }

        [Column("calculator_run_id")]
        public required int CalculatorRunId { get; set; }

        public required virtual Country Country { get; set; }

        public required virtual CostType CostType { get; set; }

        public required virtual CalculatorRun CalculatorRun { get; set; }
    }
}