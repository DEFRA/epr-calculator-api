using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.Services.RunImport;

/// <summary>Why a zip of runs can't be imported, in words the person uploading it can act on.</summary>
public sealed class RunImportPackageException(string message) : Exception(message);

/// <summary>One run's files from a zip, with the corrections the zip's optional CSVs hold for it.</summary>
public sealed record RunImportPackageRun(int RunId, string ResultsFileName, string ResultsCsv, string BillingFileName, string BillingCsv, RunImportDetails Details)
{
    public RunImportFiles Files => new(ResultsFileName, new StringReader(ResultsCsv), BillingFileName, new StringReader(BillingCsv));
}

/// <summary>
///     Reads a zip of runs to import: each run's results CSV and authorised billing CSV, by their usual names, and
///     optionally producer-names.csv, run-details.csv, billing-instructions.csv, organisations.csv and lapcap.csv, each covering every run with a
///     run_id column.
/// </summary>
public static partial class RunImportPackage
{
    public const string ProducerNamesFile = "producer-names.csv";
    public const string RunDetailsFile = "run-details.csv";
    public const string BillingInstructionsFile = "billing-instructions.csv";
    public const string OrganisationsFile = "organisations.csv";
    public const string LapcapFile = "lapcap.csv";

    /// <returns>The zip's runs, in run ID order.</returns>
    /// <exception cref="RunImportPackageException">The zip is missing a file, or has one it doesn't recognise or can't read.</exception>
    public static IReadOnlyList<RunImportPackageRun> Read(Stream zipStream)
    {
        ZipArchive zip;
        try
        {
            zip = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);
        }
        catch (InvalidDataException)
        {
            throw new RunImportPackageException("The upload isn't a zip file");
        }

        using (zip)
        {
            var (results, billing, optional) = Files(zip);

            foreach (var runId in billing.Keys.Except(results.Keys))
                throw new RunImportPackageException($"Run {runId} has a billing file but no results file (expected '{runId}-…_Results File_{{yyyyMMdd}}.csv')");
            foreach (var runId in results.Keys.Except(billing.Keys))
                throw new RunImportPackageException($"Run {runId} has a results file but no billing file (expected '{runId}-…_Billing File_{{yyyyMMddHHmm}}_AUTHORISED.csv')");
            if (results.Count == 0)
                throw new RunImportPackageException("The zip has no runs: it needs each run's results file and authorised billing file");

            var names = optional.TryGetValue(ProducerNamesFile, out var namesText) ? ReadProducerNames(namesText) : [];
            var runDetails = optional.TryGetValue(RunDetailsFile, out var detailsText) ? ReadRunDetails(detailsText) : [];
            var decisions = optional.TryGetValue(BillingInstructionsFile, out var decisionsText) ? ReadBillingDecisions(decisionsText) : [];
            var organisations = optional.TryGetValue(OrganisationsFile, out var organisationsText) ? ReadOrganisations(organisationsText) : [];
            var lapcapCosts = optional.TryGetValue(LapcapFile, out var lapcapText) ? ReadLapcapCosts(lapcapText) : [];

            return [.. results.Keys.Order().Select(runId =>
            {
                var details = runDetails.GetValueOrDefault(runId) ?? new RunImportDetails();
                return new RunImportPackageRun(runId, results[runId].Name, results[runId].Text, billing[runId].Name, billing[runId].Text, details with
                {
                    ProducerNames = names.GetValueOrDefault(runId) ?? details.ProducerNames,
                    BillingDecisions = decisions.GetValueOrDefault(runId) ?? details.BillingDecisions,
                    Organisations = organisations.GetValueOrDefault(runId) ?? details.Organisations,
                    LapcapCosts = lapcapCosts.GetValueOrDefault(runId) ?? details.LapcapCosts
                });
            })];
        }
    }

    /// <summary>The zip's results and billing files by run ID, and its optional files by name.</summary>
    private static (Dictionary<int, (string Name, string Text)> Results, Dictionary<int, (string Name, string Text)> Billing, Dictionary<string, string> Optional) Files(ZipArchive zip)
    {
        var results = new Dictionary<int, (string Name, string Text)>();
        var billing = new Dictionary<int, (string Name, string Text)>();
        var optional = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        {
            foreach (var entry in zip.Entries)
            {
                var name = entry.Name;
                if (name.Length == 0 || name.StartsWith('.') || entry.FullName.StartsWith("__MACOSX/", StringComparison.Ordinal))
                    continue;

                var text = ReadText(entry);
                if (ResultsPattern().Match(name) is { Success: true } resultsMatch)
                    Add(results, RunId(resultsMatch), name, text, "results");
                else if (BillingPattern().Match(name) is { Success: true } billingMatch)
                {
                    if (!billingMatch.Groups["authorised"].Success)
                        throw new RunImportPackageException($"'{name}' isn't an authorised billing file: its name should end '_AUTHORISED.csv'");
                    Add(billing, RunId(billingMatch), name, text, "billing");
                }
                else if (name.Equals(ProducerNamesFile, StringComparison.OrdinalIgnoreCase)
                         || name.Equals(RunDetailsFile, StringComparison.OrdinalIgnoreCase)
                         || name.Equals(BillingInstructionsFile, StringComparison.OrdinalIgnoreCase)
                         || name.Equals(OrganisationsFile, StringComparison.OrdinalIgnoreCase)
                         || name.Equals(LapcapFile, StringComparison.OrdinalIgnoreCase))
                    optional[name] = text;
                else
                    throw new RunImportPackageException(
                        $"The zip has a file the import doesn't recognise: '{entry.FullName}'. It takes results files ('{{run id}}-…_Results File_{{yyyyMMdd}}.csv'), " +
                        $"authorised billing files ('{{run id}}-…_Billing File_{{yyyyMMddHHmm}}_AUTHORISED.csv'), {ProducerNamesFile}, {RunDetailsFile}, {BillingInstructionsFile}, {OrganisationsFile} and {LapcapFile}.");
            }
        }

        return (results, billing, optional);
    }

    private static void Add(Dictionary<int, (string Name, string Text)> files, int runId, string name, string text, string kind)
    {
        if (files.TryGetValue(runId, out var existing))
            throw new RunImportPackageException($"Run {runId} has two {kind} files: '{existing.Name}' and '{name}'");
        files[runId] = (name, text);
    }

    private static int RunId(Match match) => int.Parse(match.Groups["runId"].Value, CultureInfo.InvariantCulture);

    private static string ReadText(ZipArchiveEntry entry)
    {
        using var reader = new StreamReader(entry.Open(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    /// <summary>
    ///     An optional CSV's rows by column name. A first line of just "line" is skipped: some tools save the column name of
    ///     a query that builds its CSV lines itself.
    /// </summary>
    private static List<Dictionary<string, string>> ReadRows(string text, string fileName, params string[] requiredColumns)
    {
        var rows = BillingCsv.Read(new StringReader(text)).Rows.Where(r => r.Any(c => c.Length > 0)).ToList();
        if (rows.Count > 0 && rows[0] is ["line"])
            rows.RemoveAt(0);
        if (rows.Count == 0)
            throw new RunImportPackageException($"{fileName} is empty: it needs a header row of {string.Join(", ", requiredColumns)}");

        var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
        if (requiredColumns.FirstOrDefault(c => !header.Contains(c)) is { } missing)
            throw new RunImportPackageException($"{fileName} has no '{missing}' column: it needs {string.Join(", ", requiredColumns)}");

        return [.. rows.Skip(1).Select(row => header.Select((column, i) => (column, value: i < row.Length ? row[i] : "")).ToDictionary(c => c.column, c => c.value))];
    }

    private static int Int(Dictionary<string, string> row, string column, string fileName) =>
        int.TryParse(row[column].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var value)
            ? value
            : throw new RunImportPackageException($"{fileName} has '{row[column]}' for {column}, which isn't a number");

    private static DateTime? Date(Dictionary<string, string> row, string column, string fileName) =>
        row[column].Trim() is "" ? null
        : DateTime.TryParse(row[column].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value) ? value
        : throw new RunImportPackageException($"{fileName} has '{row[column]}' for {column}, which isn't a date");

    private static string? Text(Dictionary<string, string> row, string column) => row[column].Length == 0 ? null : row[column];

    private static string? Optional(Dictionary<string, string> row, string column) => row.TryGetValue(column, out var value) && value.Length > 0 ? value : null;

    private static DateTime? OptionalDate(Dictionary<string, string> row, string column, string fileName) => row.ContainsKey(column) ? Date(row, column, fileName) : null;

    // Names are kept exactly as written, spaces at either end included: those are what the files lost.
    private static Dictionary<int, IReadOnlyDictionary<(int ProducerId, string? SubsidiaryId), ProducerNames>> ReadProducerNames(string text) =>
        ReadRows(text, ProducerNamesFile, "run_id", "producer_id", "subsidiary_id", "producer_name", "trading_name")
            .GroupBy(row => Int(row, "run_id", ProducerNamesFile))
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<(int, string?), ProducerNames>)g.ToDictionary(
                    row => (Int(row, "producer_id", ProducerNamesFile), Text(row, "subsidiary_id")),
                    row => new ProducerNames(Text(row, "producer_name"), Text(row, "trading_name"))));

    private static Dictionary<int, RunImportDetails> ReadRunDetails(string text) =>
        ReadRows(text, RunDetailsFile, "run_id", "created_at", "classification", "billing_file_shared_by", "billing_file_shared_at")
            .ToDictionary(
                row => Int(row, "run_id", RunDetailsFile),
                row => new RunImportDetails
                {
                    RunDate             = Date(row, "created_at", RunDetailsFile),
                    Classification      = Classification(row["classification"]),
                    BillingFileSharedBy  = Text(row, "billing_file_shared_by"),
                    BillingFileSharedAt  = Date(row, "billing_file_shared_at", RunDetailsFile),
                    UpdatedBy            = Optional(row, "updated_by"),
                    UpdatedAt            = OptionalDate(row, "updated_at", RunDetailsFile),
                    BillingRunStartedAt  = OptionalDate(row, "billing_run_started_at", RunDetailsFile),
                    BillingFileCreatedBy = Optional(row, "billing_file_created_by"),
                    BillingFileCreatedAt = OptionalDate(row, "billing_file_created_at", RunDetailsFile),
                    InstructionConfirmedAt = OptionalDate(row, "instruction_confirmed_at", RunDetailsFile)
                });

    private static Dictionary<int, IReadOnlyDictionary<string, decimal>> ReadLapcapCosts(string text) =>
        ReadRows(text, LapcapFile, "run_id", "unique_reference", "total_cost")
            .GroupBy(row => Int(row, "run_id", LapcapFile))
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<string, decimal>)g.ToDictionary(
                    row => row["unique_reference"].Trim(),
                    row => decimal.TryParse(row["total_cost"].Trim(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var cost)
                        ? cost
                        : throw new RunImportPackageException($"{LapcapFile} has '{row["total_cost"]}' for total_cost, which isn't a number")));

    /// <summary>Today's classification for production's, which said whether the billing file had been sent ("… RUN COMPLETED").</summary>
    private static Dictionary<int, IReadOnlyList<RunOrganisation>> ReadOrganisations(string text) =>
        ReadRows(text, OrganisationsFile, "run_id", "organisation_id", "subsidiary_id", "obligation_status")
            .GroupBy(row => Int(row, "run_id", OrganisationsFile))
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<RunOrganisation>)[.. g.Select(row => new RunOrganisation(
                    Int(row, "organisation_id", OrganisationsFile),
                    Text(row, "subsidiary_id"),
                    Optional(row, "organisation_name"),
                    Optional(row, "trading_name"),
                    Optional(row, "status_code"),
                    Optional(row, "joiner_date"),
                    Optional(row, "leaver_date"),
                    Optional(row, "num_days_obligated") is null ? null : Int(row, "num_days_obligated", OrganisationsFile),
                    row["obligation_status"].Trim()))]);

    private static RunClassification Classification(string status) => status.Trim().ToUpperInvariant() switch
    {
        "INITIAL RUN" or "INITIAL RUN COMPLETED" or "INITIAL"                   => RunClassification.Initial,
        "RECALCULATION RUN" or "RECALCULATION RUN COMPLETED" or "RECALCULATION" => RunClassification.Recalculation,
        "TEST RUN" or "TEST"                                                    => RunClassification.Test,
        _ => throw new RunImportPackageException($"{RunDetailsFile} has the classification '{status}', which the import doesn't know")
    };

    private static Dictionary<int, IReadOnlyDictionary<int, BillingDecision>> ReadBillingDecisions(string text) =>
        ReadRows(text, BillingInstructionsFile, "run_id", "producer_id", "accept_reject", "accept_reject_by", "accept_reject_at")
            .GroupBy(row => Int(row, "run_id", BillingInstructionsFile))
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<int, BillingDecision>)g.ToDictionary(
                    row => Int(row, "producer_id", BillingInstructionsFile),
                    row => new BillingDecision(row["accept_reject"].Trim(), Text(row, "accept_reject_by"), Date(row, "accept_reject_at", BillingInstructionsFile))));

    [GeneratedRegex(@"^(?<runId>\d+)-.*_Results File_\d{8}\.csv$")]
    private static partial Regex ResultsPattern();

    [GeneratedRegex(@"^(?<runId>\d+)-.*_Billing File_\d{12}(?<authorised>_AUTHORISED)?\.csv$")]
    private static partial Regex BillingPattern();
}
