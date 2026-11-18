using System.Globalization;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using EPR.Calculator.API.BackgroundService.Features.RunImport;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EPR.Calculator.API.IntegrationTests;

/// <summary>
///     Imports real billing files, which aren't in the repo. Set PAYCAL_IMPORT_FILES to the folder holding them and their
///     run-{id}.details.json files to run these.
/// </summary>
[TestCategory("LocalFiles")]
[TestClass]
[DoNotParallelize]
public class RunImportIntegrationTests : BaseIntegrationTest
{
    private static readonly JsonSerializerOptions DetailsJsonOptions = new() { Converters = { new JsonStringEnumConverter() } };

    [TestMethod]
    public async Task Import_run_10() => await ImportAndReadBack(10);

    // Runs are imported in order, since a year's first run is classified Initial and the rest Recalculation.
    [TestMethod]
    public async Task Import_all_runs()
    {
        foreach (var runId in new[] { 10, 51, 68, 84, 102 })
            await ImportAndReadBack(runId);
    }

    private static async Task ImportAndReadBack(int runId)
    {
        var folder = Environment.GetEnvironmentVariable("PAYCAL_IMPORT_FILES");
        if (string.IsNullOrEmpty(folder))
            Assert.Inconclusive("PAYCAL_IMPORT_FILES isn't set");

        var csvPath = Directory.GetFiles(folder, $"{runId}-*_Billing File_*.csv").Single();
        var details = JsonSerializer.Deserialize<RunImportDetails>(await File.ReadAllTextAsync(Path.Combine(folder, $"run-{runId}.details.json")), DetailsJsonOptions)!;

        await using var scope = Provider.CreateAsyncScope();
        var services = scope.ServiceProvider;

        using (var reader = new StreamReader(csvPath))
            (await services.GetRequiredService<IRunImporter>().ImportAsync(reader, csvPath, details, CancellationToken.None)).ShouldBe(runId);

        var csv = BillingCsv.Read(new StreamReader(csvPath));
        var header = RunSectionsParser.ReadHeader(csv);
        var stored = services.GetRequiredService<ICalcResultReader>();

        (await stored.ReadLapcapData(runId, CancellationToken.None)).ShouldBeEquivalentTo(RunSectionsParser.ReadLapcapData(csv));
        (await stored.ReadLateReportingTonnage(runId, CancellationToken.None)).ShouldBeEquivalentTo(RunSectionsParser.ReadLateReportingTonnage(csv));
        (await stored.ReadParameterOtherCost(runId, CancellationToken.None)).ShouldBeEquivalentTo(RunSectionsParser.ReadParameterOtherCost(csv, header));
        (await stored.ReadOnePlusFourApportionment(runId, CancellationToken.None)).ShouldBeEquivalentTo(RunSectionsParser.ReadOnePlusFourApportionment(csv));
        (await stored.ReadCommsCost(runId, CancellationToken.None)).ShouldBeEquivalentTo(RunSectionsParser.ReadCommsCost(csv));
        (await stored.ReadLaDisposalCostData(runId, CancellationToken.None)).ShouldBeEquivalentTo(RunSectionsParser.ReadLaDisposalCostData(csv));

        (await stored.ReadCancelledProducers(runId, CancellationToken.None)).ShouldBeEquivalentTo(ProducerSectionsParser.ReadCancelledProducers(csv).ToImmutableList());

        if (ProducerSectionsParser.ReadScaledupProducers(csv, RunSectionsParser.MaterialCodes) is { } scaledUp)
            DifferingFields(Flatten((await stored.ReadScaledData(runId, CancellationToken.None)).ScaledupProducers), Flatten(scaledUp.ScaledupProducers)).ShouldBeEmpty();

        var parameters = await services.GetRequiredService<IParameterService>().GetDefaultParameters(runId);
        parameters.CutOffDate.ShouldBe(header.CutOffDate);

        var downloaded = await ReadFileResult(await CreateController<BillingFileController>(services).DownloadBillingCsv(runId, downloadFromBlobStorage: false));
        var comparison = BillingCsvComparison.Compare(csv, BillingCsv.Read(new StreamReader(new MemoryStream(downloaded))));
        await File.WriteAllTextAsync(Path.Combine(folder, $"run-{runId}.comparison.txt"), comparison.ToString());

        var db = services.GetRequiredService<ApplicationDBContext>();
        var run = await db.CalculatorRuns.AsNoTracking().SingleAsync(r => r.Id == runId);
        run.Name.ShouldBe(header.RunName);
        run.IsBillingFileShared.ShouldBeTrue();
        run.Classification.ShouldBe(runId is 10 or 102 ? RunClassification.Initial : RunClassification.Recalculation);
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
