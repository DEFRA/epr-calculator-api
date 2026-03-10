using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class ProducerReportedMaterialConfiguration : IEntityTypeConfiguration<ProducerReportedMaterial>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<ProducerReportedMaterial> builder)
        {
            builder.ToTable("producer_reported_material");

            builder.Property(p => p.Id)
                   .HasColumnName("id")
                   .IsRequired();

            builder.Property(p => p.MaterialId)
                   .HasColumnName("material_id")
                   .IsRequired();

            builder.Property(p => p.ProducerDetailId)
                   .HasColumnName("producer_detail_id")
                   .IsRequired();

            builder.Property(p => p.PackagingType)
                   .HasColumnName("packaging_type")
                   .HasMaxLength(400);

            builder.Property(p => p.RedRamRagRating)
                   .HasColumnName("red_ram_rag_rating")
                   .HasPrecision(18, 3);

            builder.Property(p => p.AmberRamRagRating)
                   .HasColumnName("amber_ram_rag_rating")
                   .HasPrecision(18, 3);

            builder.Property(p => p.GreenRamRagRating)
                   .HasColumnName("green_ram_rag_rating")
                   .HasPrecision(18, 3);

            builder.Property(p => p.RedMedicalRamRagRating)
                   .HasColumnName("red_medical_ram_rag_rating")
                   .HasPrecision(18, 3);

            builder.Property(p => p.AmberMedicalRamRagRating)
                   .HasColumnName("amber_medical_ram_rag_rating")
                   .HasPrecision(18, 3);

            builder.Property(p => p.GreenMedicalRamRagRating)
                   .HasColumnName("green_medical_ram_rag_rating")
                   .HasPrecision(18, 3);

            builder.Property(p => p.PackagingTonnage)
                   .HasColumnName("packaging_tonnage")
                   .HasPrecision(18, 3);
        }
    }
}
