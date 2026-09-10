using Microsoft.EntityFrameworkCore;

namespace EPR.CommonDataService.DataApi.CommonDataApi.LoadTables;

/// <summary>
///     The DataApi's own context over the two staging tables it fills from the RPD source and reads
///     back for a run. Deliberately independent of the calculator's <c>ApplicationDBContext</c> - the
///     DataApi is intended to become its own HTTP service. The tables' DDL is created by the
///     calculator's migration <c>20260902173358_ReplaceOrgPomStagingWithCalculatorRunOrganisation</c>;
///     <see cref="Microsoft.EntityFrameworkCore.Metadata.Builders.TableBuilder.ExcludeFromMigrations" />
///     keeps them out of any migration this context might generate.
/// </summary>
public sealed class DataApiLoadContext(DbContextOptions<DataApiLoadContext> options) : DbContext(options)
{
    public DbSet<LoadOrganisation> LoadOrganisations => Set<LoadOrganisation>();

    public DbSet<LoadPom> LoadPoms => Set<LoadPom>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoadOrganisation>(entity =>
        {
            entity.ToTable("data_api_load_organisations", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.OrganisationId).HasColumnName("organisation_id");
            entity.Property(e => e.SubsidiaryId).HasColumnName("subsidiary_id").HasMaxLength(400);
            entity.Property(e => e.SubmitterId).HasColumnName("submitter_id").HasMaxLength(400);
            entity.Property(e => e.OrganisationName).HasColumnName("organisation_name").HasMaxLength(400);
            entity.Property(e => e.TradingName).HasColumnName("trading_name").HasMaxLength(400);
            entity.Property(e => e.StatusCode).HasColumnName("status_code").HasMaxLength(400);
            entity.Property(e => e.LeaverDate).HasColumnName("leaver_date").HasMaxLength(50);
            entity.Property(e => e.JoinerDate).HasColumnName("joiner_date").HasMaxLength(50);
            entity.Property(e => e.RegulatorStatus).HasColumnName("regulator_status").HasMaxLength(50);
            entity.Property(e => e.ObligationStatus).HasColumnName("obligation_status").HasMaxLength(10);
            entity.Property(e => e.NumDaysObligated).HasColumnName("num_days_obligated");
            entity.Property(e => e.ErrorCode).HasColumnName("error_code");
            entity.Property(e => e.SubmissionPeriodYear).HasColumnName("submission_period_year");
            entity.Property(e => e.HasH1).HasColumnName("has_h1");
            entity.Property(e => e.HasH2).HasColumnName("has_h2");
            entity.Property(e => e.FileName).HasColumnName("file_name").HasMaxLength(400);
            entity.Property(e => e.IsResubmission).HasColumnName("is_resubmission");
            entity.Property(e => e.CreatedDateTime).HasColumnName("created_date_time");
            entity.Property(e => e.LoadTs).HasColumnName("load_ts");
        });

        modelBuilder.Entity<LoadPom>(entity =>
        {
            entity.ToTable("data_api_load_poms", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.OrganisationId).HasColumnName("organisation_id");
            entity.Property(e => e.SubsidiaryId).HasColumnName("subsidiary_id").HasMaxLength(400);
            entity.Property(e => e.SubmitterId).HasColumnName("submitter_id").HasMaxLength(400);
            entity.Property(e => e.SubmissionPeriod).HasColumnName("submission_period").HasMaxLength(400);
            entity.Property(e => e.SubmissionPeriodDescription).HasColumnName("submission_period_desc").HasMaxLength(400);
            entity.Property(e => e.PackagingActivity).HasColumnName("packaging_activity").HasMaxLength(400);
            entity.Property(e => e.PackagingType).HasColumnName("packaging_type").HasMaxLength(400);
            entity.Property(e => e.PackagingClass).HasColumnName("packaging_class").HasMaxLength(400);
            entity.Property(e => e.PackagingMaterial).HasColumnName("packaging_material").HasMaxLength(400);
            entity.Property(e => e.PackagingMaterialSubtype).HasColumnName("packaging_material_subtype").HasMaxLength(400);
            entity.Property(e => e.PackagingMaterialWeight).HasColumnName("packaging_material_weight");
            entity.Property(e => e.RamRagRating).HasColumnName("ram_rag_rating").HasMaxLength(50);
            entity.Property(e => e.FileName).HasColumnName("file_name").HasMaxLength(400);
            entity.Property(e => e.IsResubmission).HasColumnName("is_resubmission");
            entity.Property(e => e.CreatedDateTime).HasColumnName("created_date_time");
            entity.Property(e => e.LoadTs).HasColumnName("load_ts");
        });
    }
}
