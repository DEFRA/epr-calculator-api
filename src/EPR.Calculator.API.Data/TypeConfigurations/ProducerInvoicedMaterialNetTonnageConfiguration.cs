using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class ProducerInvoicedMaterialNetTonnageConfiguration : IEntityTypeConfiguration<ProducerInvoicedMaterialNetTonnage>
    {
        public void Configure(EntityTypeBuilder<ProducerInvoicedMaterialNetTonnage> builder)
        {
            builder.ToTable("producer_invoiced_material_net_tonnage");

            builder.Property(p => p.Id)
                   .HasColumnName("id")
                   .IsRequired();

            builder.Property(p => p.CalculatorRunId)
                   .HasColumnName("calculator_run_id")
                   .IsRequired();

            builder.Property(p => p.MaterialId)
                   .HasColumnName("material_id")
                   .IsRequired();

            builder.Property(p => p.ProducerId)
                   .HasColumnName("producer_id")
                   .IsRequired();

            builder.Property(p => p.InvoicedNetTonnage)
                   .HasColumnName("invoiced_net_tonnage")
                   .HasMaxLength(4000);
        }
    }
}
