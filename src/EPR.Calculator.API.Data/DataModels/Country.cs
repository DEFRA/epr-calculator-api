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
    [Table("country")]
    public class Country
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
        [StringLength(400)]
        public string? Description { get; set; }
    }
}
