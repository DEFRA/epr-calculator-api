
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("calculator_run_financial_years")]
    [PrimaryKey(nameof(Id), nameof(Name))]
    public record FinancialYear
    {
        [Column("id")]
        public int Id { get; }

        [Column("financial_Year")]
        [Required]
        public required string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        public override string ToString() => Name;

        //public override bool Equals(object? obj) => Equals(obj as FinancialYear);

        //public bool Equals(FinancialYear? other) =>
        //    other is not null
        //    && this.Name == other.Name;

        //public override int GetHashCode()
        //{
        //    return this.Name.GetHashCode();
        //}

        //public static implicit operator string(FinancialYear financialYear) => financialYear.Name;

        //public static implicit operator FinancialYear(string value) => new FinancialYear { Name  = value };
    }
}
