using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class CalcResultParameterOtherCostConfiguration : IEntityTypeConfiguration<CalcResultParameterOtherCostEntry>
{
    public void Configure(EntityTypeBuilder<CalcResultParameterOtherCostEntry> builder)
    {
        builder.ToTable("calc_result_parameter_other_cost");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.CalculatorRunId).HasColumnName("calculator_run_id").IsRequired();
        
        builder.OwnsOne(x => x.ParameterOtherCost, o =>
        {
            o.ToJson("parameter_other_cost");
            o.OwnsOne(x => x.SaOperatingCost, y => y.Ignore(p => p.Total));
            o.OwnsOne(x => x.LaDataPrepCharge, y => y.Ignore(p => p.Total));
            o.OwnsOne(x => x.CountryApportionment, y => y.Ignore(p => p.Total));
            o.OwnsOne(x => x.SchemeSetupCost, y => y.Ignore(p => p.Total));
            o.OwnsOne(x => x.MaterialityIncrease);
            o.OwnsOne(x => x.MaterialityDecrease);
            o.OwnsOne(x => x.TonnageChangeIncrease);
            o.OwnsOne(x => x.TonnageChangeDecrease);
        });
    }
}
