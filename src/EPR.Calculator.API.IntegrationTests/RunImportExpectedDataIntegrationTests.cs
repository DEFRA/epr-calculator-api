using System.Text;
using System.Text.RegularExpressions;
using EPR.Calculator.API.BackgroundService.Features.RunImport;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data.DataTypes;
using Microsoft.Extensions.DependencyInjection;

namespace EPR.Calculator.API.IntegrationTests;

/// <summary>
///     Imports the billing CSV the calculator run integration test expects, and checks the run's billing files come out
///     as that test expects them.
/// </summary>
[TestCategory("IntegrationTests")]
[TestClass]
[DoNotParallelize]
public class RunImportExpectedDataIntegrationTests : BaseIntegrationTest
{
    [TestMethod]
    [DataRow(2025)]
    [DataRow(2026)]
    public async Task Importing_the_expected_billing_csv_gives_the_expected_billing_files(int year)
    {
        // The other tests' runs share the database, so the imported run gets an ID of its own.
        var runId = 900_000 + year;
        var billingCsv = Regex.Replace(
            await File.ReadAllTextAsync($"ExpectedData/{year}-billing.csv"),
            @"^Run Id,""\d+"",",
            $"Run Id,\"{runId}\",",
            RegexOptions.Multiline);

        await using var scope = Provider.CreateAsyncScope();
        var services = scope.ServiceProvider;

        await services.GetRequiredService<IRunImporter>().ImportAsync(
            new StringReader(billingCsv),
            $"{runId}-test{year}_Billing File_202609231710_AUTHORISED.csv",
            new RunImportDetails { Classification = RunClassification.Initial },
            CancellationToken.None);

        var billingFileController = CreateController<BillingFileController>(services);

        await CalculatorRunIntegrationTests.AssertCsv(
            actualContents: Encoding.UTF8.GetString(await ReadFileResult(await billingFileController.DownloadBillingCsv(runId, downloadFromBlobStorage: false))),
            expectedPath: $"ExpectedData/{year}-billing.csv",
            ignoreLines: [1, 2, 3, 7, 8, 9],
            label: "Billing CSV");

        await CalculatorRunIntegrationTests.AssertCsv(
            actualContents: Encoding.UTF8.GetString(await ReadFileResult(await billingFileController.DownloadBillingJson(runId, downloadFromBlobStorage: false))),
            expectedPath: $"ExpectedData/{year}-billing.json",
            ignoreLines: [3, 4, 5, 9, 11, 13, 16],
            label: "Billing JSON");
    }
}
