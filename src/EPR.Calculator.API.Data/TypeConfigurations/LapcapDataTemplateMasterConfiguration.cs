using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class LapcapDataTemplateMasterConfiguration : IEntityTypeConfiguration<LapcapDataTemplateMaster>
    {
        /// <inheritdoc />
        // NOSONAR
        public void Configure(EntityTypeBuilder<LapcapDataTemplateMaster> builder)
        {
            builder.ToTable("lapcap_data_template_master");

            builder.HasKey(k => k.UniqueReference);

            builder.Property(p => p.UniqueReference)
                   .HasColumnName("unique_ref")
                   .HasMaxLength(400)
                   .IsRequired();

            builder.Property(p => p.Country)
                   .HasColumnName("country")
                   .HasMaxLength(400);

            builder.Property(p => p.Material)
                   .HasColumnName("material")
                   .HasMaxLength(400);

            builder.Property(p => p.TotalCostFrom)
                   .HasColumnName("total_cost_from")
                   .HasPrecision(18, 2);

            builder.Property(p => p.TotalCostTo)
                   .HasColumnName("total_cost_to")
            .HasPrecision(18, 2);

            builder.HasMany(e => e.Details)
                   .WithOne(e => e.LapcapDataTemplateMaster)
                   .HasForeignKey(e => e.UniqueReference)
                   .IsRequired(true);
        }
    }
}
