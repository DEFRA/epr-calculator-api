using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class TransformPartialConfiguration : IEntityTypeConfiguration<TransformPartial>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<TransformPartial> builder)
        {
              builder.ToTable("transform_partial");

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
              builder.Property(p => p.Level)
                     .HasColumnName("level")
                     .HasMaxLength(5);
              builder.Property(p => p.SubmissionYear)
                     .HasColumnName("submission_year");
              builder.Property(p => p.DaysInSubmissionYear)
                     .HasColumnName("days_in_submission_year");
              builder.Property(p => p.JoiningDate)
                     .HasColumnName("joining_date")
                     .HasMaxLength(50)
                     .IsRequired(false);
              builder.Property(p => p.DaysObligated)
                     .HasColumnName("days_obligated")
                     .IsRequired(false);
              builder.Property(p => p.ObligatedFactor)
                     .HasColumnName("obligated_factor")
                     .HasPrecision(16, 12);
              builder.Property(p => p.MaterialCode)
                     .HasColumnName("material_code")
                     .HasMaxLength(50);
              builder.Property(p => p.HouseholdTonnage)
                     .HasColumnName("household_tonnage")
                     .HasPrecision(18, 3);
              builder.Property(p => p.HouseholdTonnageRed)
                     .HasColumnName("household_tonnage_red")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.HouseholdTonnageAmber)
                     .HasColumnName("household_tonnage_amber")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.HouseholdTonnageGreen)
                     .HasColumnName("household_tonnage_green")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.HouseholdTonnageRedMedical)
                     .HasColumnName("household_tonnage_red_medical")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.HouseholdTonnageAmberMedical)
                     .HasColumnName("household_tonnage_amber_medical")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.HouseholdTonnageGreenMedical)
                     .HasColumnName("household_tonnage_green_medical")
                     .HasPrecision(18, 3)
                     .IsRequired(false);       
              builder.Property(p => p.PublicBinTonnage)
                     .HasColumnName("public_bin_tonnage")
                     .HasPrecision(18, 3);       
              builder.Property(p => p.PublicBinTonnageRed)
                     .HasColumnName("public_bin_tonnage_red")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.PublicBinTonnageAmber)
                     .HasColumnName("public_bin_tonnage_amber")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.PublicBinTonnageGreen)
                     .HasColumnName("public_bin_tonnage_green")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.PublicBinTonnageRedMedical)
                     .HasColumnName("public_bin_tonnage_red_medical")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.PublicBinTonnageAmberMedical)
                     .HasColumnName("public_bin_tonnage_amber_medical")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.PublicBinTonnageGreenMedical)
                     .HasColumnName("public_bin_tonnage_green_medical")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.HDCTonnage)
                     .HasColumnName("hdc_tonnage")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.HDCTonnageRed)
                     .HasColumnName("hdc_tonnage_red")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.HDCTonnageAmber)
                     .HasColumnName("hdc_tonnage_amber")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.HDCTonnageGreen)
                     .HasColumnName("hdc_tonnage_green")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.HDCTonnageRedMedical)
                     .HasColumnName("hdc_tonnage_red_medical")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.HDCTonnageAmberMedical)
                     .HasColumnName("hdc_tonnage_amber_medical")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.HDCTonnageGreenMedical)
                     .HasColumnName("hdc_tonnage_green_medical")
                     .HasPrecision(18, 3)
                     .IsRequired(false);
              builder.Property(p => p.SMCWTonnage)
                     .HasColumnName("smcw_tonnage")
                     .HasPrecision(18, 3);
        }
    }
}
