using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class OrganisationDataConfiguration : IEntityTypeConfiguration<OrganisationData>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<OrganisationData> builder)
        {
            builder.ToTable("organisation_data");

            builder.HasNoKey();

            builder.Property(p => p.OrganisationId)
                   .HasColumnName("organisation_id");

            builder.Property(p => p.SubsidaryId)
                   .HasColumnName("subsidiary_id")
                   .HasMaxLength(400);

            builder.Property(p => p.OrganisationName)
                   .HasColumnName("organisation_name")
                   .HasMaxLength(400);

            builder.Property(p => p.TradingName)
                   .HasColumnName("trading_name")
                   .HasMaxLength(400);

            builder.Property(p => p.SubmissionPeriodDesc)
                   .HasColumnName("submission_period_desc");

            builder.Property(p => p.LoadTimestamp)
                   .HasColumnName("load_ts");

            builder.Property(p => p.LeaverCode)
                   .HasColumnName("leaver_code")
                   .HasMaxLength(4000);

            builder.Property(p => p.LeaverDate)
                  .HasColumnName("leaver_date")
                   .HasMaxLength(4000);

            builder.Property(p => p.JoinerDate)
                 .HasColumnName("joiner_date")
                   .HasMaxLength(4000);

            builder.Property(p => p.SubmitterOrgId)
                   .HasColumnName("submitter_org_id")
                   .HasMaxLength(4000);
        }
    }
}
