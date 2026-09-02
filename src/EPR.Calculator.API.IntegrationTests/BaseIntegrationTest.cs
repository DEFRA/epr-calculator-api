using EPR.Calculator.API.App;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.Telemetry.Internals;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Extensions;
using EPR.CommonDataService.DataApi.CommonDataApi;
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

        await db.Database.MigrateAsync();
    }

    public static async Task CleanupAsync()
    {
        foreach (var hostedService in Provider.GetServices<IHostedService>())
            await hostedService.StopAsync(CancellationToken.None);

        await Provider.DisposeAsync();
        await Log.CloseAndFlushAsync();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.integration.json")
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = SqlContainer.GetConnectionString(),
                ["Synapse:ConnectionString"] = SqlContainer.GetConnectionString()
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
            .AddPayCalDataApi()
            .AddPayCalBlobStorage()
            .AddPayCalServices()
            .AddPayCalBackgroundServices()
            .AddDbContextFactory<ApplicationDBContext>(options => { options.UseSqlServer(SqlContainer.GetConnectionString()); })
            .RemoveAll<IStreamOrganisationsRequestHandler>()
            .AddSingleton<FakeStreamOrganisationsRequestHandler>()
            .AddSingleton<IStreamOrganisationsRequestHandler>(sp => sp.GetRequiredService<FakeStreamOrganisationsRequestHandler>())
            .RemoveAll<IStreamPomsRequestHandler>()
            .AddSingleton<FakeStreamPomsRequestHandler>()
            .AddSingleton<IStreamPomsRequestHandler>(sp => sp.GetRequiredService<FakeStreamPomsRequestHandler>())
            .RemoveAll<IStorageUploadService>()
            .AddSingleton<FakeBlobStorageUploadService>()
            .AddSingleton<IStorageUploadService>(sp => sp.GetRequiredService<FakeBlobStorageUploadService>());
    }
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
