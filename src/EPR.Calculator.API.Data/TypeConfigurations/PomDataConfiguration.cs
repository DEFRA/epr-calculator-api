using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class PomDataConfiguration : IEntityTypeConfiguration<PomData>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<PomData> builder)
        {
            builder.ToTable("pom_data");

            builder.HasNoKey();

            builder.Property(p => p.OrganisationId)
                   .HasColumnName("organisation_id");

            builder.Property(p => p.SubsidaryId)
                   .HasColumnName("subsidiary_id")
                   .HasMaxLength(400);

            builder.Property(p => p.SubmissionPeriod)
                   .HasColumnName("submission_period")
                   .HasMaxLength(400);

            builder.Property(p => p.PackagingActivity)
                   .HasColumnName("packaging_activity")
                   .HasMaxLength(400);

            builder.Property(p => p.PackagingType)
                   .HasColumnName("packaging_type")
                   .HasMaxLength(400);

            builder.Property(p => p.PackagingClass)
                   .HasColumnName("packaging_class")
                   .HasMaxLength(400);

            builder.Property(p => p.PackagingMaterial)
                   .HasColumnName("packaging_material");

            builder.Property(p => p.PackagingMaterialWeight)
                   .HasColumnName("packaging_material_weight");

            builder.Property(p => p.SubmissionPeriodDesc)
                   .HasColumnName("submission_period_desc");

            builder.Property(p => p.LoadTimeStamp)
                   .HasColumnName("load_ts");

            builder.Property(p => p.SubmitterOrgId)
                   .HasColumnName("submitter_org_id");
        }
    }
}
