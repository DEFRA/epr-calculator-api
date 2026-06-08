using System.Diagnostics;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Extensions;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.CommonDataApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Testcontainers.MsSql;

namespace EPR.Calculator.Service.Function.UnitTests.IntegrationTests;

public abstract class BaseIntegrationTest
{
    protected static ServiceProvider Provider { get; private set; } = null!;

    private static readonly MsSqlContainer SqlContainer =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .WithReuse(true)
            .Build();

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
                theme: AnsiConsoleTheme.Code,
                applyThemeToRedirectedOutput: true)
            .CreateLogger();

        services
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging(x =>
            {
                x.ClearProviders();
                x.AddSerilog(Log.Logger, dispose: true);
            })
            .AddAppDependencies()
            .AddDbContextFactory<ApplicationDBContext>(options =>
            {
                options.UseSqlServer(SqlContainer.GetConnectionString());
            })
            .RemoveAll<CommonDataApiHttpClient>()
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
    public static async Task Initialize(TestContext context) =>
        await BaseIntegrationTest.InitializeAsync();

    [AssemblyCleanup]
    public static async Task Cleanup() =>
        await BaseIntegrationTest.CleanupAsync();
}