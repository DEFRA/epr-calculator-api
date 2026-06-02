using System.Reflection;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataSeeder;
using EPR.Calculator.API.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Data;

public class ApplicationDBContext : DbContext
{
    public ApplicationDBContext()
    {
    }

    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
        : base(options)
    {
    }

    public DbSet<DefaultParameterSettingMaster> DefaultParameterSettings { get; set; }
    public DbSet<DefaultParameterSettingDetail> DefaultParameterSettingDetail { get; set; }
    public DbSet<DefaultParameterTemplateMaster> DefaultParameterTemplateMasterList { get; set; }
    public DbSet<CalculatorRunClassification> CalculatorRunClassifications { get; set; }
    public DbSet<CalculatorRun> CalculatorRuns { get; set; }
    public DbSet<LapcapDataTemplateMaster> LapcapDataTemplateMaster { get; set; }
    public DbSet<LapcapDataMaster> LapcapDataMaster { get; set; }
    public DbSet<LapcapDataDetail> LapcapDataDetail { get; set; }
    public DbSet<CalculatorRunOrganisationDataDetail> CalculatorRunOrganisationDataDetails { get; set; }
    public DbSet<CalculatorRunOrganisationDataMaster> CalculatorRunOrganisationDataMaster { get; set; }
    public DbSet<CalculatorRunPomDataDetail> CalculatorRunPomDataDetails { get; set; }
    public DbSet<CalculatorRunPomDataMaster> CalculatorRunPomDataMaster { get; set; }
    public DbSet<OrganisationData> OrganisationData { get; set; }
    public DbSet<PomData> PomData { get; set; }
    public DbSet<Material> Material { get; set; }
    public DbSet<ProducerDetail> ProducerDetail { get; set; }
    public DbSet<CountryApportionment> CountryApportionment { get; set; }
    public DbSet<ProducerReportedMaterial> ProducerReportedMaterial { get; set; }
    public DbSet<ProducerReportedMaterialProjected> ProducerReportedMaterialProjected { get; set; }
    public DbSet<CostType> CostType { get; set; }
    public DbSet<Country> Country { get; set; }
    public DbSet<TransformProjectedH1> TransformProjectedH1 { get; set; }
    public DbSet<TransformProjectedH2> TransformProjectedH2 { get; set; }
    public DbSet<TransformScaled> TransformScaled { get; set; }
    public DbSet<TransformPartial> TransformPartial { get; set; }
    public DbSet<CalculatorRunCsvFileMetadata> CalculatorRunCsvFileMetadata { get; set; }
    public DbSet<SubmissionPeriodLookup> SubmissionPeriodLookup { get; set; }
    public DbSet<ProducerInvoicedMaterialNetTonnage> ProducerInvoicedMaterialNetTonnage { get; set; }
    public DbSet<ProducerDesignatedRunInvoiceInstruction> ProducerDesignatedRunInvoiceInstruction { get; set; }
    public DbSet<ProducerResultFileSuggestedBillingInstruction> ProducerResultFileSuggestedBillingInstruction { get; set; }
    public DbSet<CalculatorRunRelativeYear> CalculatorRunRelativeYears { get; set; }
    public DbSet<CalculatorRunBillingFileMetadata> CalculatorRunBillingFileMetadata { get; set; }
    public DbSet<ErrorReport> ErrorReports { get; set; }

    public async Task<RelativeYear?> FindRelativeYearAsync(int value, CancellationToken cancellationToken = default)
    {
        var entity = await CalculatorRunRelativeYears
            .SingleOrDefaultAsync(x => x.Value == value, cancellationToken);

        return entity == null ? null : new RelativeYear(entity.Value);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        Seeder.Initialize(modelBuilder);
    }
}
