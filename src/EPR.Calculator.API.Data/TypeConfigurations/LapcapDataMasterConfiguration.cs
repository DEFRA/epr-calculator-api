using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class LapcapDataMasterConfiguration : IEntityTypeConfiguration<LapcapDataMaster>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<LapcapDataMaster> builder)
        {
            builder.ToTable("lapcap_data_master");

            builder.Property(p => p.Id)
                   .HasColumnName("id")
                   .IsRequired();

            builder.Property(p => p.RelativeYearValue)
                   .HasColumnName("relative_year")
                   .IsRequired();

            builder.Ignore(p => p.RelativeYear);

            builder.HasOne<CalculatorRunRelativeYear>()
                   .WithMany()
                   .HasForeignKey(e => e.RelativeYearValue)
                   .HasPrincipalKey(e => e.Value)
                   .OnDelete(DeleteBehavior.Restrict);

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

            builder.Property(p => p.LapcapFileName)
                  .HasColumnName("lapcap_filename")
                  .HasMaxLength(256)
                  .IsRequired();

            builder.HasMany(e => e.Details)
                   .WithOne(e => e.LapcapDataMaster)
                   .HasForeignKey(e => e.LapcapDataMasterId)
                   .IsRequired(true);

            builder.HasMany(e => e.RunDetails)
                   .WithOne(e => e.LapcapDataMaster)
                   .HasForeignKey(e => e.LapcapDataMasterId);
        }
    }
}
