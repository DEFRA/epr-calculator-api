using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("calculator_run_organization_data_detail")]
    public class CalculatorRunOrganisationDataDetail
    {
        [Required]
        public int Id { get; set; }

        [Column("organisation_id")]
        public int? OrganisationId { get; set; }

        [StringLength(400)]
        [Column("subsidiary_id")]
        public string? SubsidaryId { get; set; }

        [StringLength(400)]
        [Column("organisation_name")]
        public required string OrganisationName { get; set; }

        [Column("submission_period_desc")]
        public required string? SubmissionPeriodDesc { get; set; }

        [Column("load_ts")]
        public DateTime LoadTimeStamp { get; set; }

        [Column("calculator_run_organization_data_master_id")]
        public int CalculatorRunOrganisationDataMasterId { get; set; }

        public virtual CalculatorRunOrganisationDataMaster? CalculatorRunOrganisationDataMaster { get; set; }
    }
}
