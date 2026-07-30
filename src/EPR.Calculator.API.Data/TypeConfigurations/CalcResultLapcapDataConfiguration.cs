using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class CalcResultLapcapDataConfiguration : IEntityTypeConfiguration<CalcResultLapcapDataEntry>
{
    public void Configure(EntityTypeBuilder<CalcResultLapcapDataEntry> builder)
    {
        builder.ToTable("calc_result_lapcap_data");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.CalculatorRunId).HasColumnName("calculator_run_id").IsRequired();

        builder.OwnsOne(x => x.LapcapData, o =>
        {
            o.ToJson("lapcap");

            o.OwnsMany(x => x.MaterialCosts, a =>
            {
                a.Property(x => x.MaterialCode);
                a.OwnsOne(x => x.Cost, c => c.Ignore(p => p.Total));
            });

            o.Ignore(x => x.Total);
            o.Ignore(x => x.CountryApportionment);
        });
    }
}
