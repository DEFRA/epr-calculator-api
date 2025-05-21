namespace EPR.Calculator.API.Data
{
    using System.Reflection;
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

        public virtual DbSet<CalculatorRunClassification> CalculatorRunClassifications { get; set; }

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

        public DbSet<ProducerInvoicedMaterialNetTonnage> ProducerInvoicedMaterialNetTonnage { get; set; }

        public DbSet<ProducerDesignatedRunInvoiceInstruction> ProducerDesignatedRunInvoiceInstruction { get; set; }

        public DbSet<ProducerResultFileSuggestedBillingInstruction> ProducerResultFileSuggestedBillingInstruction { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer();
            }
        }

        public virtual DbSet<CalculatorRunFinancialYear> FinancialYears { get; set; }

        public virtual DbSet<CalculatorRunBillingFileMetadata> CalculatorRunBillingFileMetadata { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            Seeder.Initialize(modelBuilder);
        }
    }
}