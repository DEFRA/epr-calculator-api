using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations;

public class ProducerFeesConfiguration : IEntityTypeConfiguration<ProducerFees>
{
    public void Configure(EntityTypeBuilder<ProducerFees> builder)
    {
        builder.ToTable("calc_result_producer_fees");

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(p => p.CalculatorRunId)
            .HasColumnName("calculator_run_id")
            .IsRequired();

        builder.OwnsMany(p => p.Details, fees =>
        {
            fees.ToJson("details");
            ConfigureProducerFeesDetailEntity(fees);
        });
        
        builder.OwnsOne(p => p.Total, total =>
        {
            total.ToJson("total");
            ConfigureProducerFeesDetailEntity(total);
        });
    }

    private static void ConfigureProducerFeesDetailEntity(
        OwnedNavigationBuilder<ProducerFees, ProducerFeeDetail> b)
    {
        b.OwnsOne(x => x.LADisposalCostsSection1, s => ConfigureBadDebt(s));
        b.OwnsOne(x => x.CommsCostsSection2a, s => ConfigureBadDebt(s));
        b.OwnsOne(x => x.CommsCostsSection2b, s => ConfigureBadDebt(s));
        b.OwnsOne(x => x.CommsCostsSection2c, s => ConfigureBadDebt(s));
        b.OwnsOne(x => x.SaOperatingCostsSection3, s => ConfigureBadDebt(s));
        b.OwnsOne(x => x.LaDataPrepSection4, s => ConfigureBadDebt(s));
        b.OwnsOne(x => x.SaSetupCostsSection5, s => ConfigureBadDebt(s));
        b.OwnsOne(x => x.TotalBillBreakdown, s => ConfigureBadDebt(s));
        b.OwnsOne(x => x.BillingInstruction, bi =>
        {
            bi.Property(x => x.MaterialityLiabilityDirection).HasConversion<string>();
            bi.Property(x => x.TonnageAmountLiabilityDirection).HasConversion<string>();
            bi.Property(x => x.MaterialityPercentageLiabilityDirection).HasConversion<string>();
            bi.Property(x => x.TonnageAmountPercentageLiabilityDirection).HasConversion<string>();
        });

        b.Ignore(x => x.FeesByMaterial);
        b.Ignore(x => x.DisposalFeesByMaterial);
        b.Ignore(x => x.CommsFeesByMaterial);

        b.OwnsMany(x => x.MaterialFees, pfm =>
        {
            pfm.HasJsonPropertyName("MaterialFees");
            pfm.OwnsOne(x => x.DisposalFee, disposal =>
            {
                disposal.OwnsOne(x => x.HhTonnage);
                disposal.OwnsOne(x => x.PbTonnage);
                disposal.OwnsOne(x => x.HdcTonnage);
                disposal.OwnsOne(x => x.TotalTonnage);
                disposal.OwnsOne(x => x.FeeWithBadDebtByCountry, c => c.Ignore(x => x.Total));
                disposal.OwnsOne(x => x.ActionedSmcwTonnage);
                disposal.OwnsOne(x => x.NetTonnage);
                disposal.OwnsOne(x => x.PricePerTonne);
                disposal.OwnsOne(x => x.Fee);
            });

            pfm.OwnsOne(x => x.CommFee, comm =>
            {
                comm.OwnsOne(x => x.Costs, s => ConfigureBadDebt(s));
            });
        });
    }

    private static void ConfigureBadDebt<T>(
        OwnedNavigationBuilder<T, FeeWithBadDebt> section) where T : class =>
        section.OwnsOne(x => x.ByCountry, c => c.Ignore(x => x.Total));
}
