﻿using System;
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
    [Table("calculator_run_classification")]
    public class CalculatorRunClassification
    {
        [Column("id")]
        [Required]
        public int Id { get; set; }

        [Column("status")]
        [StringLength(250)]
        [Required]
        public string Status { get; set; }

        [Column("created_by")]
        [StringLength(400)]
        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        [Column("created_at")]
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<CalculatorRun> CalculatorRunDetails { get; } = new List<CalculatorRun>();
    }
}