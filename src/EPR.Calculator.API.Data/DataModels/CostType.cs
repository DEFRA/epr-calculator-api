﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Data.DataModels
{
    public class CostType
    {
        [ExcludeFromCodeCoverage]
        [Table("cost_type")]
        public class Material
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
}
