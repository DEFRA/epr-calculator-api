using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class CountryApportionmentConfiguration : IEntityTypeConfiguration<CountryApportionment>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<CountryApportionment> builder)
        {
            builder.ToTable("country_apportionment");

            builder.Property(p => p.Id)
                   .HasColumnName("id")
                   .IsRequired();

            builder.Property(p => p.Apportionment)
                   .HasColumnName("apportionment")
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(p => p.CountryId)
                   .HasColumnName("country_id");

            builder.Property(p => p.CostTypeId)
                   .HasColumnName("cost_type_id");

            builder.Property(p => p.CalculatorRunId)
                   .HasColumnName("calculator_run_id");
        }
    }
}
