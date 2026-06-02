using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class CalculatorRunClassificationConfiguration : IEntityTypeConfiguration<CalculatorRunClassification>
{
    public void Configure(EntityTypeBuilder<CalculatorRunClassification> builder)
    {
        builder.ToTable("calculator_run_classification");

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(400)
            .IsRequired();
    }
}
