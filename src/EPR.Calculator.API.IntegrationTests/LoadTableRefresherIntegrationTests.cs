using EPR.Calculator.Api.DataApi.CommonDataApi;
using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;
using EPR.Calculator.Api.DataApi.CommonDataApi.LoadTables;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EPR.Calculator.API.IntegrationTests;

[TestCategory("IntegrationTests")]
[TestProperty("Category", "IntegrationTest")]
[TestClass]
[DoNotParallelize]
public class LoadTableRefresherIntegrationTests : BaseIntegrationTest
{
    private static readonly string AppPassword = $"Aa1!{Guid.NewGuid():N}";

    [TestMethod]
    public async Task RefreshAsync_WhenTheAppIsGrantedTheProcedures_TruncatesBothTables()
    {
        var factory = await CreateAppUserContextFactoryAsync("load_table_refresher_test_granted", grantExecute: true);

        await RefreshTwiceAsync(factory);

        // TRUNCATE resets the identity, so the second refresh's rows start from 1 again.
        await using var db = await factory.CreateDbContextAsync();
        (await db.LoadOrganisations.Select(o => o.Id).OrderBy(id => id).ToListAsync()).ShouldBe([1, 2]);
        (await db.LoadPoms.Select(p => p.Id).OrderBy(id => id).ToListAsync()).ShouldBe([1, 2, 3]);
    }

    [TestMethod]
    public async Task RefreshAsync_WhenTheAppIsNotGrantedTheProcedures_DeletesFromBothTables()
    {
        var factory = await CreateAppUserContextFactoryAsync("load_table_refresher_test_not_granted", grantExecute: false);

        await RefreshTwiceAsync(factory);

        // DELETE keeps the identity, so the second refresh's rows carry on from the first's.
        await using var db = await factory.CreateDbContextAsync();
        (await db.LoadOrganisations.Select(o => o.Id).OrderBy(id => id).ToListAsync()).ShouldBe([3, 4]);
        (await db.LoadPoms.Select(p => p.Id).OrderBy(id => id).ToListAsync()).ShouldBe([4, 5, 6]);
    }

    private static async Task RefreshTwiceAsync(IDbContextFactory<DataApiLoadContext> factory)
    {
        var refresher = new LoadTableRefresher(
            new FakeStreamOrganisationsRequestHandler { Organisations = [Organisation(1), Organisation(2)] },
            new FakeStreamPomsRequestHandler { Poms = () => [Pom(1), Pom(2), Pom(3)] },
            factory,
            Microsoft.Extensions.Options.Options.Create(new DataApiLoadOptions()));

        await ResetLoadTablesAsync();
        await refresher.RefreshAsync(2026);
        await refresher.RefreshAsync(2026);
    }

    private static string AdminConnectionString => Provider.GetRequiredService<IConfiguration>()["Database:ConnectionString"]!;

    private static async Task<IDbContextFactory<DataApiLoadContext>> CreateAppUserContextFactoryAsync(string login, bool grantExecute)
    {
        // Mirrors the API's identity: read and write through roles, EXECUTE only if the migration granted it, and no ALTER.
        var grants = grantExecute
            ? $"GRANT EXECUTE ON dbo.TruncateDataApiLoadOrganisations TO {login}; GRANT EXECUTE ON dbo.TruncateDataApiLoadPoms TO {login};"
            : "";

        await ExecuteAsAdminAsync(
            $"""
             IF SUSER_ID(N'{login}') IS NULL CREATE LOGIN {login} WITH PASSWORD = N'{AppPassword}', CHECK_POLICY = OFF; ELSE ALTER LOGIN {login} WITH PASSWORD = N'{AppPassword}';
             IF USER_ID(N'{login}') IS NULL CREATE USER {login} FOR LOGIN {login};
             ALTER ROLE db_datareader ADD MEMBER {login};
             ALTER ROLE db_datawriter ADD MEMBER {login};
             {grants}
             """);

        var connectionString = new SqlConnectionStringBuilder(AdminConnectionString) { UserID = login, Password = AppPassword }.ConnectionString;

        return new ServiceCollection()
            .AddDbContextFactory<DataApiLoadContext>(options => options.UseSqlServer(connectionString))
            .BuildServiceProvider()
            .GetRequiredService<IDbContextFactory<DataApiLoadContext>>();
    }

    private static Task ResetLoadTablesAsync() =>
        ExecuteAsAdminAsync("TRUNCATE TABLE dbo.data_api_load_organisations; TRUNCATE TABLE dbo.data_api_load_poms;");

    private static async Task ExecuteAsAdminAsync(string sql)
    {
        await using var connection = new SqlConnection(AdminConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static PayCalOrganisation Organisation(int id) => new()
    {
        OrganisationId = id,
        OrganisationName = $"Organisation {id}",
        SubmissionPeriodYear = 2025,
        OrganisationSize = "L"
    };

    private static PayCalPom Pom(int id) => new()
    {
        OrganisationId = id,
        SubmissionPeriod = "2025-H1"
    };
}
