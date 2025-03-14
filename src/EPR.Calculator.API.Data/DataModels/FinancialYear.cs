
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("calculator_run_financial_years")]
    [PrimaryKey(nameof(Id))]
    public record FinancialYear
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("financial_Year")]
        [Required]
        public required string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        public override string ToString() => Name;

        //[ForeignKey("FinancialYear")]
        public ICollection<CalculatorRun> CalculatorRuns { get; }
            = new List<CalculatorRun>();

        public ICollection<DefaultParameterSettingMaster> DefaultParameterSettingMasters { get; }
            = new List<DefaultParameterSettingMaster>();

        public ICollection<LapcapDataMaster> LapcapDataMasters { get; }
            = new List<LapcapDataMaster>();
    }
}
