using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class CalculatorRunClassificationConfiguration : IEntityTypeConfiguration<CalculatorRunClassification>
{
    public void Configure(EntityTypeBuilder<CalculatorRunClassification> builder)
    {
        builder.ToTable("calculator_run_classification");

        // Ids come from RunClassification, so identity is redundant. It is kept because the existing
        // column is an IDENTITY primary key referenced by calculator_run, and SQL Server cannot drop
        // the identity property without rebuilding the table. Declaring it keeps the model honest
        // about what the database actually holds.
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired()
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

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
