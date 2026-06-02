using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class LapcapDataMasterConfiguration : IEntityTypeConfiguration<LapcapDataMaster>
{
    public void Configure(EntityTypeBuilder<LapcapDataMaster> builder)
    {
        builder.ToTable("lapcap_data_master");

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(p => p.RelativeYear)
            .HasColumnName("relative_year")
            .IsRequired();

        builder.HasOne<CalculatorRunRelativeYear>()
            .WithMany()
            .HasForeignKey(e => e.RelativeYear)
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
            .IsRequired();

        builder.HasMany(e => e.RunDetails)
            .WithOne(e => e.LapcapDataMaster)
            .HasForeignKey(e => e.LapcapDataMasterId);
    }
}
