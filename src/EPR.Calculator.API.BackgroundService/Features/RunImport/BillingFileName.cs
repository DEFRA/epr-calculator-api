using System.Globalization;
using System.Text.RegularExpressions;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter;

namespace EPR.Calculator.API.BackgroundService.Features.RunImport;

/// <summary>
///     What a billing CSV's name holds: "{run id}-{run name}_Billing File_{yyyyMMddHHmm}.csv", with "_AUTHORISED" added once
///     the file was sent to FSS.
/// </summary>
public sealed partial record BillingFileName(int RunId, DateTime CreatedAt, bool IsAuthorised, string CsvFileName, string JsonFileName)
{
    private const string AuthorisedSuffix = "_AUTHORISED";

    public static BillingFileName Parse(string fileName)
    {
        var match = Pattern().Match(Path.GetFileName(fileName));
        if (!match.Success)
            throw new FormatException($"'{fileName}' isn't a billing file name");

        var runId = int.Parse(match.Groups["runId"].Value, CultureInfo.InvariantCulture);
        var isAuthorised = match.Groups["authorised"].Success;

        return new BillingFileName(
            runId,
            DateTime.ParseExact(match.Groups["timestamp"].Value, "yyyyMMddHHmm", CultureInfo.InvariantCulture),
            isAuthorised,
            Path.GetFileName(fileName).Replace(AuthorisedSuffix, ""),
            new CalcResultsAndBillingFileName(runId));
    }

    [GeneratedRegex(@"^(?<runId>\d+)-.*_Billing File_(?<timestamp>\d{12})(?<authorised>_AUTHORISED)?\.csv$")]
    private static partial Regex Pattern();
}
