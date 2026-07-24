using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class CalcResultCommsCostConfiguration : IEntityTypeConfiguration<CalcResultCommsCostEntry>
{
    public void Configure(EntityTypeBuilder<CalcResultCommsCostEntry> builder)
    {
        builder.ToTable("calc_result_comms_cost");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.CalculatorRunId).HasColumnName("calculator_run_id").IsRequired();

        builder.OwnsOne(x => x.CommsCost, o =>
        {
            o.ToJson("comms_cost");

            o.OwnsOne(x => x.OnePlusFourApportionment, a => a.Ignore(p => p.Total));

            o.OwnsMany(x => x.MaterialCosts, a =>
            {
                a.Property(x => x.MaterialCode);
                a.OwnsOne(x => x.CommsCost, c => c.OwnsOne(x => x.Cost, d => d.Ignore(p => p.Total)));
            });

            o.OwnsOne(x => x.CommsCostUkWide, a => a.Ignore(p => p.Total));
            o.OwnsOne(x => x.CommsCostByCountry, a => a.Ignore(p => p.Total));

            o.Ignore(x => x.Total);
        });
    }
}
