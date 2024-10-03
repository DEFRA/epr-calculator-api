using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Data.DataModels
{
    [ExcludeFromCodeCoverage]
    [Table("calculator_run_organization_data_detail")]
    public class CalculatorRunOrganisationDataDetail
    {
        [StringLength(400)]
        [Column("organisation_id")]
        public string OrganisationId { get; set; }

        [StringLength(400)]
        [Column("subsidiary_id")]
        public string SubsidaryId { get; set; }

        [StringLength(400)]
        [Column("organisation_name")]
        public string OrganisationName { get; set; }

        [Column("load_ts")]
        public DateTime LoadTimeStamp { get; set; }

        [Column("calculator_run_organization_data_master_id")]
        public int CalculatorRunOrganisationDataMasterId { get; set; }
    }
}
