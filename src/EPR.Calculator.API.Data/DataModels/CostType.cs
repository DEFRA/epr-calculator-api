using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("cost_type")]
    public class CostType
    {
        [Column("id")]
        [Required]
        public int Id { get; set; }

        [Column("code")]
        [StringLength(400)]
        public required string Code { get; set; }

        [Column("name")]
        [StringLength(400)]
        public required string Name { get; set; }

        [Column("description")]
        [StringLength(2000)]
        public string? Description { get; set; }

        public ICollection<CountryApportionment> CountryApportionments { get; } = new List<CountryApportionment>();
    }
}
