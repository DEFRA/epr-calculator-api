using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class CalculatorRunCsvFileMetadataConfiguration : IEntityTypeConfiguration<CalculatorRunCsvFileMetadata>
    {
        /// <inheritdoc />
        // NOSONAR
        public void Configure(EntityTypeBuilder<CalculatorRunCsvFileMetadata> builder)
        {
            builder.ToTable("calculator_run_csvfile_metadata");

            builder.Property(p => p.Id)
                   .HasColumnName("id")
                   .IsRequired();

            builder.Property(p => p.FileName)
                   .HasColumnName("filename")
                   .HasMaxLength(500)
                   .IsRequired();

            builder.Property(p => p.BlobUri)
                   .HasColumnName("blob_uri")
                   .HasMaxLength(2000);

            builder.Property(p => p.CalculatorRunId)
                   .HasColumnName("calculator_run_id")
                   .IsRequired(true);
        }
    }
}
