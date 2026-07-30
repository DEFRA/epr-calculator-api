using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class CalcResultLateReportingTonnageConfiguration : IEntityTypeConfiguration<CalcResultLateReportingTonnageEntry>
{
    public void Configure(EntityTypeBuilder<CalcResultLateReportingTonnageEntry> builder)
    {
        builder.ToTable("calc_result_late_reporting_tonnage");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.CalculatorRunId).HasColumnName("calculator_run_id").IsRequired();

        builder.OwnsOne(x => x.LateReportingTonnage, o =>
        {
            o.ToJson("late_reporting_tonnage");

            o.OwnsMany(x => x.MaterialTonnages, a =>
            {
                a.Property(x => x.MaterialCode);
                a.OwnsOne(x => x.Detail);
            });

            o.Ignore(x => x.Total);
        });
    }
}
