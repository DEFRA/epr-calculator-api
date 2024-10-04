using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Data.DataModels
{
    [ExcludeFromCodeCoverage]
    [Table("calculator_run_organization_data_detail")]
    [PrimaryKey(nameof(OrganisationId), nameof(SubsidaryId))]

    public class CalculatorRunOrganisationDataDetail
    {
        [StringLength(400)]
        [Column("organisation_id")]

        public required string OrganisationId { get; set; }

        [StringLength(400)]
        [Column("subsidiary_id")]
        public required string SubsidaryId { get; set; }

        [StringLength(400)]
        [Column("organisation_name")]
        public required string OrganisationName { get; set; }

        [Column("load_ts")]
        public DateTime LoadTimeStamp { get; set; }

        [Column("calculator_run_organization_data_master_id")]
        public int CalculatorRunOrganisationDataMasterId { get; set; }

        public virtual CalculatorRunOrganisationDataMaster CalculatorRunOrganisationDataMaster { get; set; }
    }
}
