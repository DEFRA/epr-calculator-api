using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class CostTypeConfiguration : IEntityTypeConfiguration<CostType>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<CostType> builder)
        {
            builder.ToTable("cost_type");

            builder.Property(p => p.Id)
                   .HasColumnName("id")
                   .IsRequired();

            builder.Property(p => p.Code)
                   .HasColumnName("code")
                   .HasMaxLength(400);

            builder.Property(p => p.Name)
                   .HasColumnName("name")
                   .HasMaxLength(400);

            builder.Property(p => p.Description)
                   .HasColumnName("description")
                   .HasMaxLength(2000);

            builder.HasMany(e => e.CountryApportionments)
                   .WithOne(e => e.CostType)
                   .HasForeignKey(e => e.CostTypeId);
        }
    }
}
