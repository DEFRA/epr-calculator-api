using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

[ExcludeFromCodeCoverage]
public class ErrorTypeConfiguration : IEntityTypeConfiguration<ErrorType>
{
    public void Configure(EntityTypeBuilder<ErrorType> builder)
    {
        builder.ToTable("error_type");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(250).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description");
    }
}
