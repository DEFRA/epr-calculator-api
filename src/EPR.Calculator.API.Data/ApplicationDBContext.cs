using System.Reflection;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataSeeder;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Data.DataTypes.Enums;
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
    public DbSet<CostType> CostType { get; set; }
    public DbSet<Country> Country { get; set; }
    public DbSet<TransformProjectedH1> TransformProjectedH1 { get; set; }
    public DbSet<TransformProjectedH2> TransformProjectedH2 { get; set; }
    public DbSet<TransformScaled> TransformScaled { get; set; }
    public DbSet<TransformPartial> TransformPartial { get; set; }
    public DbSet<ProducerMaterialPackaging> ProducerMaterialPackaging { get; set; }
    public DbSet<ProducerFees> ProducerDisposalFee { get; set; }
    public DbSet<ProducerFeeDetail> ProducerFeeDetails { get; set; }
    public DbSet<SelfManagedConsumerWaste> SelfManagedConsumerWaste { get; set; }
    public DbSet<ProducerSelfManagedConsumerWaste> ProducerSelfManagedConsumerWaste { get; set; }
    public DbSet<ModulationResult> ModulationResult { get; set; }
    public DbSet<CalculatorRunCsvFileMetadata> CalculatorRunCsvFileMetadata { get; set; }
    public DbSet<SubmissionPeriodLookup> SubmissionPeriodLookup { get; set; }
    public DbSet<ProducerInvoicedMaterialNetTonnage> ProducerInvoicedMaterialNetTonnage { get; set; }
    public DbSet<ProducerDesignatedRunInvoiceInstruction> ProducerDesignatedRunInvoiceInstruction { get; set; }
    public DbSet<ProducerResultFileSuggestedBillingInstruction> ProducerResultFileSuggestedBillingInstruction { get; set; }
    public DbSet<CalculatorRunRelativeYear> CalculatorRunRelativeYears { get; set; }
    public DbSet<CalculatorRunBillingFileMetadata> CalculatorRunBillingFileMetadata { get; set; }
    public DbSet<ErrorReport> ErrorReports { get; set; }
    public DbSet<CalcResultLapcapDataEntry> LapcapData { get; set; }
    public DbSet<CalcResultCommsCostEntry> CommCost { get; set; }
    public DbSet<CalcResultLateReportingTonnageEntry> LateReportingTonnage { get; set; }
    public DbSet<CalcResultParameterOtherCostEntry> ParameterOtherCost { get; set; }
    public DbSet<CalcResultOnePlusFourApportionmentEntry> OnePlusFourApportionment { get; set; }
    public DbSet<CalcResultLaDisposalCostDataEntry> LaDisposalCostData { get; set; }
    public DbSet<CalcResultCancelledProducerEntry> CancelledProducers { get; set; }

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

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Configures global conversion for RelativeYear struct to underlying database int type.
        configurationBuilder.Properties<RelativeYear>()
            .HaveConversion<RelativeYearValueConverter, RelativeYearValueComparer>();

        // Configures global conversion for BillingRunStatus enum to database string type.
        configurationBuilder.Properties<BillingRunStatus>()
            .HaveConversion<StringEnumConverter<BillingRunStatus>, EnumComparer<BillingRunStatus>>();

        configurationBuilder.Properties<decimal>().HavePrecision(18, 6);
    }
}
