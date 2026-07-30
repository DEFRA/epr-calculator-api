using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class CalcResultCancelledProducersConfiguration : IEntityTypeConfiguration<CalcResultCancelledProducerEntry>
{
    public void Configure(EntityTypeBuilder<CalcResultCancelledProducerEntry> builder)
    {
        builder.ToTable("calc_result_cancelled_producer");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.CalculatorRunId).HasColumnName("calculator_run_id").IsRequired();

        builder.OwnsOne(x => x.CancelledProducer, o =>
        {
            o.ToJson("cancelled_producer");
            o.OwnsOne(x => x.LastTonnage);
            o.OwnsOne(x => x.LatestInvoice);
        });

        builder.HasIndex(x => x.CalculatorRunId);

    }
}
