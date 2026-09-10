using System.Globalization;
using System.Security.Claims;
using CsvHelper;
using CsvHelper.Configuration;
using EPR.Calculator.API.App;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.Services.CommonDataApi;
using EPR.Calculator.API.BackgroundService.Telemetry.Internals;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.API.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Testcontainers.MsSql;
using TelemetryLoggingHost = EPR.Calculator.API.BackgroundService.Telemetry.Internals.TelemetryLoggingHost;

namespace EPR.Calculator.API.IntegrationTests;

public abstract class BaseIntegrationTest
{
    protected static readonly DateTime Now = DateTime.UtcNow;

    private static readonly MsSqlContainer SqlContainer =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
            .WithReuse(true)
            .WithCreateParameterModifier(parameters => parameters.User = "0:0") // SQL Server 2025's non-root default can't create /.system
            .Build();

    protected static ServiceProvider Provider { get; private set; } = null!;

    public static async Task InitializeAsync()
    {
        await SqlContainer.StartAsync();

        var services = new ServiceCollection();
        ConfigureServices(services);
        Provider = services.BuildServiceProvider();

        // Nothing here runs a Host, so hosted services (OpenTelemetry's TelemetryHostedService,
        // which builds TracerProvider/MeterProvider, and MetricLogging) must be started manually -
        // otherwise ActivityLogging/MetricLogging are silently never wired up.
        foreach (var hostedService in Provider.GetServices<IHostedService>())
            await hostedService.StartAsync(CancellationToken.None);

        using var scope = Provider.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDBContext>>();
        await using var db = await factory.CreateDbContextAsync();

        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        // The SQL Server container is reused (WithReuse(true)) across local test sessions to
        // avoid paying its startup cost every run, so its data outlives any single `dotnet test`
        // invocation. If a previous session was interrupted (e.g. a cancelled debug run) while a
        // run was mid-flight, that run is left behind still classified as RUNNING/IN_THE_QUEUE.
        // CalculatorController.Create() refuses to start a new run while any run anywhere has
        // one of those classifications, so a single abandoned row can permanently block every
        // future session. Reclassify any such leftovers as errored before tests start.
        await db.CalculatorRuns
            .Where(run =>
                run.CalculatorRunClassificationId == RunClassificationStatusIds.RUNNINGID ||
                run.CalculatorRunClassificationId == RunClassificationStatusIds.INTHEQUEUEID)
            .ExecuteUpdateAsync(s => s.SetProperty(run => run.CalculatorRunClassificationId, RunClassificationStatusIds.ERRORID));
    }

    public static async Task CleanupAsync()
    {
        foreach (var hostedService in Provider.GetServices<IHostedService>())
            await hostedService.StopAsync(CancellationToken.None);

        await Provider.DisposeAsync();
        Console.Out.Flush();
        Log.CloseAndFlush();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var connectionString = new SqlConnectionStringBuilder(SqlContainer.GetConnectionString())
        {
            InitialCatalog = "EprCalculatorIntegrationTests"
        }.ConnectionString;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.integration.json")
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = connectionString
            })
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.LocalDevConsole()
            .CreateLogger();

        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource(Telemetry.RootScope))
            .WithMetrics(metrics => metrics.AddMeter(Telemetry.RootScope));

        services.AddHostedService<TelemetryLoggingHost>();

        services
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging(x =>
            {
                x.ClearProviders();

                // Serilog owns filtering (see appsettings.integration.json). The host app replaces
                // the ILoggerFactory entirely; here Serilog is a provider, so the Microsoft filters
                // would otherwise silently drop anything below Information before Serilog sees it.
                x.SetMinimumLevel(LogLevel.Trace);
                x.AddSerilog(Log.Logger, dispose: true);
            })
            .AddPayCalDatabase()
            .AddPayCalBlobStorage()
            .AddPayCalServices()
            .AddPayCalBackgroundServices()
            .AddPayCalRequestValidation()
            .AddDbContextFactory<ApplicationDBContext>(options => { options.UseSqlServer(connectionString); })
            .RemoveAll<CommonDataApiHttpClient>()
            .AddSingleton<FakeCommonDataApiClient>()
            .AddSingleton<ICommonDataApiClient>(sp => sp.GetRequiredService<FakeCommonDataApiClient>())
            .RemoveAll<IStorageUploadService>()
            .AddSingleton<FakeBlobStorageUploadService>()
            .AddSingleton<IStorageUploadService>(sp => sp.GetRequiredService<FakeBlobStorageUploadService>());
    }

    protected static T CreateController<T>(IServiceProvider services)
        where T : ControllerBase
    {
        var scope = services.CreateAsyncScope();

        var controller = ActivatorUtilities.CreateInstance<T>(scope.ServiceProvider);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = scope.ServiceProvider,
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.Name, "some-user"),
                        new Claim(ClaimTypes.NameIdentifier, "some-user")
                    ],
                    authenticationType: "IntegrationTest"))
            }
        };

        controller.HttpContext.Response.RegisterForDispose(scope);

        return controller;
    }

    protected static async Task WaitForCalculatorRunAsync(ApplicationDBContext db, int runId) =>
        await WaitUntilAsync(
            async () =>
            {
                var status = await db.CalculatorRuns
                    .AsNoTracking()
                    .Where(x => x.Id == runId)
                    .Select(x => x.CalculatorRunClassificationId)
                    .SingleAsync();

                return status == RunClassificationStatusIds.ERRORID
                    ? throw new Exception($"Calculator run {runId} entered Errored state.")
                    : status == RunClassificationStatusIds.UNCLASSIFIEDID;
            },
            $"Calculator run {runId} did not complete.",
            TimeSpan.FromMinutes(5));

    protected static async Task WaitForBillingRunAsync(ApplicationDBContext db, int runId) =>
        await WaitUntilAsync(
            async () =>
            {
                var status = await db.CalculatorRuns
                    .AsNoTracking()
                    .Where(x => x.Id == runId)
                    .Select(x => x.BillingRunStatus)
                    .SingleAsync();

                return status == BillingRunStatus.Errored
                    ? throw new Exception($"Billing run for calculator run {runId} entered Errored state.")
                    : status == BillingRunStatus.Completed;
            },
            $"Billing run for calculator run {runId} did not complete.");

    protected static async Task SeedCalculatorDataAsync(ApplicationDBContext db, RelativeYear relativeYear, string defaultParamsPath, string lapcapPath)
    {
        var oldDefaultSettings = await db.DefaultParameterSettings
            .Where(x => x.EffectiveTo == null && x.RelativeYear == relativeYear)
            .ToListAsync();

        oldDefaultSettings.ForEach(x => { x.EffectiveTo = DateTime.UtcNow; }); // side effecting db update

        var parameterMaster = new DefaultParameterSettingMaster
        {
            RelativeYear = relativeYear,
            EffectiveFrom = Now,
            EffectiveTo = null
        };

        db.DefaultParameterSettings.Add(parameterMaster);
        await db.SaveChangesAsync();
        db.DefaultParameterSettingDetail.AddRange(DefaultParameterSettingDetails(defaultParamsPath, parameterMaster.Id));
        await db.SaveChangesAsync();

        var oldLapcapSettings = await db.LapcapDataMaster
            .Where(x => x.EffectiveTo == null && x.RelativeYear == relativeYear)
            .ToListAsync();

        oldLapcapSettings.ForEach(x => { x.EffectiveTo = DateTime.UtcNow; }); // side effecting db update

        var lapcap = new LapcapDataMaster
        {
            RelativeYear  = relativeYear,
            EffectiveFrom = Now,
            EffectiveTo   = null
        };
        db.LapcapDataMaster.Add(lapcap);
        await db.SaveChangesAsync();
        var templates = await db.LapcapDataTemplateMaster.ToImmutableListAsync();
        db.LapcapDataDetail.AddRange(LapcapDataDetails(lapcapPath, lapcap, templates));
        await db.SaveChangesAsync();
    }

    protected static async Task SeedAcceptOrRejectProducersAsync(ApplicationDBContext db, int calculatorRunId, string modifiedBy, string csvPath)
    {
        var csvRows     = SlurpCsv(csvPath).GetRecords<dynamic>().ToImmutableList();
        var producerIds = csvRows.Select(x => int.Parse(x.producer_id)).ToHashSet();
        var dbRows      = await db.ProducerResultFileSuggestedBillingInstruction
                            .Where(x => x.CalculatorRunId == calculatorRunId && producerIds.Contains(x.ProducerId))
                            .ToListAsync();

        foreach (var dbRow in dbRows)
        {
            var csvRow = csvRows.Single(x => int.Parse(x.producer_id) == dbRow.ProducerId);

            dbRow.BillingInstructionAcceptReject = csvRow.billing_status;
            dbRow.ReasonForRejection             = csvRow.rejected_reason == "NULL" ? null : csvRow.rejected_reason;
            dbRow.LastModifiedAcceptReject       = Now;
            dbRow.LastModifiedAcceptRejectBy     = modifiedBy;
        }

        await db.SaveChangesAsync();
    }

    protected static ImmutableList<DefaultParameterSettingDetail> DefaultParameterSettingDetails(string defaultParamsPath, int masterId) =>
        SlurpCsv(defaultParamsPath)
            .GetRecords<dynamic>()
            .Select(row => (IDictionary<string, object>)row)
            .SelectMany(row =>
            {
                var paramRef = row["Parameter Unique Ref"]?.ToString();
                var rawValue = row["Parameter Value"]?.ToString();

                if (string.IsNullOrWhiteSpace(paramRef))
                    return Enumerable.Empty<DefaultParameterSettingDetail>();

                if (string.IsNullOrWhiteSpace(rawValue))
                    return Enumerable.Empty<DefaultParameterSettingDetail>();

                var valueClean = rawValue!
                    .Replace("£", "")
                    .Replace("%", "")
                    .Replace(",", "")
                    .Trim();

                return
                [
                    new DefaultParameterSettingDetail
                    {
                        DefaultParameterSettingMasterId = masterId,
                        ParameterUniqueReferenceId      = paramRef!,
                        ParameterValue                  = valueClean
                    }
                ];
            }).ToImmutableList();

    protected static ImmutableList<LapcapDataDetail> LapcapDataDetails(string lapcapPath, LapcapDataMaster master, ImmutableList<LapcapDataTemplateMaster> templates) =>
        SlurpCsv(lapcapPath)
            .GetRecords<dynamic>()
            .Select(row => new LapcapDataDetail
            {
                LapcapDataMasterId = master.Id,
                UniqueReference    = templates.Single(x => x.Material == row.material && x.Country == row.country).UniqueReference,
                TotalCost          = decimal.Parse(row.total_cost),
                LapcapDataMaster   = master // TODO make virtual?
            }).ToImmutableList();

    protected static ImmutableList<OrganisationResponse> OrganisationResponses(string organisationsPath) =>
        SlurpCsv(organisationsPath)
            .GetRecords<dynamic>()
            .Select(row => new OrganisationResponse
            {
                OrganisationId   = int.Parse(row.organisation_id),
                SubsidiaryId     = Nullable(row.subsidiary_id),
                OrganisationName = row.organisation_name,
                TradingName      = row.trading_name,
                ObligationStatus = row.obligation_status,
                SubmitterId      = row.submitter_id,
                ErrorCode        = Nullable(row.error_code),
                StatusCode       = Nullable(row.status_code),
                NumDaysObligated = Nullable((string)row.num_days_obligated, short.Parse),
                JoinerDate       = Nullable(row.joiner_date),
                LeaverDate       = Nullable(row.leaver_date),
                HasH1            = row.has_h1 == "1",
                HasH2            = row.has_h2 == "1"
            }).ToImmutableList();

    protected static ImmutableList<PomResponse> PomResponses(string pomsPath) =>
        SlurpCsv(pomsPath)
            .GetRecords<dynamic>()
            .Select(row => new PomResponse
            {
                OrganisationId              = int.Parse(row.organisation_id),
                SubsidiaryId                = Nullable(row.subsidiary_id),
                SubmissionPeriod            = row.submission_period,
                PackagingActivity           = row.packaging_activity,
                PackagingType               = row.packaging_type,
                PackagingClass              = row.packaging_class,
                PackagingMaterial           = row.packaging_material,
                PackagingMaterialWeight     = Nullable((string)row.packaging_material_weight, double.Parse),
                SubmissionPeriodDescription = row.submission_period_desc,
                SubmitterId                 = row.submitter_id,
                PackagingMaterialSubtype    = Nullable(row.packaging_material_subtype),
                RamRagRating                = Nullable(row.ram_rag_rating)
            }).ToImmutableList();

    private  static async Task WaitUntilAsync(Func<Task<bool>> condition, string failureMessage, TimeSpan? timeout = null)
    {
        var until = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(30));
        while (DateTime.UtcNow < until)
        {
            if (await condition())
                return;

            await Task.Delay(100);
        }

        throw new TimeoutException(failureMessage);
    }

    private static CsvReader SlurpCsv(string csvPath) =>
        new(new StreamReader(csvPath), new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

    private static string? Nullable(string value) =>
        value.Equals("NULL", StringComparison.OrdinalIgnoreCase)
            ? null
            : value;

    private static T? Nullable<T>(string value, Func<string, T> parser) where T : struct =>
        value.Equals("NULL", StringComparison.OrdinalIgnoreCase)
            ? null
            : parser(value);
}

[TestClass]
public static class TestAssemblyHooks
{
    [AssemblyInitialize]
    public static async Task Initialize(TestContext context)
    {
        try
        {
            await BaseIntegrationTest.InitializeAsync();
        }
        catch
        {
            // Suppressed so non-integration tests can run
        }
    }

    [AssemblyCleanup]
    public static async Task Cleanup()
    {
        try
        {
            await BaseIntegrationTest.CleanupAsync();
        }
        catch
        {
            // Suppressed so non-integration tests can run
        }
    }
}
