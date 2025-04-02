using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("organisation_data")]
    public class OrganisationData
    {
        [Column("organisation_id")]
        public int? OrganisationId { get; set; }

        [Column("subsidiary_id")]
        [StringLength(400)]
        public string? SubsidaryId { get; set; }

        [Column("organisation_name")]
        [StringLength(400)]
        public required string OrganisationName { get; set; }

        [Column("submission_period_desc")]
        public required string SubmissionPeriodDesc { get; set; }

        [Column("load_ts")]
        public required DateTime LoadTimestamp { get; set; }
    }
}