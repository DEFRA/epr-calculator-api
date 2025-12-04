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

            builder.Property(p => p.SubsidiaryId)
                   .HasColumnName("subsidiary_id")
                   .HasMaxLength(400);

            builder.Property(p => p.OrganisationName)
                   .HasColumnName("organisation_name")
                   .HasMaxLength(400);

            builder.Property(p => p.TradingName)
                   .HasColumnName("trading_name")
                   .HasMaxLength(400);

            builder.Property(p => p.LoadTimestamp)
                   .HasColumnName("load_ts");

            builder.Property(p => p.ObligationStatus)
                   .HasColumnName("obligation_status")
                   .HasMaxLength(10);

            builder.Property(p => p.SubmitterId)
                     .HasColumnName("submitter_id");

            builder.Property(p => p.StatusCode)
                     .HasColumnName("status_code");

            builder.Property(p => p.DaysObligated)
                     .HasColumnName("num_days_obligated");

            builder.Property(p => p.ErrorCode)
                     .HasColumnName("error_code");
        }
    }
}
