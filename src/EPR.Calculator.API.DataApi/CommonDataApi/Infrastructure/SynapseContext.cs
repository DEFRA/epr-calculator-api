using EPR.CommonDataService.DataApi.CommonDataApi.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace EPR.CommonDataService.DataApi.CommonDataApi.Infrastructure;

[ExcludeFromCodeCoverage]
public class SynapseContext : DbContext
{
    public DbSet<PayCalOrganisation> PayCalOrganisations { get; set; } = null!;
    public DbSet<PayCalPom> PayCalPoms { get; set; } = null!;

    public SynapseContext(DbContextOptions<SynapseContext> options)
        : base(options)
    {
    }

    public SynapseContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PayCalOrganisation>(entity =>
        {
            // The data source for this entity is the inline query in StreamOrganisationsRequestHandler
            entity.HasNoKey();
            entity.Property(e => e.OrganisationId).HasColumnName("organisation_id");
            entity.Property(e => e.SubsidiaryId).HasColumnName("subsidiary_id").HasMaxLength(4000);
            entity.Property(e => e.SubmitterId).HasColumnName("submitter_id").HasMaxLength(4000);
            entity.Property(e => e.OrganisationName).HasColumnName("organisation_name").HasMaxLength(4000);
            entity.Property(e => e.TradingName).HasColumnName("trading_name").HasMaxLength(4000);
            entity.Property(e => e.StatusCode).HasColumnName("status_code").HasMaxLength(4000);
            entity.Property(e => e.LeaverDate).HasColumnName("leaver_date").HasMaxLength(4000);
            entity.Property(e => e.JoinerDate).HasColumnName("joiner_date").HasMaxLength(4000);
            entity.Property(e => e.RegulatorStatus).HasColumnName("regulator_status").HasMaxLength(4000);
            entity.Property(e => e.SubmissionPeriodYear).HasColumnName("submission_period_year");

            // ObligationStatus/NumDaysObligated/ErrorCode (IProducerObligationDeterminer) and
            // HasH1/HasH2 (see IOrganisationPeriodFlagsCalculator) are computed in C# from the raw
            // columns above and the POM stream - the organisations query no longer selects them, so
            // they must not be mapped to a column here (FromSqlInterpolated would fail looking for it).
            entity.Ignore(e => e.ObligationStatus);
            entity.Ignore(e => e.NumDaysObligated);
            entity.Ignore(e => e.ErrorCode);
            entity.Ignore(e => e.HasH1);
            entity.Ignore(e => e.HasH2);
        });

        modelBuilder.Entity<PayCalPom>(entity =>
        {
            // The data source for this entity is the inline query in StreamPomsRequestHandler
            entity.HasNoKey();
            entity.Property(e => e.OrganisationId).HasColumnName("organisation_id");
            entity.Property(e => e.SubsidiaryId).HasColumnName("subsidiary_id").HasMaxLength(4000);
            entity.Property(e => e.SubmitterId).HasColumnName("submitter_id").HasMaxLength(4000);
            entity.Property(e => e.SubmissionPeriod).HasColumnName("submission_period").HasMaxLength(4000);
            entity.Property(e => e.SubmissionPeriodDescription).HasColumnName("submission_period_desc").HasMaxLength(4000);
            entity.Property(e => e.PackagingActivity).HasColumnName("packaging_activity").HasMaxLength(4000);
            entity.Property(e => e.PackagingType).HasColumnName("packaging_type").HasMaxLength(4000);
            entity.Property(e => e.PackagingClass).HasColumnName("packaging_class").HasMaxLength(4000);
            entity.Property(e => e.PackagingMaterial).HasColumnName("packaging_material").HasMaxLength(4000);
            entity.Property(e => e.PackagingMaterialSubtype).HasColumnName("packaging_material_subtype").HasMaxLength(4000);
            entity.Property(e => e.PackagingMaterialWeight).HasColumnName("packaging_material_weight");
            entity.Property(e => e.RamRagRating).HasColumnName("ram_rag_rating").HasMaxLength(4000);
        });
    }
}
