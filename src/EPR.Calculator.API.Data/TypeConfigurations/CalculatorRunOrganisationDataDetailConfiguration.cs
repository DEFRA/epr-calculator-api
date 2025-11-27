using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class CalculatorRunOrganisationDataDetailConfiguration : IEntityTypeConfiguration<CalculatorRunOrganisationDataDetail>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<CalculatorRunOrganisationDataDetail> builder)
        {
            builder.ToTable("calculator_run_organization_data_detail");

            builder.Property(p => p.Id)
                   .IsRequired();

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

            builder.Property(p => p.LoadTimeStamp)
                   .HasColumnName("load_ts");

            builder.Property(p => p.CalculatorRunOrganisationDataMasterId)
                   .HasColumnName("calculator_run_organization_data_master_id");

            builder.Property(p => p.ObligationStatus)
                   .HasColumnName("obligation_status")
                   .HasMaxLength(10);

            builder.Property(p => p.SubmitterId)
                   .HasColumnName("submitter_id");

            builder.HasIndex(e => new { e.OrganisationId })
                   .HasDatabaseName("IX_index_calculator_run_organization_data_detail_OrgId")
                   .IsClustered(false);
        }
    }
}
