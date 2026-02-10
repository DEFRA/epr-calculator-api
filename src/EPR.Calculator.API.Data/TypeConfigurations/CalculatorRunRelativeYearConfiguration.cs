using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class CalculatorRunRelativeYearConfiguration : IEntityTypeConfiguration<CalculatorRunRelativeYear>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<CalculatorRunRelativeYear> builder)
        {
            builder.ToTable("calculator_run_relative_years");

            builder.HasKey(k => k.Value);

            builder.Property(p => p.Value)
                   .HasColumnName("relative_year")
                   .ValueGeneratedNever()
                   .IsRequired();

            builder.Property(p => p.Description)
                   .HasColumnName("description");
        }
    }
}
