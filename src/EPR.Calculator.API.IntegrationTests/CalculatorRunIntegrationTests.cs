using System.Globalization;
using System.Security.Claims;
using CsvHelper;
using CsvHelper.Configuration;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.BackgroundService.Services.CommonDataApi;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using EPR.Calculator.API.BackgroundService.Enums;
namespace EPR.Calculator.API.IntegrationTests;

[TestCategory("IntegrationTests")]
[TestProperty("Category", "IntegrationTest")] // pipeline should use TestCategory not Category
[TestClass]
[DoNotParallelize]
public class CalculatorRunIntegrationTests : BaseIntegrationTest
{
    private static readonly DateTime Now = DateTime.UtcNow;

    [TestMethod]
    public async Task IntegrationTest_2025() => await RunTest($"test2025-{Guid.NewGuid():N}", new RelativeYear(2025), "some-user");

    [TestMethod]
    public async Task IntegrationTest_2026() => await RunTest($"test2026-{Guid.NewGuid():N}", new RelativeYear(2026), "some-user");

    private async static Task RunTest(string name, RelativeYear relativeYear, String rundBy)
    {
        await using var scope = Provider.CreateAsyncScope();
        var services = scope.ServiceProvider;

        await using var db = await services.GetRequiredService<IDbContextFactory<ApplicationDBContext>>()
            .CreateDbContextAsync();

        await SeedCalculatorData(db, relativeYear, "TestData/defaultParams.csv", "TestData/lapcap.csv");

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

        await WaitUntil(
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
            $"Calculator run {runId} did not complete.");

        await AssertCsv(
            actualContents: await ExecuteFileResult(await calculatorController.DownloadResultCsv(runId), services),
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

        await SeedAcceptOrRejectProducers(db, runId, rundBy, $"TestData/{relativeYear}-accept-or-reject-producers.csv");

        var startBillingResult = (await producerBillingFileController.ProducerBillingInstructions(runId))
            .ShouldBeOfType<ObjectResult>();

        startBillingResult.StatusCode.ShouldBe(StatusCodes.Status200OK, $"Controller returned: {JsonSerializer.Serialize(startBillingResult.Value)}");

        await WaitUntil(
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

        await AssertCsv(
            actualContents: await ExecuteFileResult(await billingFileController.DownloadBillingCsv(runId), services),
            expectedPath: $"ExpectedData/{relativeYear}-billing.csv",
            ignoreLines: [1, 2, 3, 7, 8, 9],
            label: "Billing CSV");

        await AssertCsv(
            actualContents: await ExecuteFileResult(await billingFileController.DownloadBillingJson(runId), services),
            expectedPath: $"ExpectedData/{relativeYear}-billing.json",
            ignoreLines: [3, 4, 5, 9, 11, 13, 16],
            label: "Billing JSON");
    }

    private static async Task AssertCsv(string actualContents, string expectedPath, List<int> ignoreLines, string label)
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

    private static T CreateController<T>(IServiceProvider services)
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

    private static async Task WaitUntil(Func<Task<bool>> condition, string failureMessage, TimeSpan? timeout = null)
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

    private static Task<string> ExecuteFileResult(IActionResult result, IServiceProvider services) =>
        Task.FromResult(Encoding.UTF8.GetString(result.ShouldBeOfType<FileContentResult>().FileContents));

    private static CsvReader SlurpCsv(string csvPath) =>
        new(new StreamReader(csvPath), new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

    private async static Task SeedCalculatorData(ApplicationDBContext db, RelativeYear relativeYear, String defaultParamsPath, String lapcapPath)
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

    private static async Task SeedAcceptOrRejectProducers(ApplicationDBContext db, int calculatorRunId, string modifiedBy, string csvPath)
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

    private static ImmutableList<DefaultParameterSettingDetail> DefaultParameterSettingDetails(string defaultParamsPath, int masterId) =>
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

    private static ImmutableList<LapcapDataDetail> LapcapDataDetails(string lapcapPath, LapcapDataMaster master, ImmutableList<LapcapDataTemplateMaster> templates) =>
        SlurpCsv(lapcapPath)
            .GetRecords<dynamic>()
            .Select(row => new LapcapDataDetail
            {
                LapcapDataMasterId = master.Id,
                UniqueReference    = templates.Single(x => x.Material == row.material && x.Country == row.country).UniqueReference,
                TotalCost          = decimal.Parse(row.total_cost),
                LapcapDataMaster   = master // TODO make virtual?
            }).ToImmutableList();

    private static ImmutableList<OrganisationResponse> OrganisationResponses(string organisationsPath) =>
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

    private static ImmutableList<PomResponse> PomResponses(string pomsPath) =>
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

    private static string? Nullable(string value) =>
        value.Equals("NULL", StringComparison.OrdinalIgnoreCase)
            ? null
            : value;

    private static T? Nullable<T>(string value, Func<string, T> parser) where T : struct =>
        value.Equals("NULL", StringComparison.OrdinalIgnoreCase)
            ? null
            : parser(value);
}
