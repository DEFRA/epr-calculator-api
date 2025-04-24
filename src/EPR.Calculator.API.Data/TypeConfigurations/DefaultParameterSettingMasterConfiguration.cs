using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class DefaultParameterSettingMasterConfiguration : IEntityTypeConfiguration<DefaultParameterSettingMaster>
    {
        /// <inheritdoc />
        // NOSONAR
        public void Configure(EntityTypeBuilder<DefaultParameterSettingMaster> builder)
        {
            builder.ToTable("default_parameter_setting_master");

            builder.Property(p => p.ParameterYearId)
                   .HasColumnName("parameter_year")
                   .IsRequired();

            builder.Property(p => p.EffectiveFrom)
                   .HasColumnName("effective_from")
                   .IsRequired();

            builder.Property(p => p.EffectiveTo)
                   .HasColumnName("effective_to");

            builder.Property(p => p.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(400)
                   .IsRequired();

            builder.Property(p => p.CreatedAt)
                   .HasColumnName("created_at")
                   .IsRequired();

            builder.Property(p => p.ParameterFileName)
                   .HasColumnName("parameter_filename")
                   .HasMaxLength(256)
                   .IsRequired();

            builder.HasMany(e => e.Details)
                   .WithOne(e => e.DefaultParameterSettingMaster)
                   .HasForeignKey(e => e.DefaultParameterSettingMasterId)
                   .IsRequired(true);

            builder.HasMany(e => e.RunDetails)
                   .WithOne(e => e.DefaultParameterSettingMaster)
                   .HasForeignKey(e => e.DefaultParameterSettingMasterId);
        }
    }
}
