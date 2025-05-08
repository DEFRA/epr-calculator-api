using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class CalculatorRunBillingFileMetadataConfiguration : IEntityTypeConfiguration<CalculatorRunBillingFileMetadata>
    {
        public void Configure(EntityTypeBuilder<CalculatorRunBillingFileMetadata> builder)
        {
            builder.ToTable("calculator_run_billing_file_metadata");

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(p => p.BillingCsvFileName)
                .HasColumnName("billing_csv_filename")
                .HasMaxLength(400);

            builder.Property(p => p.BillingJsonFileName)
                .HasColumnName("billing_json_filename")
                .HasMaxLength(400);

            builder.Property(p => p.BillingFileCreatedDate)
                .HasColumnName("billing_file_created_date")
                .IsRequired();

            builder.Property(p => p.BillingFileCreatedBy)
                .HasColumnName("billing_file_created_by")
                .HasMaxLength(400)
                .IsRequired();

            builder.Property(p => p.BillingFileAuthorisedDate)
                .HasColumnName("billing_file_authorised_date");

            builder.Property(p => p.BillingFileAuthorisedBy)
                .HasColumnName("billing_file_authorised_by")
                .HasMaxLength(400);

            builder.Property(p => p.CalculatorRunId)
                .HasColumnName("calculator_run_id")
                .IsRequired();
        }
    }
}
