using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class ModulationResultConfiguration : IEntityTypeConfiguration<ModulationResult>
{
    public void Configure(EntityTypeBuilder<ModulationResult> builder)
    {
        builder.ToTable("calc_result_modulation");

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(p => p.CalculatorRunId)
            .HasColumnName("calculator_run_id")
            .IsRequired();

        builder.Property(p => p.GreenFactor)
            .HasColumnName("green_factor")
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(p => p.RedFactor)
            .HasColumnName("red_factor")
            .HasPrecision(6, 3)
            .IsRequired();

        builder.OwnsMany(p => p.MaterialModulations, mods =>
        {
            mods.ToJson("material_modulations");
            mods.OwnsOne(x => x.MaterialDetail);
            mods.OwnsOne(x => x.ModulationDetail);
        });

        builder.Ignore(p => p.ModulationByMaterial);
    }
}