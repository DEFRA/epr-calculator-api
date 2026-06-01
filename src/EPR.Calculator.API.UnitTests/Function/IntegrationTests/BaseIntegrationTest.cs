using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Testcontainers.MsSql;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Extensions;
using EPR.Calculator.Service.Function.Services.CommonDataApi;
using EPR.Calculator.Service.Function.Services;

[TestClass]
public abstract class BaseIntegrationTest
{
    protected ServiceProvider Provider = null!;
    protected MsSqlContainer SqlContainer = null!;

    [TestInitialize]
    public async Task Initialize()
    {
        SqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        await SqlContainer.StartAsync();

        var services = new ServiceCollection();
        ConfigureServices(services);
        Provider = services.BuildServiceProvider();

        await CreateDatabase();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await SqlContainer.DisposeAsync();
        await Provider.DisposeAsync();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("Function/IntegrationTests/appsettings.integration.json")
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
            .AddAppDependencies(configuration)
            .AddDbContextFactory<ApplicationDBContext>(options =>
                {
                    options.UseSqlServer(SqlContainer.GetConnectionString());
                })
            .RemoveAll<CommonDataApiHttpClient>()
            .AddSingleton<FakeCommonDataApiClient>()
            .AddSingleton<ICommonDataApiClient>(sp => sp.GetRequiredService<FakeCommonDataApiClient>())
            .RemoveAll<IStorageUploadService>()
            .AddSingleton<FakeBlobStorageService>()
            .AddSingleton<IStorageUploadService>(sp => sp.GetRequiredService<FakeBlobStorageService>());
    }

    private async Task CreateDatabase()
    {
        using var scope = Provider.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDBContext>();

        await db.Database.MigrateAsync();
    }
}