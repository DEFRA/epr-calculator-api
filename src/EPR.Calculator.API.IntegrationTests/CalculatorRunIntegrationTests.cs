using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
namespace EPR.Calculator.API.IntegrationTests;

[TestCategory("IntegrationTests")]
[TestProperty("Category", "IntegrationTest")] // pipeline should use TestCategory not Category
[TestClass]
[DoNotParallelize]
public class CalculatorRunIntegrationTests : BaseIntegrationTest
{
    [TestMethod]
    public async Task IntegrationTest_2025() =>
        await RunTestAsync($"test2025-{Guid.NewGuid():N}", new RelativeYear(2025), "some-user");

    [TestMethod]
    public async Task IntegrationTest_2026() =>
        await RunTestAsync($"test2026-{Guid.NewGuid():N}", new RelativeYear(2026), "some-user");

    private static async Task RunTestAsync(string name, RelativeYear relativeYear, string rundBy)
    {
        await using var scope = Provider.CreateAsyncScope();
        var services = scope.ServiceProvider;

        await using var db = await services.GetRequiredService<IDbContextFactory<ApplicationDBContext>>()
            .CreateDbContextAsync();

        await SeedCalculatorDataAsync(db, relativeYear, "TestData/defaultParams.csv", "TestData/lapcap.csv");

        var fakeCommonDataApi                   = Provider.GetRequiredService<FakeCommonDataApiClient>();
        fakeCommonDataApi.OrganisationResponses = OrganisationResponses($"TestData/{relativeYear}-organisation-data.csv");
        fakeCommonDataApi.PomResponses          = PomResponses($"TestData/{relativeYear}-pom-data.csv");

        var calculatorController          = CreateController<CalculatorController>(services);
        var calculatorNewController       = CreateController<CalculatorNewController>(services);
        var producerBillingFileController = CreateController<ProducerBillingFileController>(services);
        var billingFileController         = CreateController<BillingFileController>(services);

        // Results
        var createRunResult = (await calculatorController.Create(new CreateCalculatorRunDto {CalculatorRunName = name, RelativeYear = relativeYear}))
            .ShouldBeOfType<ObjectResult>();

        createRunResult.StatusCode.ShouldBe(StatusCodes.Status202Accepted, $"Controller returned: {JsonSerializer.Serialize(createRunResult.Value)}");

        var runId = await db.CalculatorRuns
            .Where(x => x.Name == name)
            .Select(x => x.Id)
            .SingleAsync();

        await WaitForCalculatorRunAsync(db, runId);

        await AssertFile(
            actualContents: GetFileContentAsString(await calculatorController.DownloadResultCsv(runId), expectUtf8Bom: true),
            expectedPath: $"ExpectedData/{relativeYear}-results.csv",
            ignoreLines: [1, 2, 3, 7, 8, 9],
            label: "Results CSV");

        // Billing
        foreach (var run in await db.CalculatorRuns
                                    .Where(x =>
                                        x.RelativeYear == relativeYear &&
                                        x.CalculatorRunClassificationId == RunClassificationStatusIds.INITIALRUNID)
                                    .ToListAsync())
        {
            run.CalculatorRunClassificationId = RunClassificationStatusIds.DELETEDID;
        }
        await db.SaveChangesAsync();

        var setBillingClassificationResult = await calculatorNewController.PutCalculatorRunStatus(new CalculatorRunStatusUpdateDto {
                RunId = runId,
                ClassificationId = RunClassificationStatusIds.INITIALRUNID
            });

        if (setBillingClassificationResult is StatusCodeResult statusCodeResult)
        {
            statusCodeResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
        }
        else
        {
            throw new Exception($"Controller returned: {JsonSerializer.Serialize(setBillingClassificationResult.ShouldBeOfType<ObjectResult>().Value)}");
        }

        await SeedAcceptOrRejectProducersAsync(db, runId, rundBy, $"TestData/{relativeYear}-accept-or-reject-producers.csv");

        var startBillingResult = (await producerBillingFileController.ProducerBillingInstructions(runId))
            .ShouldBeOfType<ObjectResult>();

        startBillingResult.StatusCode.ShouldBe(StatusCodes.Status200OK, $"Controller returned: {JsonSerializer.Serialize(startBillingResult.Value)}");

        await WaitForBillingRunAsync(db, runId);

        await AssertFile(
            actualContents: GetFileContentAsString(await billingFileController.DownloadBillingCsv(runId), expectUtf8Bom: true),
            expectedPath: $"ExpectedData/{relativeYear}-billing.csv",
            ignoreLines: [1, 2, 3, 7, 8, 9],
            label: "Billing CSV");

        await AssertFile(
            actualContents: GetFileContentAsString(await billingFileController.DownloadBillingJson(runId), expectUtf8Bom: false),
            expectedPath: $"ExpectedData/{relativeYear}-billing.json",
            ignoreLines: [3, 4, 5, 9, 11, 13, 16],
            label: "Billing JSON");
    }

    private static async Task AssertFile(string actualContents, string expectedPath, List<int> ignoreLines, string label)
    {
        static string DisplayFullContents(string contents) =>
            $"Full contents:\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\n{contents}<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<";

        var actualLines = actualContents
            .Trim()
            .Split(Environment.NewLine);

        var expectedLines = string.Join(Environment.NewLine, await File.ReadAllLinesAsync(expectedPath))
            .Trim()
            .Split(Environment.NewLine);

        actualLines.Length.ShouldBe(
            expectedLines.Length,
            $"{label} mismatch: {DisplayFullContents(actualContents)}");

        try
        {
            for (var i = 0; i < actualLines.Length; i++)
            {
                if (!ignoreLines.Contains(i + 1)) actualLines[i].ShouldBe(expectedLines[i], $"{label} mismatch at line {i + 1}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"{ex.Message}; {DisplayFullContents(actualContents)}", ex); // Needs to be lazy as dramatically increases test time if added to ShouldBe message
        }
    }

    private static string GetFileContentAsString(IActionResult result, bool expectUtf8Bom)
    {
        var fileContents = result.ShouldBeOfType<FileContentResult>().FileContents;

        if (expectUtf8Bom)
        {
            fileContents.Take(3).ShouldBe([0xEF, 0xBB, 0xBF]);
        }
        return Encoding.UTF8.GetString(fileContents);
    }
}
