using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class ProducerDetailConfiguration : IEntityTypeConfiguration<ProducerDetail>
{
    public void Configure(EntityTypeBuilder<ProducerDetail> builder)
    {
        builder.ToTable("producer_detail");

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(p => p.ProducerId)
            .HasColumnName("producer_id")
            .IsRequired();

        builder.Property(p => p.SubsidiaryId)
            .HasColumnName("subsidiary_id")
            .HasMaxLength(400);

        builder.Property(p => p.ProducerName)
            .HasColumnName("producer_name")
            .HasMaxLength(400);

        builder.Property(p => p.TradingName)
            .HasColumnName("trading_name")
            .HasMaxLength(4000);

        builder.Property(p => p.CalculatorRunId)
            .HasColumnName("calculator_run_id");

        builder.Property(p => p.SubmitterId)
            .HasColumnName("submitter_id");

        builder.Property(p => p.ObligationStatus)
            .HasColumnName("obligation_status")
            .HasMaxLength(10);

        builder.Property(p => p.DaysObligated)
            .HasColumnName("num_days_obligated");

        builder.Property(p => p.JoinerDate)
            .HasColumnName("joiner_date")
            .HasMaxLength(50);

        builder.Property(p => p.LeaverDate)
            .HasColumnName("leaver_date")
            .HasMaxLength(50);

        builder.Property(p => p.StatusCode)
            .HasColumnName("status_code")
            .HasMaxLength(400);

        builder.HasMany(e => e.ProducerReportedMaterials)
            .WithOne(e => e.ProducerDetail)
            .HasForeignKey(e => e.ProducerDetailId);

        builder.HasMany(e => e.ProducerMaterialPackaging)
            .WithOne(e => e.ProducerDetail)
            .HasForeignKey(e => e.ProducerDetailId);
    }
}
