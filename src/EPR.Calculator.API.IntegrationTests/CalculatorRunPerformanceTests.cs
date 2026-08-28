using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EPR.Calculator.API.IntegrationTests;

public class CalculatorRunPerformanceTests : BaseIntegrationTest
{
    private const int NumberOfRuns                         = 5;
    private const int NumberOfOrganisations                = 4_000;
    private const int PartialPercent                       = 1;
    private const int ProjectedRamH1Percent                = 10;
    private const int ProjectedRamH2Percent                = 10;
    private const int OrganisationsWithSubsidiariesPercent = 15;
    private const int MaxSubsidiaries                      = 5;
    private const int OrganisationsWithCwPercent           = 25;

    enum OrganisationScenario
    {
        Standard,
        Partial,
        ProjectedRamH1,
        ProjectedRamH2,
        MissingRegistration,
        Error
    }

    public static async Task RunAsync()
    {
        var relativeYear      = new RelativeYear(2026);
        var outputDirectory   = Path.Combine(AppContext.BaseDirectory, "PerformanceResults");
        var organisationPath  = GenerateOrganisationData(relativeYear);
        var pomPath           = GeneratePomData(relativeYear);
        var generatedPomCount = File.ReadLines(pomPath).Skip(1).Count();

        await using var scope = Provider.CreateAsyncScope();
        var services = scope.ServiceProvider;

        await using var db = await services.GetRequiredService<IDbContextFactory<ApplicationDBContext>>()
            .CreateDbContextAsync();

        await SeedCalculatorDataAsync(db, relativeYear, "TestData/defaultParams.csv", "TestData/lapcap.csv");

        var fakeCommonDataApi                   = Provider.GetRequiredService<FakeCommonDataApiClient>();
        fakeCommonDataApi.OrganisationResponses = OrganisationResponses(organisationPath);
        fakeCommonDataApi.PomResponses          = PomResponses(pomPath);

        var calculatorController          = CreateController<CalculatorController>(services);
        var calculatorNewController       = CreateController<CalculatorNewController>(services);
        var producerBillingFileController = CreateController<ProducerBillingFileController>(services);
        var billingFileController         = CreateController<BillingFileController>(services);

        var calculatorTimings  = new List<TimeSpan>();
        var resultsCsvTimings  = new List<TimeSpan>();
        var billingTimings     = new List<TimeSpan>();
        var billingCsvTimings  = new List<TimeSpan>();
        var billingJsonTimings = new List<TimeSpan>();

        Console.WriteLine($"Performance test data: {fakeCommonDataApi.OrganisationResponses.Count:N0} organisations, {fakeCommonDataApi.PomResponses.Count:N0} POMs");
        Console.WriteLine($"Database: {db.Database.GetConnectionString()}");

        Directory.CreateDirectory(outputDirectory);

        for (var i = 0; i < NumberOfRuns; i++)
        {
            var name = $"performance-{relativeYear}-{Guid.NewGuid():N}";

            // Results
            var stopwatch = Stopwatch.StartNew();

            var createRunResult = (await calculatorController.Create(new CreateCalculatorRunDto
            {
                CalculatorRunName = name,
                RelativeYear = relativeYear
            })).ShouldBeOfType<ObjectResult>();

            createRunResult.StatusCode.ShouldBe(StatusCodes.Status202Accepted, $"Controller returned: {JsonSerializer.Serialize(createRunResult.Value)}");

            var runId = await db.CalculatorRuns
                .Where(x => x.Name == name)
                .Select(x => x.Id)
                .SingleAsync();

            await WaitForCalculatorRunAsync(db, runId);

            stopwatch.Stop();
            calculatorTimings.Add(stopwatch.Elapsed);

            // Results CSV
            stopwatch.Restart();
            SaveFileResult(await calculatorController.DownloadResultCsv(runId), Path.Combine(outputDirectory, $"run-{i + 1}-results.csv"));
            stopwatch.Stop();
            resultsCsvTimings.Add(stopwatch.Elapsed);

            // Billing
            stopwatch.Restart();

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

            await SeedAllProducersAsAcceptedAsync(db, runId, "some-user", fakeCommonDataApi.OrganisationResponses.Select(x =>x.OrganisationId!.Value));
            var startBillingResult = (await producerBillingFileController.ProducerBillingInstructions(runId)).ShouldBeOfType<ObjectResult>();
            startBillingResult.StatusCode.ShouldBe(StatusCodes.Status200OK, $"Controller returned: {JsonSerializer.Serialize(startBillingResult.Value)}");
            await WaitForBillingRunAsync(db, runId);

            stopwatch.Stop();
            billingTimings.Add(stopwatch.Elapsed);

            // Billing CSV
            stopwatch.Restart();
            SaveFileResult(await billingFileController.DownloadBillingCsv(runId), Path.Combine(outputDirectory, $"run-{i + 1}-billing.csv"));
            stopwatch.Stop();
            billingCsvTimings.Add(stopwatch.Elapsed);

            // Billing JSON
            stopwatch.Restart();
            SaveFileResult(await billingFileController.DownloadBillingJson(runId), Path.Combine(outputDirectory, $"run-{i + 1}-billing.json"));
            stopwatch.Stop();
            billingJsonTimings.Add(stopwatch.Elapsed);
        }

        Console.WriteLine();
        Console.WriteLine($"Performance test data: {fakeCommonDataApi.OrganisationResponses.Count:N0} organisations, {fakeCommonDataApi.PomResponses.Count:N0} POMs");
        Console.WriteLine($"Database: {db.Database.GetConnectionString()}");
        Console.WriteLine($"Output: {outputDirectory}");
        Console.WriteLine();

        static string   Format (TimeSpan value)         => $"{value.TotalSeconds:0.00}s";
        static TimeSpan Average(List<TimeSpan> timings) => TimeSpan.FromTicks(timings.Sum(x => x.Ticks) / timings.Count);

        Console.WriteLine("Timings by run");
        Console.WriteLine("--------------");
        Console.WriteLine(
            $"{"Run"         , -9}" +
            $"{"Results"     , 10}" +
            $"{"Results CSV" , 15}" +
            $"{"Billing"     , 10}" +
            $"{"Billing CSV" , 15}" +
            $"{"Billing JSON", 15}");
        Console.WriteLine(new string('-', 74));
        for (var i = 0; i < NumberOfRuns; i++)
        {
            Console.WriteLine(
                $"{i + 1                        , -9}" +
                $"{Format(calculatorTimings[i]) , 10}" +
                $"{Format(resultsCsvTimings[i]) , 15}" +
                $"{Format(billingTimings[i])    , 10}" +
                $"{Format(billingCsvTimings[i]) , 15}" +
                $"{Format(billingJsonTimings[i]), 15}");
        }
        Console.WriteLine(new string('-', 74));
        Console.WriteLine(
            $"{"Average"                          , -9}" +
            $"{Format(Average(calculatorTimings)) , 10}" +
            $"{Format(Average(resultsCsvTimings)) , 15}" +
            $"{Format(Average(billingTimings))    , 10}" +
            $"{Format(Average(billingCsvTimings)) , 15}" +
            $"{Format(Average(billingJsonTimings)), 15}");
    }

    private static OrganisationScenario GetOrganisationScenario(int index)
    {
        var percent = (double)index / NumberOfOrganisations * 100;

        return index switch
        {
            0 => OrganisationScenario.MissingRegistration,
            1 => OrganisationScenario.Error,
            _ => percent switch
                {
                    _ when percent < PartialPercent                                                 => OrganisationScenario.Partial,
                    _ when percent < PartialPercent + ProjectedRamH1Percent                         => OrganisationScenario.ProjectedRamH1,
                    _ when percent < PartialPercent + ProjectedRamH1Percent + ProjectedRamH2Percent => OrganisationScenario.ProjectedRamH2,
                    _ => OrganisationScenario.Standard
                }
        };
    }

    private static string GenerateOrganisationData(RelativeYear relativeYear)
    {
        static (string ObligationStatus, Guid SubmitterId, string? ErrorCode, int? NumDaysObligated, string? StatusCode, string? JoinerDate) GetOrganisationValues(OrganisationScenario scenario, Guid submitterId) =>
            scenario switch
            {
                OrganisationScenario.Partial             => ("O", submitterId         , null , 233 , "02", "22-05-2025"),
                OrganisationScenario.MissingRegistration => ("O", CreateSubmitterId(0), null , null, null, null),
                OrganisationScenario.Error               => ("E", submitterId         , "111", null, null, null),
                _                                        => ("O", submitterId         , null , null, null, null)
            };

        var path   = Path.Combine(AppContext.BaseDirectory, "TestData", $"performance-{relativeYear}-organisation-data.csv");
        var output = new StringBuilder();
        output.AppendLine("organisation_id,subsidiary_id,organisation_name,trading_name,obligation_status,submitter_id,error_code,num_days_obligated,status_code,joiner_date,leaver_date,has_h1,has_h2");

        for (var i = 0; i < NumberOfOrganisations; i++)
        {
            var organisationId  = GetOrganisationId(i);
            var submitterId     = CreateSubmitterId(i);
            var scenario        = GetOrganisationScenario(i);
            var subsidiaryCount = GetSubsidiaryCount(i);
            var subsidiaryIds   = subsidiaryCount == 0
                ? [null]
                : Enumerable.Range(1, subsidiaryCount)
                    .Select(x => (int?)GetSubsidiaryId(i, x))
                    .ToArray();

            var (obligationStatus, scenarioSubmitterId, errorCode, numDaysObligated, statusCode, joinerDate) = GetOrganisationValues(scenario, submitterId);

            foreach (var subsidiaryId in subsidiaryIds)
            {
                var organisationName = subsidiaryId is null
                    ? $"Performance Test {organisationId}"
                    : $"Performance Test {organisationId} Subsidiary {subsidiaryId}";

                output.AppendLine(
                    $"{organisationId}," +
                    $"{subsidiaryId?.ToString() ?? "NULL"}," +
                    $"{organisationName},," +
                    $"{obligationStatus}," +
                    $"{scenarioSubmitterId}," +
                    $"{errorCode ?? "NULL"}," +
                    $"{numDaysObligated?.ToString() ?? "NULL"}," +
                    $"{statusCode ?? "NULL"}," +
                    $"{joinerDate ?? "NULL"}," +
                    $"NULL,1,1");
            }
        }

        File.WriteAllText(path, output.ToString());
        return path;
    }

    private static string GeneratePomData(RelativeYear relativeYear)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", $"performance-{relativeYear}-pom-data.csv");
        var output = new StringBuilder();
        output.AppendLine("organisation_id,subsidiary_id,submission_period,packaging_activity,packaging_type,packaging_class,packaging_material,packaging_material_weight,submission_period_desc,submitter_id,packaging_material_subtype,ram_rag_rating");

        var materials = new[]
        {
            MaterialCodes.Aluminium,
            MaterialCodes.FibreComposite,
            MaterialCodes.Glass,
            MaterialCodes.PaperOrCard,
            MaterialCodes.Plastic,
            MaterialCodes.Steel,
            MaterialCodes.Wood,
            MaterialCodes.OtherMaterials
        };

        for (var i = 0; i < NumberOfOrganisations; i++)
        {
            var organisationId = GetOrganisationId(i);
            var submitterId    = CreateSubmitterId(i);
            var random         = new Random(i);
            var scenario       = GetOrganisationScenario(i);
            var projectedRamH1 = scenario == OrganisationScenario.ProjectedRamH1;
            var projectedRamH2 = scenario == OrganisationScenario.ProjectedRamH2;
            var materialCount  = (i % 20) switch
            {
                < 10 => 2, // 50%
                < 17 => 3, // 35%
                _    => 4  // 15%
            };

            var selectedMaterials = materials
                .OrderBy(_ => random.Next())
                .Take(materialCount)
                .ToArray();

            var subsidiaryCount = GetSubsidiaryCount(i);
            var subsidiaryIds   = subsidiaryCount == 0
                ? [null]
                : Enumerable.Range(1, subsidiaryCount)
                    .Select(x => (int?)GetSubsidiaryId(i, x))
                    .ToArray();

            foreach (var subsidiaryId in subsidiaryIds)
            {
                AddPomsForPeriod(output, organisationId, subsidiaryId, "2025-H1", "January to June 2025" , submitterId, selectedMaterials, random, projectedRamH1);
                AddPomsForPeriod(output, organisationId, subsidiaryId, "2025-H2", "July to December 2025", submitterId, selectedMaterials, random, projectedRamH2);
            }
        }

        File.WriteAllText(path, output.ToString());
        return path;
    }

    private static void AddPomsForPeriod(StringBuilder output, int organisationId, int? subsidiaryId, string period, string description, Guid submitterId, string[] materials, Random random, bool projectedRam)
    {
        void AddPom(StringBuilder output, string material, string packagingType, bool projectedRam)
        {
            var weight       = random.Next(10_000, 500_001);
            var ramRagRating = projectedRam ? null : "R";

            output.AppendLine(
                string.Join(",",
                    organisationId,
                    subsidiaryId?.ToString() ?? "NULL",
                    period,
                    "SO",
                    packagingType,
                    "O1",
                    material,
                    weight.ToString(CultureInfo.InvariantCulture),
                    description,
                    submitterId,
                    "NULL",
                    ramRagRating ?? "NULL"));
        }

        foreach (var material in materials)
        {
            AddPom(output, material, "HH", projectedRam);

            var organisationPercent = (double)(organisationId - 1_000_000) / NumberOfOrganisations * 100;
            if (organisationPercent < OrganisationsWithCwPercent)
            {
                AddPom(output, material, "CW", projectedRam);
            }
        }
    }

    private static int GetOrganisationId(int index) =>
        1_000_000 + index;

    private static int GetSubsidiaryCount(int index) =>
        index % 100 < OrganisationsWithSubsidiariesPercent
            ? index % MaxSubsidiaries + 1
            : 0;

    private static int GetSubsidiaryId(int index, int subsidiaryNumber) =>
        4_000_000 + (index * MaxSubsidiaries) + subsidiaryNumber;

    private static Guid CreateSubmitterId(int index)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(index).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    private static async Task SeedAllProducersAsAcceptedAsync(ApplicationDBContext db, int calculatorRunId, string modifiedBy, IEnumerable<int> producerIds)
    {
        var producerIdSet = producerIds.ToHashSet();

        var dbRows = await db.ProducerResultFileSuggestedBillingInstruction
            .Where(x =>
                x.CalculatorRunId == calculatorRunId &&
                producerIdSet.Contains(x.ProducerId))
            .ToListAsync();

        foreach (var dbRow in dbRows)
        {
            dbRow.BillingInstructionAcceptReject = "Accepted";
            dbRow.ReasonForRejection             = null;
            dbRow.LastModifiedAcceptReject       = Now;
            dbRow.LastModifiedAcceptRejectBy     = modifiedBy;
        }

        await db.SaveChangesAsync();
    }

    private static void SaveFileResult(IActionResult result, string path) =>
        File.WriteAllBytes(path, result.ShouldBeOfType<FileContentResult>().FileContents);
}
