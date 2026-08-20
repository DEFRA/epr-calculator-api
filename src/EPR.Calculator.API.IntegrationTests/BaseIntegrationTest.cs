using EPR.Calculator.API.App;
using EPR.Calculator.API.BackgroundService.Logging;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Extensions;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.Services.CommonDataApi;
using EPR.Calculator.API.BackgroundService.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Serilog;
using Testcontainers.MsSql;

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

        using var scope = Provider.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDBContext>>();
        await using var db = await factory.CreateDbContextAsync();

        await db.Database.MigrateAsync();
    }

    public static async Task CleanupAsync()
    {
        await Provider.DisposeAsync();
        Log.CloseAndFlush();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.integration.json")
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = SqlContainer.GetConnectionString()
            })
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                DevConsole.Logger())
            .CreateLogger();

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
            .AddDbContextFactory<ApplicationDBContext>(options => { options.UseSqlServer(SqlContainer.GetConnectionString()); })
            .RemoveAll<CommonDataApiHttpClient>()
            .AddSingleton<ITelemetryClient, LoggerTelemetryClient>()
            .AddSingleton<FakeCommonDataApiClient>()
            .AddSingleton<ICommonDataApiClient>(sp => sp.GetRequiredService<FakeCommonDataApiClient>())
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
