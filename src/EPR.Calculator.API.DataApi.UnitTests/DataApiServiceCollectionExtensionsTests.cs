using EPR.Calculator.Api.DataApi;
using EPR.Calculator.Api.DataApi.AcceptedFileSelection;
using EPR.Calculator.Api.DataApi.CommonDataApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.DataApi.UnitTests;

/// <summary>
///     Unit tests for <see cref="DataApiServiceCollectionExtensions.AddDataApi" />'s DI wiring - the
///     one place a bug here (e.g. IPayCalDataSource resolving to a different type depending on
///     loadTablesEnabled) would previously have gone unnoticed, since ProducerDataServiceTests
///     constructs its own IPayCalDataSource by hand rather than resolving it from a real container.
/// </summary>
[TestClass]
public class DataApiServiceCollectionExtensionsTests
{
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void AddDataApi_AlwaysResolvesLoadTableDataSource_RegardlessOfLoadTablesEnabled(bool loadTablesEnabled)
    {
        // A run always reads from the load tables - loadTablesEnabled only controls whether
        // ProducerDataService refreshes them first, not which IPayCalDataSource gets resolved.
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddDataApi(
            loadTableConnectionString: _ => "Server=(local);Database=Unused;Trusted_Connection=True;TrustServerCertificate=True;",
            loadTablesEnabled: _ => loadTablesEnabled);

        var provider = services.BuildServiceProvider();

        var dataSource = provider.GetRequiredService<IPayCalDataSource>();

        dataSource.ShouldBeOfType<LoadTableDataSource>();
    }

    [TestMethod]
    [DataRow(null, false)]
    [DataRow("0", false)]
    [DataRow("13", false)]
    [DataRow("1", true)]
    [DataRow("12", true)]
    public void AddDataApi_ValidatesLateSubmissionsAfterMonth(string? lateSubmissionsAfterMonth, bool expectValid)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["CalculatorDataApi:LateSubmissionsAfterMonth"] = lateSubmissionsAfterMonth })
            .Build());

        services.AddDataApi(
            loadTableConnectionString: _ => "Server=(local);Database=Unused;Trusted_Connection=True;TrustServerCertificate=True;",
            loadTablesEnabled: _ => true);

        var resolve = () => services.BuildServiceProvider().GetRequiredService<IOptions<CalculatorDataApiOptions>>().Value;

        if (expectValid)
            resolve().LateSubmissionsAfterMonth.ShouldBe(int.Parse(lateSubmissionsAfterMonth!));
        else
            Should.Throw<OptionsValidationException>(resolve);
    }
}
