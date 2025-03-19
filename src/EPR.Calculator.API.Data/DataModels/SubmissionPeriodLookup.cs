using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("submission_period_lookup")]
    public class SubmissionPeriodLookup
    {
        [Column("submission_period")]
        [StringLength(400)]
        [Required]
        public required string SubmissionPeriod { get; set; }

        [Column("submission_period_desc")]
        [StringLength(400)]
        [Required]
        public required string SubmissionPeriodDesc { get; set; }

        [Column("start_date")]
        [Required]
        public required DateTime StartDate { get; set; }

        [Column("end_date")]
        [Required]
        public required DateTime EndDate { get; set; }

        [Column("days_in_submission_period")]
        [Required]
        public required int DaysInSubmissionPeriod { get; set; }

        [Column("days_in_whole_period")]
        [Required]
        public required int DaysInWholePeriod { get; set; }

        [Column("scaleup_factor")]
        [Precision(16, 12)]
        [Required]
        public required decimal ScaleupFactor { get; set; }
    }
}
