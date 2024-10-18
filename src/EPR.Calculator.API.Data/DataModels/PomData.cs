using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Data.DataModels
{
    [ExcludeFromCodeCoverage]
    [Table("pom_data")]
    public class PomData
    {
        [Column("organisation_id")]
        [StringLength(400)]
        public string? OrganisationId { get; set; }

        [Column("subsidiary_id")]
        [StringLength(400)]
        public string? SubsidaryId { get; set; }

        [Column("submission_period")]
        [StringLength(400)]
        public required string SubmissionPeriod { get; set; }

        [Column("packaging_activity")]
        [StringLength(400)]
        public string? PackagingActivity { get; set; }

        [Column("packaging_type")]
        [StringLength(400)]
        public string? PackagingType { get; set; }

        [Column("packaging_class")]
        [StringLength(400)]
        public string? PackagingClass { get; set; }

        [Column("packaging_material")]
        [StringLength(400)]
        public string? PackagingMaterial { get; set; }

        [Column("packaging_material_weight")]
        [StringLength(400)]
        public string? PackagingMaterialWeight { get; set; }

        [Column("load_ts")]
        public required DateTime LoadTimeStamp { get; set; }
    }
}
