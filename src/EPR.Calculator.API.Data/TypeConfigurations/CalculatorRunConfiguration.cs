using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.Converters;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

[ExcludeFromCodeCoverage]
public class CalculatorRunConfiguration : IEntityTypeConfiguration<CalculatorRun>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CalculatorRun> builder)
    {
        builder.ToTable("calculator_run");

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(p => p.Classification)
            .HasColumnName("classification")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.BillingRunStatus)
            .HasColumnName("billing_run_status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.BillingRunStartedAt)
            .HasColumnName("billing_run_started_at")
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(p => p.RelativeYearValue)
            .HasColumnName("relative_year")
            .IsRequired();

        builder.Ignore(p => p.RelativeYear);

        builder.HasOne<CalculatorRunRelativeYear>()
            .WithMany()
            .HasForeignKey(e => e.RelativeYearValue)
            .HasPrincipalKey(e => e.Value)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(400)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(400);

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(p => p.CalculatorRunPomDataMasterId)
            .HasColumnName("calculator_run_pom_data_master_id");

        builder.Property(p => p.CalculatorRunOrganisationDataMasterId)
            .HasColumnName("calculator_run_organization_data_master_id");

        builder.Property(p => p.LapcapDataMasterId)
            .HasColumnName("lapcap_data_master_id");

        builder.Property(p => p.DefaultParameterSettingMasterId)
            .HasColumnName("default_parameter_setting_master_id");

        builder.HasMany(e => e.CountryApportionments)
            .WithOne(e => e.CalculatorRun)
            .HasForeignKey(e => e.CalculatorRunId);

        builder.HasMany(e => e.ProducerDetails)
            .WithOne(e => e.CalculatorRun)
            .HasForeignKey(e => e.CalculatorRunId);

        builder.HasOne(e => e.BillingFileMetadata)
            .WithOne(m => m.CalculatorRun)
            .HasForeignKey<CalculatorRunBillingFileMetadata>(m => m.CalculatorRunId)
            .IsRequired();
    }
}
