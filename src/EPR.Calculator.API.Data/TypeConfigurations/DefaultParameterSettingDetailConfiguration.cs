using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class DefaultParameterSettingDetailConfiguration : IEntityTypeConfiguration<DefaultParameterSettingDetail>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<DefaultParameterSettingDetail> builder)
        {
            builder.ToTable("default_parameter_setting_detail");

            builder.Property(p => p.DefaultParameterSettingMasterId)
                   .HasColumnName("default_parameter_setting_master_id");

            builder.Property(p => p.ParameterUniqueReferenceId)
                   .HasColumnName("parameter_unique_ref")
                   .HasMaxLength(450);

            builder.Property(p => p.ParameterValue)
                   .HasColumnName("parameter_value")
                   .HasPrecision(18, 3);
        }
    }
}
