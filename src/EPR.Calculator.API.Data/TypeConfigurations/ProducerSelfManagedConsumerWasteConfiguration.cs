using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class ProducerSelfManagedConsumerWasteConfiguration : IEntityTypeConfiguration<ProducerSelfManagedConsumerWaste>
{
    public void Configure(EntityTypeBuilder<ProducerSelfManagedConsumerWaste> builder)
    {
        builder.ToTable("calc_result_smcw_producer");

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(x => x.SmcwId)
            .HasColumnName("smcw_id")
            .IsRequired();

        builder.HasOne<SelfManagedConsumerWaste>()
            .WithMany(p => p.ProducerTotals)
            .HasForeignKey(x => x.SmcwId);

        builder.Property(p => p.ProducerId)
            .HasColumnName("producer_id")
            .HasMaxLength(400)
            .IsRequired();

        builder.Property(p => p.SubsidiaryId)
            .HasColumnName("subsidiary_id")
            .HasMaxLength(400);

        builder.Property(p => p.Level)
            .HasColumnName("level")
            .HasMaxLength(10)
            .IsRequired();

        builder.OwnsMany(x => x.SmcwMaterialData, data =>
        {
            data.ToJson("material_totals");
            data.OwnsOne(a => a.Smcw, smcw => 
            {
                smcw.OwnsOne(x => x.ActionedSmcwTonnage);
                smcw.OwnsOne(x => x.NetTonnage);
            });
        });

        builder.Ignore(p => p.SmcwByMaterial);
    }
}
