using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class CalculatorRunFinancialYearConfiguration : IEntityTypeConfiguration<CalculatorRunFinancialYear>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<CalculatorRunFinancialYear> builder)
        {
            builder.ToTable("calculator_run_financial_years");

            builder.HasKey(k => k.Name);

            builder.Property(p => p.Name)
                   .HasColumnName("financial_Year")
                   .IsRequired();

            builder.Property(p => p.Description)
                   .HasColumnName("description");

            builder.HasMany(e => e.CalculatorRuns)
                   .WithOne(e => e.Financial_Year)
                   .HasForeignKey(e => e.FinancialYearId)
                   .HasPrincipalKey(e => e.Name);

            builder.HasMany(e => e.DefaultParameterSettingMasters)
                .WithOne(e => e.ParameterYear)
                .HasForeignKey(e => e.ParameterYearId)
                .HasPrincipalKey(e => e.Name);

            builder
                .HasMany(e => e.LapcapDataMasters)
                .WithOne(e => e.ProjectionYear)
                .HasForeignKey(e => e.ProjectionYearId)
                .HasPrincipalKey(e => e.Name);
        }
    }
}
