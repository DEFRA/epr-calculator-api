using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Options;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EPR.Calculator.Service.Function.UnitTests;

[TestCategory(TestCategories.Core)]
[TestClass]
public class StartupTests
{
    [TestMethod]
    public void All_services_stemming_from_ServiceBusQueueTrigger_should_resolve()
    {
        var services = new ServiceCollection();

        // Manually wire up a few things since we're not calling from a real Functions Host
        services
            .AddTransient<IConfiguration>(_ => new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [$"{DatabaseOptions.SectionKey}:{nameof(DatabaseOptions.ConnectionString)}"] = "ignored",
                    [$"{BlobStorageOptions.SectionKey}:{nameof(BlobStorageOptions.ConnectionString)}"] = "UseDevelopmentStorage=true",
                    [$"{BlobStorageOptions.SectionKey}:{nameof(BlobStorageOptions.ResultFileCsvContainer)}"] = "ignored",
                    [$"{BlobStorageOptions.SectionKey}:{nameof(BlobStorageOptions.BillingFileCsvContainer)}"] = "ignored",
                    [$"{BlobStorageOptions.SectionKey}:{nameof(BlobStorageOptions.BillingFileJsonContainer)}"] = "ignored",
                    [$"{CommonDataApiHttpClientOptions.SectionKey}:{nameof(CommonDataApiHttpClientOptions.BaseUrl)}"] = "https://ignored"
                })
                .Build())
            .AddTransient<TelemetryClient>(_ => new TelemetryClient(new TelemetryConfiguration
            {
                ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000"
            }))
            .AddTransient<ApplicationDBContext>(provider => provider
                .GetRequiredService<IDbContextFactory<ApplicationDBContext>>()
                .CreateDbContext())
            .AddTransient<ServiceBusQueueTrigger>();

        var rootService = services
            .AddAppDependencies()
            .BuildServiceProvider(true)
            .GetService<ServiceBusQueueTrigger>();

        rootService.ShouldNotBeNull();
    }
}
