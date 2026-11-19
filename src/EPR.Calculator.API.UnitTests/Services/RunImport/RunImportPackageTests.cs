using System.IO.Compression;
using System.Text;
using EPR.Calculator.API.Services.RunImport;
using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.UnitTests.Services.RunImport;

[TestClass]
public class RunImportPackageTests
{
    private const string Results51 = "51-Some Run_Results File_20260311.csv";
    private const string Billing51 = "51-Some Run_Billing File_202603201145_AUTHORISED.csv";
    private const string Results10 = "10-First Run_Results File_20250922.csv";
    private const string Billing10 = "10-First Run_Billing File_202510061011_AUTHORISED.csv";

    [TestMethod]
    public void Read_ReturnsEachRunsFiles_InRunIdOrder()
    {
        var runs = RunImportPackage.Read(Zip((Results51, "results 51"), (Billing51, "billing 51"), (Results10, "results 10"), (Billing10, "billing 10")));

        runs.Select(r => r.RunId).ShouldBe([10, 51]);
        runs[1].ShouldSatisfyAllConditions(
            r => r.ResultsFileName.ShouldBe(Results51),
            r => r.ResultsCsv.ShouldBe("results 51"),
            r => r.BillingFileName.ShouldBe(Billing51),
            r => r.BillingCsv.ShouldBe("billing 51"));
    }

    [TestMethod]
    public void Read_Fails_WhenARunHasNoResultsFile() =>
        Should.Throw<RunImportPackageException>(() => RunImportPackage.Read(Zip((Billing51, ""))))
            .Message.ShouldBe("Run 51 has a billing file but no results file (expected '51-…_Results File_{yyyyMMdd}.csv')");

    [TestMethod]
    public void Read_Fails_WhenARunHasNoBillingFile() =>
        Should.Throw<RunImportPackageException>(() => RunImportPackage.Read(Zip((Results51, ""))))
            .Message.ShouldBe("Run 51 has a results file but no billing file (expected '51-…_Billing File_{yyyyMMddHHmm}_AUTHORISED.csv')");

    [TestMethod]
    public void Read_Fails_WhenABillingFileIsntAuthorised() =>
        Should.Throw<RunImportPackageException>(() => RunImportPackage.Read(Zip((Results51, ""), ("51-Some Run_Billing File_202603201145.csv", ""))))
            .Message.ShouldContain("isn't an authorised billing file");

    [TestMethod]
    public void Read_Fails_WhenTheZipHasAFileItDoesntRecognise() =>
        Should.Throw<RunImportPackageException>(() => RunImportPackage.Read(Zip((Results51, ""), (Billing51, ""), ("notes.txt", ""))))
            .Message.ShouldStartWith("The zip has a file the import doesn't recognise: 'notes.txt'");

    [TestMethod]
    public void Read_Fails_WhenTheUploadIsntAZip() =>
        Should.Throw<RunImportPackageException>(() => RunImportPackage.Read(new MemoryStream("not a zip"u8.ToArray())))
            .Message.ShouldBe("The upload isn't a zip file");

    [TestMethod]
    public void Read_IgnoresMacFolderFiles() =>
        RunImportPackage.Read(Zip((Results51, ""), (Billing51, ""), ("__MACOSX/._x.csv", ""), (".DS_Store", ""))).Count.ShouldBe(1);

    [TestMethod]
    public void Read_KeepsNamesExactly_AndSkipsALeadingLineRow()
    {
        var runs = RunImportPackage.Read(Zip(
            (Results51, ""),
            (Billing51, ""),
            (RunImportPackage.ProducerNamesFile, "line\nrun_id,producer_id,subsidiary_id,producer_name,trading_name\n51,100,\"\",\"Smith, Jones Ltd \",\"\"\n51,100,\"7\",\"Sub \"\"One\"\"\",\"Trading\"\n")));

        runs.Single().Details.ProducerNames.ShouldBe(new Dictionary<(int, string?), ProducerNames>
        {
            [(100, null)] = new("Smith, Jones Ltd ", null),
            [(100, "7")]  = new("Sub \"One\"", "Trading")
        });
    }

    [TestMethod]
    public void Read_TakesRunDetails_AndBillingDecisions_WithoutALineRow()
    {
        var runs = RunImportPackage.Read(Zip(
            (Results51, ""),
            (Billing51, ""),
            (RunImportPackage.RunDetailsFile, "run_id,created_at,classification_id,classification,billing_file_shared_by,billing_file_shared_at\n51,2026-03-11T11:33:14.0190108,12,\"RECALCULATION RUN COMPLETED\",\"Someone\",2026-03-20T16:47:59\n"),
            (RunImportPackage.BillingInstructionsFile, "run_id,producer_id,accept_reject,accept_reject_by,accept_reject_at\n51,100,Rejected,\"Someone Else\",2026-03-19T10:00:00\n")));

        var details = runs.Single().Details;
        details.ShouldSatisfyAllConditions(
            d => d.RunDate.ShouldBe(new DateTime(2026, 3, 11, 11, 33, 14).AddTicks(190108)),
            d => d.Classification.ShouldBe(RunClassification.Recalculation),
            d => d.BillingFileSharedBy.ShouldBe("Someone"),
            d => d.BillingFileSharedAt.ShouldBe(new DateTime(2026, 3, 20, 16, 47, 59)),
            d => d.BillingDecisions[100].ShouldBe(new BillingDecision("Rejected", "Someone Else", new DateTime(2026, 3, 19, 10, 0, 0))));
    }

    [TestMethod]
    public void Read_TakesEachRunsOrganisations_InTheirOrder()
    {
        var runs = RunImportPackage.Read(Zip(
            (Results51, ""),
            (Billing51, ""),
            (RunImportPackage.OrganisationsFile, "run_id,organisation_id,subsidiary_id,obligation_status,organisation_name\n51,200,,E,\n51,100,7,N,\"Not, In The Files\"\n")));

        runs.Single().Details.Organisations.ShouldBe([
            new RunOrganisation(200, null, null, null, null, null, null, null, "E"),
            new RunOrganisation(100, "7", "Not, In The Files", null, null, null, null, null, "N")
        ]);
    }

    [TestMethod]
    public void Read_Fails_WhenAnOptionalFileIsMissingAColumn() =>
        Should.Throw<RunImportPackageException>(() => RunImportPackage.Read(Zip((Results51, ""), (Billing51, ""), (RunImportPackage.ProducerNamesFile, "run_id,producer_id\n"))))
            .Message.ShouldBe("producer-names.csv has no 'subsidiary_id' column: it needs run_id, producer_id, subsidiary_id, producer_name, trading_name");

    private static MemoryStream Zip(params (string Name, string Text)[] files)
    {
        var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            foreach (var (name, text) in files)
            {
                using var entry = zip.CreateEntry(name).Open();
                entry.Write(Encoding.UTF8.GetBytes(text));
            }

        stream.Position = 0;
        return stream;
    }
}
