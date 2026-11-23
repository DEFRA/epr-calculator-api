using System.Collections.Immutable;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using EPR.Calculator.API.Services.RunImport;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EPR.Calculator.API.IntegrationTests;

/// <summary>
///     Imports the results and billing CSVs the calculator run integration test expects, and checks the run's files come
///     out as that test expects them.
/// </summary>
[TestCategory("IntegrationTests")]
[TestClass]
[DoNotParallelize]
public class RunImportExpectedDataIntegrationTests : BaseIntegrationTest
{
    [TestMethod]
    [DataRow(2025)]
    [DataRow(2026)]
    public async Task Importing_the_expected_files_gives_the_expected_files(int year)
    {
        // The other tests' runs share the database, so the imported run gets an ID of its own.
        var runId = 900_000 + year;
        string WithRunId(string csv) => Regex.Replace(csv, @"^Run Id,""\d+"",", $"Run Id,\"{runId}\",", RegexOptions.Multiline);
        var results = await File.ReadAllTextAsync($"ExpectedData/{year}-results.csv");

        // One organisation from the Error Table marked as having errors, and one the files don't show.
        var withErrors = ProducerSectionsParser.ReadErrorReport(BillingCsv.Read(new StringReader(results))).First(e => e.ErrorCode is not null);
        const int notInTheFiles = 999_999;

        using var zip = new MemoryStream();
        using (var archive = new ZipArchive(zip, ZipArchiveMode.Create, leaveOpen: true))
        {
            void Add(string name, string text)
            {
                using var writer = new StreamWriter(archive.CreateEntry(name).Open());
                writer.Write(text);
            }

            Add($"{runId}-test{year}_Results File_20260923.csv", WithRunId(results));
            Add($"{runId}-test{year}_Billing File_202609231710_AUTHORISED.csv", WithRunId(await File.ReadAllTextAsync($"ExpectedData/{year}-billing.csv")));
            Add(RunImportPackage.RunDetailsFile, $"line\nrun_id,created_at,classification_id,classification,billing_file_shared_by,billing_file_shared_at,updated_by,updated_at,billing_run_started_at,billing_file_created_by,billing_file_created_at\n{runId},2026-09-23T17:09:42.1234567,7,\"INITIAL RUN COMPLETED\",\"some-user\",2026-09-24T09:15:00,\"designer\",2026-09-23T17:12:00,2026-09-23T17:13:00,\"creator\",2026-09-23T17:10:30.5\n");
            Add(RunImportPackage.LapcapFile, $"run_id,unique_reference,total_cost\n{runId},ENG-AL,5000.004\n");
            Add(RunImportPackage.OrganisationsFile, $"run_id,organisation_id,subsidiary_id,obligation_status,organisation_name\n{runId},{withErrors.ProducerId},{withErrors.SubsidiaryId},E,\n{runId},{notInTheFiles},,N,\"Not, In The Files\"\n");
        }

        await using var scope = Provider.CreateAsyncScope();
        var services = scope.ServiceProvider;

        zip.Position = 0;
        (await services.GetRequiredService<IRunImportService>().ImportAsync(zip, checkOnly: true, CancellationToken.None))
            .ShouldBe([new RunImportOutcome(runId, RunImportService.Checked)]);
        (await services.GetRequiredService<ApplicationDBContext>().CalculatorRuns.AnyAsync(r => r.Id == runId)).ShouldBeFalse();

        zip.Position = 0;
        (await services.GetRequiredService<IRunImportService>().ImportAsync(zip, checkOnly: false, CancellationToken.None))
            .ShouldBe([new RunImportOutcome(runId, RunImportService.Imported)]);

        await CalculatorRunIntegrationTests.AssertCsv(
            actualContents: Encoding.UTF8.GetString(await ReadFileResult(await CreateController<CalculatorController>(services).DownloadResultCsv(runId, downloadFromBlobStorage: false))),
            expectedPath: $"ExpectedData/{year}-results.csv",
            ignoreLines: [1, 2, 3, 7, 8, 9],
            label: "Results CSV");

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

        var db = services.GetRequiredService<ApplicationDBContext>();
        var organisations = await db.CalculatorRunOrganisations.AsNoTracking().Where(o => o.CalculatorRunId == runId).ToListAsync();
        organisations.Where(o => o.IsError).Select(o => (o.OrganisationId, o.SubsidiaryId)).ShouldBe([(withErrors.ProducerId, withErrors.SubsidiaryId)]);
        organisations.Single(o => o.OrganisationId == notInTheFiles).OrganisationName.ShouldBe("Not, In The Files");

        // What was invoiced, which later runs read: a net tonnage per material for each producer, and an invoice
        // instruction for each billing instruction.
        var instructions = await db.ProducerResultFileSuggestedBillingInstruction.AsNoTracking().Where(i => i.CalculatorRunId == runId).ToListAsync();
        var invoiced = await db.ProducerDesignatedRunInvoiceInstruction.AsNoTracking().Where(i => i.CalculatorRunId == runId).ToListAsync();
        var tonnages = await db.ProducerInvoicedMaterialNetTonnage.AsNoTracking().Where(t => t.CalculatorRunId == runId).ToListAsync();
        var materials = await db.Material.CountAsync();

        instructions.ShouldNotBeEmpty();
        invoiced.Select(i => i.ProducerId).Order().ShouldBe(instructions.Select(i => i.ProducerId).Order());
        tonnages.Count.ShouldBe(instructions.Count(i => i.SuggestedBillingInstruction != "CANCEL") * materials);
        foreach (var instruction in instructions.Where(i => i is { BillingInstructionAcceptReject: "Accepted", SuggestedBillingInstruction: "INITIAL" }))
            invoiced.Single(i => i.ProducerId == instruction.ProducerId).InvoiceAmount.ShouldBe(instruction.TotalProducerBillWithBadDebt);
        invoiced.Select(i => i.InstructionConfirmedDate).Distinct().ShouldBe([new DateTime(2026, 9, 24, 9, 15, 0)]);

        // The LAPCAP and parameter uploads the run used, worked out from the file, are the ones the calculator run uploaded.
        var run = await db.CalculatorRuns.AsNoTracking().Include(r => r.CalculatorRunBillingFileMetadata).SingleAsync(r => r.Id == runId);
        run.ShouldSatisfyAllConditions(
            r => r.UpdatedBy.ShouldBe("designer"),
            r => r.UpdatedAt.ShouldBe(new DateTime(2026, 9, 23, 17, 12, 0)),
            r => r.BillingRunStartedAt.ShouldBe(new DateTime(2026, 9, 23, 17, 13, 0, DateTimeKind.Utc)),
            r => r.BillingFileSharedAt.ShouldBe(new DateTime(2026, 9, 24, 9, 15, 0, DateTimeKind.Utc)),
            r => r.CalculatorRunBillingFileMetadata.Single().BillingFileCreatedBy.ShouldBe("creator"),
            r => r.CalculatorRunBillingFileMetadata.Single().BillingFileCreatedDate.ShouldBe(new DateTime(2026, 9, 23, 17, 10, 30, 500)));

        var lapcapMaster = await db.LapcapDataMaster.AsNoTracking().Include(m => m.Details).SingleAsync(m => m.Id == run.LapcapDataMasterId);
        var templates = (await db.LapcapDataTemplateMaster.AsNoTracking().ToListAsync()).ToImmutableList();
        lapcapMaster.Details.Select(d => (d.UniqueReference, d.TotalCost)).OrderBy(d => d.UniqueReference)
            .ShouldBe(LapcapDataDetails("TestData/lapcap.csv", lapcapMaster, templates).Select(d => (d.UniqueReference, d.UniqueReference == "ENG-AL" ? 5000.004m : d.TotalCost)).OrderBy(d => d.UniqueReference));

        var parameters = await db.DefaultParameterSettingDetail.AsNoTracking().Where(d => d.DefaultParameterSettingMasterId == run.DefaultParameterSettingMasterId).ToListAsync();
        var imported = parameters.ToDictionary(d => d.ParameterUniqueReferenceId, d => Normalise(d.ParameterValue));
        var uploaded = DefaultParameterSettingDetails("TestData/defaultParams.csv", 0).ToDictionary(d => d.ParameterUniqueReferenceId, d => Normalise(d.ParameterValue));
        // A file without modulation doesn't show the red modulation factor, so the import can't know what was uploaded.
        if (year < 2026)
            uploaded["REDM-RF"] = imported["REDM-RF"];

        imported.Keys.Union(uploaded.Keys)
            .Where(key => imported.GetValueOrDefault(key) != uploaded.GetValueOrDefault(key))
            .Select(key => $"{key}: imported {imported.GetValueOrDefault(key) ?? "(none)"}, uploaded {uploaded.GetValueOrDefault(key) ?? "(none)"}")
            .ShouldBeEmpty();
    }

    // Numbers compare by value, so "2870.00" matches "2870"; the cut-off date compares as written.
    private static string Normalise(string value) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
            ? (number / 1.000000000000000000000000000000000m).ToString(CultureInfo.InvariantCulture)
            : value;
}
