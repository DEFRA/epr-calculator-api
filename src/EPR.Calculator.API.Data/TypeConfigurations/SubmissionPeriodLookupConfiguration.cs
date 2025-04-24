using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class SubmissionPeriodLookupConfiguration : IEntityTypeConfiguration<SubmissionPeriodLookup>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<SubmissionPeriodLookup> builder)
        {
            builder.ToTable("submission_period_lookup");

            builder.HasKey(k => k.SubmissionPeriod);

            builder.Property(p => p.SubmissionPeriod)
                   .HasColumnName("submission_period")
                   .HasMaxLength(400)
                   .IsRequired();

            builder.Property(p => p.SubmissionPeriodDesc)
                   .HasColumnName("submission_period_desc")
                   .HasMaxLength(400)
                   .IsRequired();

            builder.Property(p => p.StartDate)
                   .HasColumnName("start_date")
                   .IsRequired();

            builder.Property(p => p.EndDate)
                   .HasColumnName("end_date")
                   .IsRequired();

            builder.Property(p => p.DaysInSubmissionPeriod)
                   .HasColumnName("days_in_submission_period")
                   .IsRequired();

            builder.Property(p => p.DaysInWholePeriod)
                   .HasColumnName("days_in_whole_period")
                   .IsRequired();

            builder.Property(p => p.ScaleupFactor)
                   .HasColumnName("scaleup_factor")
                   .HasPrecision(16, 12)
                   .IsRequired();
        }
    }
}
