using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class TransformScaledConfiguration : IEntityTypeConfiguration<TransformScaled>
{
    public void Configure(EntityTypeBuilder<TransformScaled> builder)
    {
        builder.ToTable("transform_scaled");

        builder.HasIndex(x => x.CalculatorRunId);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();
        builder.Property(p => p.CalculatorRunId)
            .HasColumnName("calculator_run_id")
            .IsRequired();
        builder.Property(p => p.ProducerId)
            .HasColumnName("producer_id")
            .IsRequired();
        builder.Property(p => p.SubsidiaryId)
            .HasColumnName("subsidiary_id")
            .HasMaxLength(100)
            .IsRequired(false);
        builder.Property(p => p.ProducerName)
            .HasColumnName("producer_name")
            .HasMaxLength(400)
            .IsRequired(false);
        builder.Property(p => p.TradingName)
            .HasColumnName("trading_name")
            .HasMaxLength(400)
            .IsRequired(false);
        builder.Property(p => p.SubmissionPeriodCode)
            .HasColumnName("submission_period")
            .HasMaxLength(50);
        builder.Property(p => p.Level)
            .HasColumnName("level")
            .HasMaxLength(5);
        builder.Property(p => p.IsSubTotal)
            .HasColumnName("is_subtotal");
        builder.Property(p => p.DaysInSubmissionPeriod)
            .HasColumnName("days_in_submission_period");
        builder.Property(p => p.DaysInWholePeriod)
            .HasColumnName("days_in_whole_period");
        builder.Property(p => p.ScaleupFactor)
            .HasColumnName("scaled_factor")
            .HasPrecision(16, 12);
        builder.Property(p => p.MaterialId)
            .HasColumnName("material_id");
        builder.Property(p => p.PackagingType)
            .HasColumnName("packaging_type")
            .HasMaxLength(50);
        builder.Property(p => p.Tonnage)
            .HasColumnName("tonnage")
            .HasPrecision(18, 3);
        builder.Property(p => p.ScaledTonnage)
            .HasColumnName("scaled_tonnage")
            .HasPrecision(18, 3);
    }
}
