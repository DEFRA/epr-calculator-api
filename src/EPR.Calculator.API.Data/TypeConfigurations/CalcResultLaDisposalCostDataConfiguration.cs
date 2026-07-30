using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class CalcResultLaDisposalCostDataConfiguration : IEntityTypeConfiguration<CalcResultLaDisposalCostDataEntry>
{
    public void Configure(EntityTypeBuilder<CalcResultLaDisposalCostDataEntry> builder)
    {
        builder.ToTable("calc_result_la_disposal_cost");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.CalculatorRunId).HasColumnName("calculator_run_id").IsRequired();

        builder.OwnsOne(x => x.LaDisposalCost, o =>
        {
            o.ToJson("la_disposal_cost");

            o.OwnsMany(x => x.MaterialCosts, a =>
            {
                a.Property(x => x.MaterialCode);
                a.OwnsOne(x => x.Detail, c => c.OwnsOne(x => x.Cost, d => d.Ignore(p => p.Total)));
            });

            o.Ignore(x => x.Total);
        });

    }
}
