using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class DefaultParameterTemplateMasterConfiguration : IEntityTypeConfiguration<DefaultParameterTemplateMaster>
    {
        /// <inheritdoc />
        // NOSONAR
        public void Configure(EntityTypeBuilder<DefaultParameterTemplateMaster> builder)
        {
            builder.ToTable("default_parameter_template_master");

            builder.HasKey(k => k.ParameterUniqueReferenceId);

            builder.Property(p => p.ParameterUniqueReferenceId)
                   .HasColumnName("parameter_unique_ref")
                   .HasMaxLength(450)
                   .IsRequired();

            builder.Property(p => p.ParameterType)
                   .HasColumnName("parameter_type")
                   .HasMaxLength(250)
                   .IsRequired();

            builder.Property(p => p.ParameterCategory)
                   .HasColumnName("parameter_category")
                   .HasMaxLength(250)
                   .IsRequired();

            builder.Property(p => p.ValidRangeFrom)
                   .HasColumnName("valid_Range_from")
                   .HasPrecision(18, 3)
                   .IsRequired();

            builder.Property(p => p.ValidRangeTo)
                   .HasColumnName("valid_Range_to")
                   .HasPrecision(18, 3)
                   .IsRequired();
        }
    }
}
