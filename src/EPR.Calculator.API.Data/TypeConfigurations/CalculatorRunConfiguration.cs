using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes.DateTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class CalculatorRunConfiguration : IEntityTypeConfiguration<CalculatorRun>
{
    public void Configure(EntityTypeBuilder<CalculatorRun> builder)
    {
        builder.ToTable("calculator_run");

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(p => p.CalculatorRunClassificationId)
            .HasColumnName("calculator_run_classification_id")
            .IsRequired();

        builder.Property(p => p.BillingRunStatus)
            .HasColumnName("billing_run_status")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(p => p.BillingRunStartedAt)
            .HasColumnName("billing_run_started_at")
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(p => p.RelativeYear)
            .HasColumnName("relative_year")
            .IsRequired();

        builder.HasOne<CalculatorRunRelativeYear>()
            .WithMany()
            .HasForeignKey(e => e.RelativeYear)
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

        builder.HasOne<CalculatorRunClassification>()
            .WithMany()
            .HasForeignKey(e => e.CalculatorRunClassificationId);

        builder.HasMany(e => e.CountryApportionments)
            .WithOne(e => e.CalculatorRun)
            .HasForeignKey(e => e.CalculatorRunId);

        builder.HasMany(e => e.ProducerDetails)
            .WithOne(e => e.CalculatorRun)
            .HasForeignKey(e => e.CalculatorRunId);

        builder.HasMany(e => e.CalculatorRunBillingFileMetadata)
            .WithOne(e => e.CalculatorRun)
            .HasForeignKey(e => e.CalculatorRunId);

        builder.HasMany(e => e.CsvFileMetadata)
            .WithOne(e => e.CalculatorRun)
            .HasForeignKey(e => e.CalculatorRunId);

        builder.HasMany(e => e.ProducerInvoicedMaterialNetTonnage)
            .WithOne(e => e.CalculatorRun)
            .HasForeignKey(e => e.CalculatorRunId);

        builder.HasMany(e => e.ProducerDesignatedRunInvoiceInstruction)
            .WithOne(e => e.CalculatorRun)
            .HasForeignKey(e => e.CalculatorRunId);

        builder.HasMany(e => e.ProducerResultFileSuggestedBillingInstruction)
            .WithOne(e => e.CalculatorRun)
            .HasForeignKey(e => e.CalculatorRunId);
    }
}
