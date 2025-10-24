using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.Configurations;

public class PIMaterialNetTonnageConfiguration : IEntityTypeConfiguration<ProducerInvoicedMaterialNetTonnage>
{
    public void Configure(EntityTypeBuilder<ProducerInvoicedMaterialNetTonnage> builder)
    {
        builder.HasIndex(e => new { e.ProducerId, e.CalculatorRunId, e.Id })
                    .HasDatabaseName("IX_index_producer_invoiced_material_net_tonnage")
                    .IncludeProperties(e => new
                    {
                        e.MaterialId,
                        e.InvoicedNetTonnage
                    })
                    .IsClustered(false);
    }
}