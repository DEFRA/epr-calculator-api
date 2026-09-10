using System.Diagnostics;
using System.Globalization;
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
    private const int NumberOfProducers                    = 7_000; // producer + subsidiary rows the generator emits; the fee section ends up a little smaller after eligibility filtering (2025 PRE2: ~6,188)
    private const int PartialPercent                       = 1;
    private const int ProjectedRamH1Percent                = 10;
    private const int ProjectedRamH2Percent                = 10;
    private const int OrganisationsWithSubsidiariesPercent = 28;
    private const int MaxSubsidiaries                      = 5;
    private const int OrganisationsWithCwPercent           = 25;
    private const int BothHalvesSubmittedPercent           = 90; // organisations that submit both H1 and H2; the rest only report H1
    private const int ObligatedPercent                     = 90; // organisations with obligation status "O"; the rest stay in the Organisations output but are excluded from Producers

    // Base organisation count, back-calculated from NumberOfProducers through the subsidiary expansion.
    private static readonly int NumberOfOrganisations = OrganisationsForProducers(NumberOfProducers);

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
        fakeCommonDataApi.Poms                  = () => StreamPoms(pomPath);

        var fakeBlobStorage = Provider.GetRequiredService<FakeBlobStorageUploadService>();

        var calculatorTimings     = new List<TimeSpan>();
        var resultsCsvTimings     = new List<TimeSpan>();
        var billingTimings        = new List<TimeSpan>();
        var billingCsvTimings     = new List<TimeSpan>();
        var billingJsonTimings    = new List<TimeSpan>();
        var calculatorAllocations = new List<long>(); // managed bytes allocated (process-wide) during the Results section

        Console.WriteLine($"Performance test data: {fakeCommonDataApi.OrganisationResponses.Count:N0} organisations, {generatedPomCount:N0} POMs");
        Console.WriteLine($"Database: {db.Database.GetConnectionString()}");
        WriteGcCeiling(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes);

        Directory.CreateDirectory(outputDirectory);

        // Sample the process working set every 250ms so we can report the peak reached during the runs.
        using var process = Process.GetCurrentProcess();
        var peakWorkingSet = 0L;
        using var workingSetSampler = new Timer(_ =>
        {
            process.Refresh();
            peakWorkingSet = Math.Max(peakWorkingSet, process.WorkingSet64);
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(250));

        Console.WriteLine($"Test setup retained heap: {Gb(GC.GetTotalMemory(forceFullCollection: true))}");

        // Reclaim the previous section's garbage before measuring the next. In production each calc
        // message and each download is a separate, time-separated request with this same breathing
        // room; running them back to back here without it just measures GC timing.
        static long CollectAndMeasure()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            return GC.GetTotalMemory(forceFullCollection: false);
        }

        void Step(int run, string label)
        {
            var retainedHeap = CollectAndMeasure();
            process.Refresh();
            Console.WriteLine();
            Console.WriteLine($"========== run {run}: {label}  |  working set {Gb(process.WorkingSet64)}, retained heap {Gb(retainedHeap)}, allocated so far {Gb(GC.GetTotalAllocatedBytes())} ==========");
        }

        for (var i = 0; i < NumberOfRuns; i++)
        {
            var name = $"performance-{relativeYear}-{Guid.NewGuid():N}";

            db.ChangeTracker.Clear();   // db is reused across iterations; don't let tracked entities accumulate
            fakeBlobStorage.Reset();    // and don't let previous runs' file contents pin the heap

            Step(i + 1, "Calculator run");

            // Results
            var allocatedBefore = GC.GetTotalAllocatedBytes();
            var stopwatch = Stopwatch.StartNew();

            var createRunResult = (await CallController<CalculatorController, IActionResult>(services, c => c.Create(new CreateCalculatorRunDto
            {
                CalculatorRunName = name,
                RelativeYear = relativeYear
            }))).ShouldBeOfType<ObjectResult>();

            createRunResult.StatusCode.ShouldBe(StatusCodes.Status202Accepted, $"Controller returned: {JsonSerializer.Serialize(createRunResult.Value)}");

            var runId = await db.CalculatorRuns
                .Where(x => x.Name == name)
                .Select(x => x.Id)
                .SingleAsync();

            await WaitForCalculatorRunAsync(db, runId);

            stopwatch.Stop();
            calculatorTimings.Add(stopwatch.Elapsed);
            calculatorAllocations.Add(GC.GetTotalAllocatedBytes() - allocatedBefore);

            // Results CSV - written to blob storage during the run; read it back and save to disk
            Step(i + 1, "Read results CSV");
            stopwatch.Restart();
            await SaveBlobFile(fakeBlobStorage, await ResultsFileName(db, runId), Path.Combine(outputDirectory, $"run-{i + 1}-results.csv"));
            stopwatch.Stop();
            resultsCsvTimings.Add(stopwatch.Elapsed);

            // Billing
            Step(i + 1, "Billing run");
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

            var setBillingClassificationResult = await CallController<CalculatorNewController, IActionResult>(services, c => c.PutCalculatorRunStatus(new CalculatorRunStatusUpdateDto {
                    RunId = runId,
                    ClassificationId = RunClassificationStatusIds.INITIALRUNID
                }));

            if (setBillingClassificationResult is StatusCodeResult statusCodeResult)
            {
                statusCodeResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
            }
            else
            {
                throw new Exception($"Controller returned: {JsonSerializer.Serialize(setBillingClassificationResult.ShouldBeOfType<ObjectResult>().Value)}");
            }

            await SeedAllProducersAsAcceptedAsync(db, runId, "some-user", fakeCommonDataApi.OrganisationResponses.Select(x => x.OrganisationId!.Value));
            var startBillingResult = (await CallController<ProducerBillingFileController, IActionResult>(services, c => c.ProducerBillingInstructions(runId))).ShouldBeOfType<ObjectResult>();
            startBillingResult.StatusCode.ShouldBe(StatusCodes.Status200OK, $"Controller returned: {JsonSerializer.Serialize(startBillingResult.Value)}");
            await WaitForBillingRunAsync(db, runId);

            stopwatch.Stop();
            billingTimings.Add(stopwatch.Elapsed);

            var billingMetadata = await db.CalculatorRunBillingFileMetadata
                .AsNoTracking()
                .Where(x => x.CalculatorRunId == runId)
                .OrderByDescending(x => x.BillingFileCreatedDate)
                .FirstAsync();

            // Billing CSV - read from blob
            Step(i + 1, "Read billing CSV");
            stopwatch.Restart();
            await SaveBlobFile(fakeBlobStorage, billingMetadata.BillingCsvFileName, Path.Combine(outputDirectory, $"run-{i + 1}-billing.csv"));
            stopwatch.Stop();
            billingCsvTimings.Add(stopwatch.Elapsed);

            // Billing JSON - read from blob
            Step(i + 1, "Read billing JSON");
            stopwatch.Restart();
            await SaveBlobFile(fakeBlobStorage, billingMetadata.BillingJsonFileName, Path.Combine(outputDirectory, $"run-{i + 1}-billing.json"));
            stopwatch.Stop();
            billingJsonTimings.Add(stopwatch.Elapsed);
        }

        Console.WriteLine();
        Console.WriteLine($"Performance test data: {fakeCommonDataApi.OrganisationResponses.Count:N0} organisations, {generatedPomCount:N0} POMs");
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

        var gcCeiling = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;

        Console.WriteLine();
        Console.WriteLine("Memory");
        Console.WriteLine("------");
        WriteGcCeiling(gcCeiling);
        Console.WriteLine($"Peak working set (whole test):           {Gb(peakWorkingSet)} ({Percent(peakWorkingSet, gcCeiling)} of ceiling)");
        Console.WriteLine($"Managed heap live at end:                {Gb(GC.GetTotalMemory(forceFullCollection: true))}");
        Console.WriteLine($"Managed bytes allocated per Results run: {Gb((long)calculatorAllocations.Average())} avg, {Gb(calculatorAllocations.Max())} max");
    }

    private static string Gb(long bytes) => $"{bytes / 1024d / 1024d / 1024d:0.00} GB";

    private static string Percent(long value, long of) => of > 0 ? $"{100.0 * value / of:0}%" : "n/a";

    // The GC's heap ceiling - the cgroup/container memory limit on Linux, DOTNET_GCHeapHardLimit if set,
    // otherwise a fraction of physical RAM. This is what the process is actually judged against.
    private static void WriteGcCeiling(long gcCeiling)
    {
        var hardLimit = Environment.GetEnvironmentVariable("DOTNET_GCHeapHardLimit");
        var hardLimitPercent = Environment.GetEnvironmentVariable("DOTNET_GCHeapHardLimitPercent");
        var note = (hardLimit, hardLimitPercent) switch
        {
            ({ } h, _) => $"  (DOTNET_GCHeapHardLimit=0x{h.TrimStart('0', 'x', 'X')})",
            (_, { } p) => $"  (DOTNET_GCHeapHardLimitPercent={p})",
            _          => "  (no hard limit set - physical RAM fraction)"
        };
        Console.WriteLine($"GC heap ceiling:                      {Gb(gcCeiling)}{note}");
    }

    private static async Task<string> ResultsFileName(ApplicationDBContext db, int runId) =>
        await db.CalculatorRunCsvFileMetadata
            .AsNoTracking()
            .Where(x => x.CalculatorRunId == runId)
            .Select(x => x.FileName)
            .SingleAsync();

    private static async Task SaveBlobFile(FakeBlobStorageUploadService blob, string fileName, string path) =>
        await File.WriteAllTextAsync(path, blob.Get(fileName));

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
        static (string ObligationStatus, Guid SubmitterId, string? ErrorCode, int? NumDaysObligated, string? StatusCode, string? JoinerDate) GetOrganisationValues(OrganisationScenario scenario, Guid submitterId, bool obligated) =>
            scenario switch
            {
                OrganisationScenario.Partial             => ("O", submitterId         , null , 233 , "02", "22-05-2025"),
                OrganisationScenario.MissingRegistration => ("O", CreateSubmitterId(0), null , null, null, null),
                OrganisationScenario.Error               => ("E", submitterId         , "111", null, null, null),
                _                                        => (obligated ? "O" : "N", submitterId, null, null, null, null)
            };

        var path = Path.Combine(AppContext.BaseDirectory, "TestData", $"performance-{relativeYear}-organisation-data.csv");
        using var output = new StreamWriter(path);
        output.WriteLine("organisation_id,subsidiary_id,organisation_name,trading_name,obligation_status,submitter_id,error_code,num_days_obligated,status_code,joiner_date,leaver_date,has_h1,has_h2");

        for (var i = 0; i < NumberOfOrganisations; i++)
        {
            var organisationId  = GetOrganisationId(i);
            var submitterId     = CreateSubmitterId(i);
            var scenario        = GetOrganisationScenario(i);
            var obligated       = scenario != OrganisationScenario.Standard || i % 100 < ObligatedPercent;
            var subsidiaryCount = GetSubsidiaryCount(i);
            var subsidiaryIds   = subsidiaryCount == 0
                ? [null]
                : Enumerable.Range(1, subsidiaryCount)
                    .Select(x => (int?)GetSubsidiaryId(i, x))
                    .ToArray();

            var (obligationStatus, scenarioSubmitterId, errorCode, numDaysObligated, statusCode, joinerDate) = GetOrganisationValues(scenario, submitterId, obligated);

            foreach (var subsidiaryId in subsidiaryIds)
            {
                var organisationName = subsidiaryId is null
                    ? $"Performance Test {organisationId}"
                    : $"Performance Test {organisationId} Subsidiary {subsidiaryId}";

                output.WriteLine(
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

        return path;
    }

    private static string GeneratePomData(RelativeYear relativeYear)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", $"performance-{relativeYear}-pom-data.csv");
        using var output = new StreamWriter(path);
        output.WriteLine("organisation_id,subsidiary_id,submission_period,packaging_activity,packaging_type,packaging_class,packaging_material,packaging_material_weight,submission_period_desc,submitter_id,packaging_material_subtype,ram_rag_rating");

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
            var bothHalves     = scenario != OrganisationScenario.Standard || i % 100 < BothHalvesSubmittedPercent;
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

                if (bothHalves)
                    AddPomsForPeriod(output, organisationId, subsidiaryId, "2025-H2", "July to December 2025", submitterId, selectedMaterials, random, projectedRamH2);
            }
        }

        return path;
    }

    private static void AddPomsForPeriod(TextWriter output, int organisationId, int? subsidiaryId, string period, string description, Guid submitterId, string[] materials, Random random, bool projectedRam)
    {
        void AddPom(TextWriter output, string material, string packagingType, bool projectedRam)
        {
            var weight       = random.Next(10_000, 500_001);
            var ramRagRating = projectedRam ? null : "R";

            output.WriteLine(
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

    // Invert the subsidiary expansion: an organisation with no subsidiaries is one row,
    // one with subsidiaries is that many rows (no parent row of its own).
    private static int OrganisationsForProducers(int producerRows)
    {
        var rows = 0;
        var organisations = 0;
        while (rows < producerRows)
            rows += Math.Max(1, GetSubsidiaryCount(organisations++));
        return organisations;
    }

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
}
