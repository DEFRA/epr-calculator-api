using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class TransformProjectedH2Configuration : IEntityTypeConfiguration<TransformProjectedH2>
{
    public void Configure(EntityTypeBuilder<TransformProjectedH2> builder)
    {
        builder.ToTable("transform_projected_h2");

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
        builder.Property(p => p.MaterialCode)
            .HasColumnName("material_code")
            .HasMaxLength(50);
        builder.Property(p => p.SubmissionPeriodCode)
            .HasColumnName("submission_period")
            .HasMaxLength(50);
        builder.Property(p => p.Level)
            .HasColumnName("level")
            .HasMaxLength(5);
        builder.Property(p => p.HouseholdTonnage)
            .HasColumnName("household_tonnage")
            .HasPrecision(18, 3);
        builder.Property(p => p.HouseholdTonnageRed)
            .HasColumnName("household_tonnage_red")
            .HasPrecision(18, 3);
        builder.Property(p => p.HouseholdTonnageAmber)
            .HasColumnName("household_tonnage_amber")
            .HasPrecision(18, 3);
        builder.Property(p => p.HouseholdTonnageGreen)
            .HasColumnName("household_tonnage_green")
            .HasPrecision(18, 3);
        builder.Property(p => p.HouseholdTonnageRedMedical)
            .HasColumnName("household_tonnage_red_medical")
            .HasPrecision(18, 3);
        builder.Property(p => p.HouseholdTonnageAmberMedical)
            .HasColumnName("household_tonnage_amber_medical")
            .HasPrecision(18, 3);
        builder.Property(p => p.HouseholdTonnageGreenMedical)
            .HasColumnName("household_tonnage_green_medical")
            .HasPrecision(18, 3);
        builder.Property(p => p.PublicBinTonnage)
            .HasColumnName("public_bin_tonnage")
            .HasPrecision(18, 3);
        builder.Property(p => p.PublicBinTonnageRed)
            .HasColumnName("public_bin_tonnage_red")
            .HasPrecision(18, 3);
        builder.Property(p => p.PublicBinTonnageAmber)
            .HasColumnName("public_bin_tonnage_amber")
            .HasPrecision(18, 3);
        builder.Property(p => p.PublicBinTonnageGreen)
            .HasColumnName("public_bin_tonnage_green")
            .HasPrecision(18, 3);
        builder.Property(p => p.PublicBinTonnageRedMedical)
            .HasColumnName("public_bin_tonnage_red_medical")
            .HasPrecision(18, 3);
        builder.Property(p => p.PublicBinTonnageAmberMedical)
            .HasColumnName("public_bin_tonnage_amber_medical")
            .HasPrecision(18, 3);
        builder.Property(p => p.PublicBinTonnageGreenMedical)
            .HasColumnName("public_bin_tonnage_green_medical")
            .HasPrecision(18, 3);
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
        builder.Property(p => p.HouseholdTonnageWithoutRAM)
            .HasColumnName("household_tonnage_without_ram")
            .HasPrecision(18, 3);
        builder.Property(p => p.PublicBinTonnageWithoutRAM)
            .HasColumnName("public_bin_tonnage_without_ram")
            .HasPrecision(18, 3);
        builder.Property(p => p.HDCTonnageWithoutRAM)
            .HasColumnName("hdc_tonnage_without_ram")
            .HasPrecision(18, 3)
            .IsRequired(false);
        builder.Property(p => p.ProjectedHouseholdTonnage)
            .HasColumnName("projected_household_tonnage")
            .HasPrecision(18, 3);
        builder.Property(p => p.ProjectedHouseholdTonnageRed)
            .HasColumnName("projected_household_tonnage_red")
            .HasPrecision(18, 3);
        builder.Property(p => p.ProjectedHouseholdTonnageAmber)
            .HasColumnName("projected_household_tonnage_amber")
            .HasPrecision(18, 3);
        builder.Property(p => p.ProjectedHouseholdTonnageGreen)
            .HasColumnName("projected_household_tonnage_green")
            .HasPrecision(18, 3);
        builder.Property(p => p.ProjectedHouseholdTonnageRedMedical)
            .HasColumnName("projected_household_tonnage_red_medical")
            .HasPrecision(18, 3);
        builder.Property(p => p.ProjectedHouseholdTonnageAmberMedical)
            .HasColumnName("projected_household_tonnage_amber_medical")
            .HasPrecision(18, 3);
        builder.Property(p => p.ProjectedHouseholdTonnageGreenMedical)
            .HasColumnName("projected_household_tonnage_green_medical")
            .HasPrecision(18, 3);
        builder.Property(p => p.ProjectedPublicBinTonnage)
            .HasColumnName("projected_public_bin_tonnage")
            .HasPrecision(18, 3);
        builder.Property(p => p.ProjectedPublicBinTonnageRed)
            .HasColumnName("projected_public_bin_tonnage_red")
            .HasPrecision(18, 3);
        builder.Property(p => p.ProjectedPublicBinTonnageAmber)
            .HasColumnName("projected_public_bin_tonnage_amber")
            .HasPrecision(18, 3);
        builder.Property(p => p.ProjectedPublicBinTonnageGreen)
            .HasColumnName("projected_public_bin_tonnage_green")
            .HasPrecision(18, 3);
        builder.Property(p => p.ProjectedPublicBinTonnageRedMedical)
            .HasColumnName("projected_public_bin_tonnage_red_medical")
            .HasPrecision(18, 3);
        builder.Property(p => p.ProjectedPublicBinTonnageAmberMedical)
            .HasColumnName("projected_public_bin_tonnage_amber_medical")
            .HasPrecision(18, 3);
        builder.Property(p => p.ProjectedPublicBinTonnageGreenMedical)
            .HasColumnName("projected_public_bin_tonnage_green_medical")
            .HasPrecision(18, 3);
        builder.Property(p => p.ProjectedHDCTonnage)
            .HasColumnName("projected_hdc_tonnage")
            .HasPrecision(18, 3)
            .IsRequired(false);
        builder.Property(p => p.ProjectedHDCTonnageRed)
            .HasColumnName("projected_hdc_tonnage_red")
            .HasPrecision(18, 3)
            .IsRequired(false);
        builder.Property(p => p.ProjectedHDCTonnageAmber)
            .HasColumnName("projected_hdc_tonnage_amber")
            .HasPrecision(18, 3)
            .IsRequired(false);
        builder.Property(p => p.ProjectedHDCTonnageGreen)
            .HasColumnName("projected_hdc_tonnage_green")
            .HasPrecision(18, 3)
            .IsRequired(false);
        builder.Property(p => p.ProjectedHDCTonnageRedMedical)
            .HasColumnName("projected_hdc_tonnage_red_medical")
            .HasPrecision(18, 3)
            .IsRequired(false);
        builder.Property(p => p.ProjectedHDCTonnageAmberMedical)
            .HasColumnName("projected_hdc_tonnage_amber_medical")
            .HasPrecision(18, 3)
            .IsRequired(false);
        builder.Property(p => p.ProjectedHDCTonnageGreenMedical)
            .HasColumnName("projected_hdc_tonnage_green_medical")
            .HasPrecision(18, 3)
            .IsRequired(false);
    }
}
