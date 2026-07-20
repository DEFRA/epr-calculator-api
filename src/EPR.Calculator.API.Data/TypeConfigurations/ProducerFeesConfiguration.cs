using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class ProducerFeesConfiguration : IEntityTypeConfiguration<ProducerFees>
{
    public void Configure(EntityTypeBuilder<ProducerFees> builder)
    {
        builder.ToTable("calc_result_producer_fees");

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(p => p.CalculatorRunId)
            .HasColumnName("calculator_run_id")
            .IsRequired();

        builder.OwnsOne(p => p.Total, total =>
        {
            total.ToJson("total");
            ProducerFeeDetailConfiguration.ConfigureFeeDetail(total);
        });
    }
}
