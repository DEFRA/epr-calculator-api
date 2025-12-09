using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

[ExcludeFromCodeCoverage]
public class ErrorReportConfiguration : IEntityTypeConfiguration<ErrorReport>
{
    public void Configure(EntityTypeBuilder<ErrorReport> builder)
    {
        builder.ToTable("error_report");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.ProducerId).HasColumnName("producer_id").IsRequired();
        builder.Property(e => e.SubsidiaryId).HasColumnName("subsidiary_id").HasMaxLength(400);
        builder.Property(e => e.CalculatorRunId).HasColumnName("calculator_run_id").IsRequired();
        builder.Property(e => e.LeaverCode).HasColumnName("leaver_code");
        builder.Property(e => e.ErrorCode).HasColumnName("error_code").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(400).IsRequired();

        builder.HasOne(e => e.CalculatorRun)
        .WithMany(r => r.ErrorReports)
        .HasForeignKey(e => e.CalculatorRunId);
    }
}
