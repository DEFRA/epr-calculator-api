using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Data.DataModels
{
    [ExcludeFromCodeCoverage]
    [Table("organization_data")]
    public class OrganisationData
    {
        [Column("organisation_id")]
        [StringLength(400)]
        public required string OrganisationId { get; set; }

        [Column("subsidiary_id")]
        [StringLength(400)]
        public required string SubsidaryId { get; set; }

        [Column("organisation_name")]
        [StringLength(400)]
        public required string OrganisationName{ get; set; }

        [Column("load_ts")]
        public required DateTime LoadTimestamp { get; set; }
    }
}
