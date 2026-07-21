using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class SelfManagedConsumerWasteConfiguration : IEntityTypeConfiguration<SelfManagedConsumerWaste>
{
    public void Configure(EntityTypeBuilder<SelfManagedConsumerWaste> builder)
    {
        builder.ToTable("calc_result_smcw");

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(p => p.CalculatorRunId)
            .HasColumnName("calculator_run_id")
            .IsRequired();

        builder.OwnsMany(p => p.MaterialTotals, omt =>
        {
            omt.ToJson("material_totals");
            omt.OwnsOne(a => a.Smcw, smcw => 
            {
                smcw.OwnsOne(x => x.ActionedSmcwTonnage);
                smcw.OwnsOne(x => x.NetTonnage);
            });
        });

        builder.Ignore(p => p.TotalByMaterial);
    }
}
