using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Features.BillingRun;
using EPR.Calculator.Service.Function.Features.BillingRun.Contexts;
using EPR.Calculator.Service.Function.Features.CalculatorRun;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.Services.CommonDataApi;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EPR.Calculator.Service.Function.UnitTests.IntegrationTests;

[TestCategory("IntegrationTest")]
[TestProperty("Category", "IntegrationTest")] // pipeline should use TestCategory not Category
[TestClass]
[DoNotParallelize]
public class CalculatorRunIntegrationTests : BaseIntegrationTest
{
    private static readonly DateTime Now = DateTime.UtcNow;

    [TestMethod]
    public async Task IntegrationTest_2025() => await RunTest("test2025", new RelativeYear(2025), "some-user");

    [TestMethod]
    public async Task IntegrationTest_2026() => await RunTest("test2026", new RelativeYear(2026), "some-user");

    private async Task RunTest(string name, RelativeYear relativeYear, String rundBy)
    {
        await using var db = await Provider
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<IDbContextFactory<ApplicationDBContext>>()
            .CreateDbContextAsync();

        var calculatorRunId = await SeedCalculatorRun(db, name, relativeYear, "TestData/defaultParams.csv", "TestData/lapcap.csv");

        var fakeCommonDataApi                   = Provider.GetRequiredService<FakeCommonDataApiClient>();
        fakeCommonDataApi.OrganisationResponses = OrganisationResponses($"TestData/{relativeYear.Value}-organisation-data.csv");
        fakeCommonDataApi.PomResponses          = PomResponses($"TestData/{relativeYear.Value}-pom-data.csv");

        var fakeBlobUploadStorage = Provider.GetRequiredService<FakeBlobStorageUploadService>();

        var calculatorRunResult = await PerformCalculatorRun(calculatorRunId, rundBy);
        {
            var contents       = fakeBlobUploadStorage.Get(calculatorRunResult.ExportResult.CsvMetadata.FileName);
            var actualLines   = string.Join(Environment.NewLine, contents.Split(Environment.NewLine, StringSplitOptions.None                                   )).Trim().Split(Environment.NewLine);
            var expectedLines = string.Join(Environment.NewLine, await File.ReadAllLinesAsync($"ExpectedData/{relativeYear.Value}-results.csv")).Trim().Split(Environment.NewLine);

            actualLines.Length.ShouldBe(expectedLines.Length, $"Results CSV mismatch: {DisplayFullContents(contents)}");

            var ignoreLines = new List<int> {2, 3, 6, 7, 8}; // Ignore run id and date fields
            AssertLines(actualLines, expectedLines, ignoreLines, "Results CSV", contents);
        }

        await SeedAcceptOrRejectProducers(db, calculatorRunId, rundBy, $"TestData/{relativeYear.Value}-accept-or-reject-producers.csv");

        var billingRunResult = await PerformBillingRun(db, calculatorRunId, rundBy);
        {
            var contents      = fakeBlobUploadStorage.Get(billingRunResult.ExportResult.CsvMetadata.FileName);
            var actualLines   = string.Join(Environment.NewLine, contents.Split(Environment.NewLine, StringSplitOptions.None                                     )).Trim().Split(Environment.NewLine);
            var expectedLines = string.Join(Environment.NewLine, await File.ReadAllLinesAsync($"ExpectedData/{relativeYear.Value}-billing.csv")).Trim().Split(Environment.NewLine);

            actualLines.Length.ShouldBe(expectedLines.Length, $"Billing CSV mismatch: {DisplayFullContents(contents)}");

            var ignoreLines = new List<int> {2, 3, 6, 7, 8}; // Ignore run id and date fields
            AssertLines(actualLines, expectedLines, ignoreLines, "Billing CSV", contents);
        }
        {   // TODO sort json fields before comparison?
            var contents      = fakeBlobUploadStorage.Get(billingRunResult.ExportResult.JsonMetadata.BillingJsonFileName!);
            var actualLines   = string.Join(Environment.NewLine, contents.Split(Environment.NewLine, StringSplitOptions.None                                    )).Trim().Split(Environment.NewLine);
            var expectedLines = string.Join(Environment.NewLine, await File.ReadAllLinesAsync($"ExpectedData/{relativeYear.Value}-billing.json")).Trim().Split(Environment.NewLine);

            actualLines.Length.ShouldBe(expectedLines.Length, $"Billing JSON mismatch: {DisplayFullContents(contents)}");

            var ignoreLines = new List<int> {4, 5, 9, 11, 13, 16}; // Ignore run id and date fields
            AssertLines(actualLines, expectedLines, ignoreLines, "Billing JSON", contents);
        }
    }

    private static void AssertLines(string[] actualLines, string[] expectedLines, List<int> ignoreLines, string label, string contents)
    {
        try
        {
            for (var i = 0; i < actualLines.Length; i++)
            {
                if (!ignoreLines.Contains(i + 1)) actualLines[i].ShouldBe(expectedLines[i], $"{label} mismatch at line {i + 1}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"{ex.Message}; {DisplayFullContents(contents)}", ex); // Needs to be lazy as dramatically increases test time if added to ShouldBe message
        }
    }

    private static string DisplayFullContents(string contents) =>
        $"Full contents:\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\n{contents}<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<";


    private static async Task<CalculatorRunResult> PerformCalculatorRun(int runId, string user)
    {
        var runContext = await Provider.GetRequiredService<ICalculatorRunContextBuilder>().Build(runId, user, CancellationToken.None);
        var runResult = await Provider.GetRequiredService<ICalculatorRunProcessor>().Process(runContext, CancellationToken.None);
        runResult.Succeeded.ShouldBeTrue();
        return (CalculatorRunResult) runResult;
    }

    private static async Task<BillingRunResult> PerformBillingRun(ApplicationDBContext dbContext, int runId, string user)
    {
        // Simulates the billing run being started by a user in the FE
        var calcRun = await dbContext.CalculatorRuns.SingleAsync(r => r.Id == runId);
        calcRun.CalculatorRunClassificationId = RunClassificationStatusIds.INITIALRUNID;
        calcRun.IsBillingFileGenerating = true;
        await dbContext.SaveChangesAsync();

        var runContext = await Provider.GetRequiredService<IBillingRunContextBuilder>().Build(runId, user, CancellationToken.None);
        var runResult = await Provider.GetRequiredService<IBillingRunProcessor>().Process(runContext, CancellationToken.None);
        runResult.Succeeded.ShouldBeTrue();
        return (BillingRunResult) runResult;
    }

    private static CsvReader SlurpCsv(string csvPath) =>
        new(new StreamReader(csvPath), new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

    private async Task<int> SeedCalculatorRun(ApplicationDBContext db, String name, RelativeYear relativeYear, String defaultParamsPath, String lapcapPath)
    {
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

        var run = new CalculatorRun
        {
            Name                            = name,
            RelativeYear                    = relativeYear,
            CreatedBy                       = "some-user",
            CreatedAt                       = Now,
            CalculatorRunClassificationId   = (int)RunClassification.RUNNING,
            DefaultParameterSettingMasterId = parameterMaster.Id,
            LapcapDataMasterId              = lapcap.Id,
            IsBillingFileGenerating         = null
        };
        db.CalculatorRuns.Add(run);
        await db.SaveChangesAsync();

        return run.Id;
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

                if (paramRef.StartsWith("Parameter upload version"))
                    return Enumerable.Empty<DefaultParameterSettingDetail>();

                if (string.IsNullOrWhiteSpace(rawValue))
                    return Enumerable.Empty<DefaultParameterSettingDetail>();

                var valueClean = rawValue!
                    .Replace("£", "")
                    .Replace("%", "")
                    .Replace(",", "")
                    .Trim();

                if (!decimal.TryParse(valueClean, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value))
                    throw new Exception($"Failed to parse value '{rawValue}' -> '{valueClean}' for {paramRef}");

                return
                [
                    new DefaultParameterSettingDetail
                    {
                        DefaultParameterSettingMasterId = masterId,
                        ParameterUniqueReferenceId      = paramRef!,
                        ParameterValue                  = value
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
