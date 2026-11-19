using System.Globalization;
using System.Collections.Immutable;
using System.IO.Compression;
using EPR.Calculator.API.Services.RunImport;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EPR.Calculator.API.IntegrationTests;

/// <summary>
///     Imports real runs from their results and billing files, which aren't in the repo, with any producer-names.csv,
///     run-details.csv and billing-instructions.csv beside them. Set PAYCAL_IMPORT_FILES to the folder holding them to run
///     this. It writes run-{id}.results-comparison.txt and run-{id}.billing-comparison.txt there.
/// </summary>
[TestCategory("LocalFiles")]
[TestClass]
[DoNotParallelize]
public class RunImportIntegrationTests : BaseIntegrationTest
{
    [TestMethod]
    public async Task Import_all_runs()
    {
        var folder = Environment.GetEnvironmentVariable("PAYCAL_IMPORT_FILES");
        if (string.IsNullOrEmpty(folder))
            Assert.Inconclusive("PAYCAL_IMPORT_FILES isn't set");

        var zip = Zip(folder);
        var runIds = RunImportPackage.Read(new MemoryStream(zip)).Select(r => r.RunId).ToList();

        await using (var scope = Provider.CreateAsyncScope())
        {
            var outcomes = await scope.ServiceProvider.GetRequiredService<IRunImportService>().ImportAsync(new MemoryStream(zip), checkOnly: false, CancellationToken.None);
            outcomes.ShouldBe(runIds.Select(id => new RunImportOutcome(id, RunImportService.Imported)));
        }

        foreach (var runId in runIds)
            await ReadBack(folder, runId);

        // Uploading the same zip again skips every run, since they're all stored.
        await using (var scope = Provider.CreateAsyncScope())
        {
            var outcomes = await scope.ServiceProvider.GetRequiredService<IRunImportService>().ImportAsync(new MemoryStream(zip), checkOnly: false, CancellationToken.None);
            outcomes.ShouldBe(runIds.Select(id => new RunImportOutcome(id, RunImportService.Skipped)));
        }
    }

    private static byte[] Zip(string folder)
    {
        var files = Directory.GetFiles(folder, "*_Results File_*.csv")
            .Concat(Directory.GetFiles(folder, "*_Billing File_*_AUTHORISED.csv"))
            .Concat(new[] { RunImportPackage.ProducerNamesFile, RunImportPackage.RunDetailsFile, RunImportPackage.BillingInstructionsFile }
                .Select(name => Path.Combine(folder, name))
                .Where(File.Exists));

        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create))
            foreach (var file in files)
                zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Fastest);
        return stream.ToArray();
    }

    private static async Task ReadBack(string folder, int runId)
    {
        await using var scope = Provider.CreateAsyncScope();
        var services = scope.ServiceProvider;

        var results = BillingCsv.Read(new StreamReader(Directory.GetFiles(folder, $"{runId}-*_Results File_*.csv").Single()));
        var billing = BillingCsv.Read(new StreamReader(Directory.GetFiles(folder, $"{runId}-*_Billing File_*_AUTHORISED.csv").Single()));
        var header = RunSectionsParser.ReadHeader(results);
        var stored = services.GetRequiredService<ICalcResultReader>();

        (await stored.ReadLapcapData(runId, CancellationToken.None)).ShouldBeEquivalentTo(RunSectionsParser.ReadLapcapData(results));
        (await stored.ReadLateReportingTonnage(runId, CancellationToken.None)).ShouldBeEquivalentTo(RunSectionsParser.ReadLateReportingTonnage(results));
        (await stored.ReadParameterOtherCost(runId, CancellationToken.None)).ShouldBeEquivalentTo(RunSectionsParser.ReadParameterOtherCost(results, header));
        (await stored.ReadOnePlusFourApportionment(runId, CancellationToken.None)).ShouldBeEquivalentTo(RunSectionsParser.ReadOnePlusFourApportionment(results));
        (await stored.ReadCommsCost(runId, CancellationToken.None)).ShouldBeEquivalentTo(RunSectionsParser.ReadCommsCost(results));
        (await stored.ReadLaDisposalCostData(runId, CancellationToken.None)).ShouldBeEquivalentTo(RunSectionsParser.ReadLaDisposalCostData(results));

        if (ProducerSectionsParser.ReadScaledupProducers(results, RunSectionsParser.MaterialCodes) is { } scaledUp)
            DifferingFields(Flatten((await stored.ReadScaledData(runId, CancellationToken.None)).ScaledupProducers), Flatten(scaledUp.ScaledupProducers)).ShouldBeEmpty();

        var parameters = await services.GetRequiredService<IParameterService>().GetDefaultParameters(runId);
        parameters.CutOffDate.ShouldBe(header.CutOffDate);

        var downloadedResults = await ReadFileResult(await CreateController<CalculatorController>(services).DownloadResultCsv(runId, downloadFromBlobStorage: false));
        await File.WriteAllTextAsync(Path.Combine(folder, $"run-{runId}.results-comparison.txt"),
            BillingCsvComparison.Compare(results, BillingCsv.Read(new StreamReader(new MemoryStream(downloadedResults)))).ToString());

        var downloadedBilling = await ReadFileResult(await CreateController<BillingFileController>(services).DownloadBillingCsv(runId, downloadFromBlobStorage: false));
        await File.WriteAllTextAsync(Path.Combine(folder, $"run-{runId}.billing-comparison.txt"),
            BillingCsvComparison.Compare(billing, BillingCsv.Read(new StreamReader(new MemoryStream(downloadedBilling)))).ToString());

        var db = services.GetRequiredService<ApplicationDBContext>();
        var run = await db.CalculatorRuns.AsNoTracking().SingleAsync(r => r.Id == runId);
        run.Name.ShouldBe(header.RunName);
        run.IsBillingFileShared.ShouldBeTrue();
    }

    // One entry per producer, material and field, so a mismatch can be reported by field name without printing producer data.
    private static Dictionary<string, string> Flatten(IEnumerable<CalcResultScaledupProducer> producers)
    {
        var values = new Dictionary<string, string>();
        foreach (var (producer, index) in producers.Select((p, i) => (p, i)))
        {
            foreach (var property in typeof(CalcResultScaledupProducer).GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string)))
                values[$"{index}|-|{property.Name}"] = Normalise(property.GetValue(producer));

            foreach (var (material, tonnage) in producer.ScaledupProducerTonnageByMaterial)
                foreach (var property in typeof(CalcResultScaledupProducerTonnage).GetProperties())
                    values[$"{index}|{material}|{property.Name}"] = Normalise(property.GetValue(tonnage));
        }
        return values;
    }

    private static string Normalise(object? value) => value is decimal d ? (d / 1.000000000000000000000000000000000m).ToString(CultureInfo.InvariantCulture) : value?.ToString() ?? "";

    private static List<string> DifferingFields(Dictionary<string, string> actual, Dictionary<string, string> expected) =>
        [.. expected.Keys.Union(actual.Keys)
            .Where(k => actual.GetValueOrDefault(k) != expected.GetValueOrDefault(k))
            .Select(k => string.Join('|', k.Split('|').Skip(1)))
            .GroupBy(f => f)
            .Select(g => $"{g.Key} x{g.Count()}")];
}
