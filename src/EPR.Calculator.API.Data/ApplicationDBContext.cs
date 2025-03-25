namespace EPR.Calculator.API.Data
{
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Data.DataSeeder;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext()
        {
        }

        public ApplicationDBContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<DefaultParameterSettingMaster> DefaultParameterSettings { get; set; }

        public virtual DbSet<DefaultParameterSettingDetail> DefaultParameterSettingDetail { get; set; }

        public DbSet<DefaultParameterTemplateMaster> DefaultParameterTemplateMasterList { get; set; }

        public DbSet<CalculatorRunClassification> CalculatorRunClassifications { get; set; }

        public virtual DbSet<CalculatorRun> CalculatorRuns { get; set; }

        public DbSet<LapcapDataTemplateMaster> LapcapDataTemplateMaster { get; set; }

        public DbSet<LapcapDataMaster> LapcapDataMaster { get; set; }

        public DbSet<LapcapDataDetail> LapcapDataDetail { get; set; }

        public DbSet<CalculatorRunOrganisationDataDetail> CalculatorRunOrganisationDataDetails { get; set; }

        public DbSet<CalculatorRunOrganisationDataMaster> CalculatorRunOrganisationDataMaster { get; set; }

        public DbSet<CalculatorRunPomDataDetail> CalculatorRunPomDataDetails { get; set; }

        public DbSet<CalculatorRunPomDataMaster> CalculatorRunPomDataMaster { get; set; }

        public DbSet<OrganisationData> OrganisationData { get; set; }

        public DbSet<PomData> PomData { get; set; }

        public virtual DbSet<Material> Material { get; set; }

        public DbSet<ProducerDetail> ProducerDetail { get; set; }

        public virtual DbSet<CountryApportionment> CountryApportionment { get; set; }

        public virtual DbSet<ProducerReportedMaterial> ProducerReportedMaterial { get; set; }

        public DbSet<CostType> CostType { get; set; }

        public virtual DbSet<Country> Country { get; set; }

        public virtual DbSet<CalculatorRunCsvFileMetadata> CalculatorRunCsvFileMetadata { get; set; }

        public virtual DbSet<SubmissionPeriodLookup> SubmissionPeriodLookup { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer();
            }
        }
        public virtual DbSet<CalculatorRunFinancialYear> FinancialYears { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DefaultParameterTemplateMaster>().Property(x => x.ValidRangeFrom).HasPrecision(18, 3);
            modelBuilder.Entity<DefaultParameterTemplateMaster>().Property(x => x.ValidRangeTo).HasPrecision(18, 3);
            modelBuilder.Entity<DefaultParameterSettingDetail>();
            modelBuilder.Entity<DefaultParameterSettingMaster>();

            modelBuilder.Entity<DefaultParameterSettingMaster>()
            .HasMany(e => e.Details)
            .WithOne(e => e.DefaultParameterSettingMaster)
            .HasForeignKey(e => e.DefaultParameterSettingMasterId)
            .IsRequired(true);

            modelBuilder.Entity<LapcapDataTemplateMaster>();
            modelBuilder.Entity<LapcapDataMaster>();
            modelBuilder.Entity<LapcapDataDetail>();

            modelBuilder.Entity<Material>();
            modelBuilder.Entity<Country>();
            modelBuilder.Entity<CostType>();
            modelBuilder.Entity<CountryApportionment>();
            modelBuilder.Entity<ProducerDetail>();
            modelBuilder.Entity<ProducerReportedMaterial>();
            modelBuilder.Entity<CalculatorRunCsvFileMetadata>();
            modelBuilder.Entity<SubmissionPeriodLookup>()
            .HasKey(e => e.SubmissionPeriod);

            modelBuilder.Entity<LapcapDataTemplateMaster>()
            .HasMany(e => e.Details)
            .WithOne(e => e.LapcapDataTemplateMaster)
            .HasForeignKey(e => e.UniqueReference)
            .IsRequired(true);

            modelBuilder.Entity<LapcapDataMaster>()
            .HasMany(e => e.Details)
            .WithOne(e => e.LapcapDataMaster)
            .HasForeignKey(e => e.LapcapDataMasterId)
            .IsRequired(true);

            modelBuilder.Entity<LapcapDataTemplateMaster>()
            .HasMany(e => e.Details)
            .WithOne(e => e.LapcapDataTemplateMaster)
            .HasForeignKey(e => e.UniqueReference)
            .IsRequired(true);

            modelBuilder.Entity<CalculatorRunOrganisationDataDetail>();
            modelBuilder.Entity<CalculatorRunOrganisationDataMaster>()
            .HasMany(e => e.Details)
            .WithOne(e => e.CalculatorRunOrganisationDataMaster)
            .HasForeignKey(e => e.CalculatorRunOrganisationDataMasterId)
            .IsRequired(true);

            modelBuilder.Entity<CalculatorRunPomDataDetail>();
            modelBuilder.Entity<CalculatorRunPomDataMaster>()
            .HasMany(e => e.Details)
            .WithOne(e => e.CalculatorRunPomDataMaster)
            .HasForeignKey(e => e.CalculatorRunPomDataMasterId);

            modelBuilder.Entity<OrganisationData>().HasNoKey();
            modelBuilder.Entity<PomData>().HasNoKey();

            modelBuilder.Entity<CalculatorRun>().Property(e => e.CalculatorRunPomDataMasterId).IsRequired(false);
            modelBuilder.Entity<CalculatorRun>().Property(e => e.CalculatorRunOrganisationDataMasterId).IsRequired(false);
            modelBuilder.Entity<CalculatorRun>().Property(e => e.LapcapDataMasterId).IsRequired(false);
            modelBuilder.Entity<CalculatorRun>().Property(e => e.DefaultParameterSettingMasterId).IsRequired(false);

            modelBuilder.Entity<CalculatorRunPomDataMaster>()
            .HasMany(e => e.RunDetails)
            .WithOne(e => e.CalculatorRunPomDataMaster)
            .HasForeignKey(e => e.CalculatorRunPomDataMasterId);

            modelBuilder.Entity<CalculatorRunOrganisationDataMaster>()
            .HasMany(e => e.RunDetails)
            .WithOne(e => e.CalculatorRunOrganisationDataMaster)
            .HasForeignKey(e => e.CalculatorRunOrganisationDataMasterId);

            modelBuilder.Entity<LapcapDataMaster>()
            .HasMany(e => e.RunDetails)
            .WithOne(e => e.LapcapDataMaster)
            .HasForeignKey(e => e.LapcapDataMasterId);

            modelBuilder.Entity<DefaultParameterSettingMaster>()
            .HasMany(e => e.RunDetails)
            .WithOne(e => e.DefaultParameterSettingMaster)
            .HasForeignKey(e => e.DefaultParameterSettingMasterId);

            modelBuilder.Entity<CalculatorRun>()
            .HasMany(e => e.CountryApportionments)
            .WithOne(e => e.CalculatorRun)
            .HasForeignKey(e => e.CalculatorRunId);

            modelBuilder.Entity<CalculatorRun>()
            .HasMany(e => e.ProducerDetails)
            .WithOne(e => e.CalculatorRun)
            .HasForeignKey(e => e.CalculatorRunId);

            modelBuilder.Entity<Material>()
            .HasMany(e => e.ProducerReportedMaterials)
            .WithOne(e => e.Material)
            .HasForeignKey(e => e.MaterialId);

            modelBuilder.Entity<ProducerDetail>()
            .HasMany(e => e.ProducerReportedMaterials)
            .WithOne(e => e.ProducerDetail)
            .HasForeignKey(e => e.ProducerDetailId);

            modelBuilder.Entity<CostType>()
            .HasMany(e => e.CountryApportionments)
            .WithOne(e => e.CostType)
            .HasForeignKey(e => e.CostTypeId);

            modelBuilder.Entity<Country>()
            .HasMany(e => e.CountryApportionments)
            .WithOne(e => e.Country)
            .HasForeignKey(e => e.CountryId);

            modelBuilder.Entity<CalculatorRunFinancialYear>()
            .HasMany(e => e.CalculatorRuns)
            .WithOne(e => e.Financial_Year)
            .HasForeignKey(e => e.FinancialYearId)
            .HasPrincipalKey(e => e.Name);

            modelBuilder.Entity<CalculatorRunFinancialYear>()
            .HasMany(e => e.DefaultParameterSettingMasters)
            .WithOne(e => e.ParameterYear)
            .HasForeignKey(e => e.ParameterYearId)
            .HasPrincipalKey(e => e.Name);

            modelBuilder.Entity<CalculatorRunFinancialYear>()
            .HasMany(e => e.LapcapDataMasters)
            .WithOne(e => e.ProjectionYear)
            .HasForeignKey(e => e.ProjectionYearId)
            .HasPrincipalKey(e => e.Name);

            modelBuilder.Entity<CalculatorRunCsvFileMetadata>().Property(e => e.CalculatorRunId).IsRequired(true);

            Seeder.Initialize(modelBuilder);
        }
    }
}