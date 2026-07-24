using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class CalcResultOnePlusFourApportionmentConfiguration : IEntityTypeConfiguration<CalcResultOnePlusFourApportionmentEntry>
{
    public void Configure(EntityTypeBuilder<CalcResultOnePlusFourApportionmentEntry> builder)
    {
        builder.ToTable("calc_result_one_plus_four_apportionment");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.CalculatorRunId).HasColumnName("calculator_run_id").IsRequired();

        builder.OwnsOne(x => x.OnePlusFourApportionment, o =>
        {
            o.ToJson("one_plus_four_apppointment");
            o.OwnsOne(x => x.LaDisposalCost, a => a.Ignore(p => p.Total));
            o.OwnsOne(x => x.LADataPrepCharge, a => a.Ignore(p => p.Total));
            o.Ignore(x => x.TotalOnePlusFour);
            o.Ignore(x => x.OnePlusFourApportionment);
        });
    }
}
