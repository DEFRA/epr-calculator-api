using System.Diagnostics.CodeAnalysis;
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

            builder.Property(p => p.TradingName)
                   .HasColumnName("trading_name")
                   .HasMaxLength(4000);

            builder.Property(p => p.CalculatorRunId)
                   .HasColumnName("calculator_run_id");

            builder.Property(p => p.DaysInSubmissionYear)
                   .HasColumnName("days_in_submission_year");

            builder.Property(p => p.JoinerDate)
                   .HasColumnName("joiner_date");

            builder.Property(p => p.LeaverDate)
                   .HasColumnName("leaver_date");

            builder.Property(p => p.LeaverCode)
                   .HasColumnName("leaver_code")
                   .HasMaxLength(4000);

            builder.Property(p => p.ObligatedDays)
                   .HasColumnName("obligated_days");

            builder.Property(p => p.ObligatedPercentage)
                   .HasColumnName("obligated_percentage")
                   .HasPrecision(18, 3);

            builder.HasMany(e => e.ProducerReportedMaterials)
                   .WithOne(e => e.ProducerDetail)
                   .HasForeignKey(e => e.ProducerDetailId);
        }
    }
}
