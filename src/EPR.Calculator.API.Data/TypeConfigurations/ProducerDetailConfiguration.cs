using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class ProducerDetailConfiguration : IEntityTypeConfiguration<ProducerDetail>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<ProducerDetail> builder)
        {
            builder.ToTable("producer_detail");

            builder.Property(p => p.Id)
                   .HasColumnName("id")
                   .IsRequired();

            builder.Property(p => p.ProducerId)
                   .HasColumnName("producer_id")
                   .IsRequired();

            builder.Property(p => p.SubsidiaryId)
                   .HasColumnName("subsidiary_id")
                   .HasMaxLength(400);

            builder.Property(p => p.ProducerName)
                   .HasColumnName("producer_name")
                   .HasMaxLength(400);

            builder.Property(p => p.CalculatorRunId)
                   .HasColumnName("calculator_run_id");

            builder.HasMany(e => e.ProducerReportedMaterials)
                   .WithOne(e => e.ProducerDetail)
                   .HasForeignKey(e => e.ProducerDetailId);
        }
    }
}
