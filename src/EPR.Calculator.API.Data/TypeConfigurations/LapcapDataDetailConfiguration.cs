using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class LapcapDataDetailConfiguration : IEntityTypeConfiguration<LapcapDataDetail>
    {
        /// <inheritdoc />
        // NOSONAR
        public void Configure(EntityTypeBuilder<LapcapDataDetail> builder)
        {
            builder.ToTable("lapcap_data_detail");

            builder.Property(p => p.Id)
                   .HasColumnName("id")
                   .IsRequired();

            builder.Property(p => p.LapcapDataMasterId)
                   .HasColumnName("lapcap_data_master_id");

            builder.Property(p => p.UniqueReference)
                   .HasColumnName("lapcap_data_template_master_unique_ref")
                   .HasMaxLength(400);

            builder.Property(p => p.TotalCost)
                   .HasColumnName("total_cost");
        }
    }
}
