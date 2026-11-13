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
        builder.Property(p => p.IsTotal)
            .HasColumnName("is_total");
        builder.Property(p => p.DaysInSubmissionPeriod)
            .HasColumnName("days_in_submission_period");
        builder.Property(p => p.DaysInWholePeriod)
            .HasColumnName("days_in_whole_period");
        builder.Property(p => p.ScaleupFactor)
            .HasColumnName("scaled_factor")
            .HasPrecision(16, 12);
        builder.Property(p => p.MaterialCode)
            .HasColumnName("material_code")
            .HasMaxLength(50);
        builder.Property(p => p.ReportedHouseholdPackagingWasteTonnage)
            .HasColumnName("household_tonnage")
            .HasPrecision(18, 3);
        builder.Property(p => p.ReportedPublicBinTonnage)
            .HasColumnName("public_bin_tonnage")
            .HasPrecision(18, 3);
        builder.Property(p => p.HouseholdDrinksContainersTonnageGlass)
            .HasColumnName("hdc_tonnage")
            .HasPrecision(18, 3);
        builder.Property(p => p.TotalReportedTonnage)
            .HasColumnName("total_reported_tonnage")
            .HasPrecision(18, 3);
        builder.Property(p => p.ReportedSelfManagedConsumerWasteTonnage)
            .HasColumnName("smcw_tonnage")
            .HasPrecision(18, 3);
        builder.Property(p => p.NetReportedTonnage)
            .HasColumnName("net_reported_tonnage")
            .HasPrecision(18, 3);
        builder.Property(p => p.ScaledupReportedHouseholdPackagingWasteTonnage)
            .HasColumnName("scaled_household_tonnage")
            .HasPrecision(18, 3);
        builder.Property(p => p.ScaledupReportedPublicBinTonnage)
            .HasColumnName("scaled_public_bin_tonnage")
            .HasPrecision(18, 3);
        builder.Property(p => p.ScaledupHouseholdDrinksContainersTonnageGlass)
            .HasColumnName("scaled_hdc_tonnage")
            .HasPrecision(18, 3);
        builder.Property(p => p.ScaledupTotalReportedTonnage)
            .HasColumnName("scaled_total_reported_tonnage")
            .HasPrecision(18, 3);
        builder.Property(p => p.ScaledupReportedSelfManagedConsumerWasteTonnage)
            .HasColumnName("scaled_smcw_tonnage")
            .HasPrecision(18, 3);
        builder.Property(p => p.ScaledupNetReportedTonnage)
            .HasColumnName("scaled_net_reported_tonnage")
            .HasPrecision(18, 3);
    }
}
