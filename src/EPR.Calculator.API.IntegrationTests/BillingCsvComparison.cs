using System.Globalization;
using EPR.Calculator.API.BackgroundService.Features.RunImport;

namespace EPR.Calculator.API.IntegrationTests;

/// <summary>
///     Compares two billing CSVs cell by cell, matching cells by section, row and column heading rather than position, so
///     files in different layouts can be compared. Reports counts and column names only, never values.
/// </summary>
public static class BillingCsvComparison
{
    public record Result(IReadOnlyDictionary<string, SectionCounts> Sections, IReadOnlyList<string> DifferingColumns, IReadOnlyList<string> SectionDifferences, IReadOnlyList<string> OnlyInOneFile)
    {
        public override string ToString() =>
            string.Join(Environment.NewLine, Sections.Select(s => $"{s.Key}: {s.Value}"))
            + Environment.NewLine + "Differing columns:" + Environment.NewLine + string.Join(Environment.NewLine, DifferingColumns.Select(c => "  " + c))
            + Environment.NewLine + "Differences outside producer rows (original => export):" + Environment.NewLine + string.Join(Environment.NewLine, SectionDifferences.Select(c => "  " + c))
            + Environment.NewLine + "Columns only in one file:" + Environment.NewLine + string.Join(Environment.NewLine, OnlyInOneFile.Select(c => "  " + c));
    }

    public record SectionCounts(int Same, int SameNumber, int Different, int OnlyInOriginal, int OnlyInExport)
    {
        public override string ToString() =>
            $"same {Same}, same number differently formatted {SameNumber}, different {Different}, only in original {OnlyInOriginal}, only in export {OnlyInExport}";
    }

    public static Result Compare(BillingCsv original, BillingCsv export)
    {
        var a = Cells(original);
        var b = Cells(export);
        var sections = new Dictionary<string, (int same, int number, int diff, int onlyA, int onlyB)>();
        var differing = new Dictionary<string, int>();
        var sectionDifferences = new List<string>();
        var onlyIn = new Dictionary<string, int>();

        foreach (var key in a.Keys.Union(b.Keys))
        {
            var section = key.Split('|')[0];
            var counts = sections.GetValueOrDefault(section);
            var column = string.Join('|', key.Split('|').Where((_, i) => i != 1));

            if (!b.TryGetValue(key, out var exported))
            {
                counts.onlyA++;
                onlyIn[$"original: {column}"] = onlyIn.GetValueOrDefault($"original: {column}") + 1;
                if (section == "header") sectionDifferences.Add($"only in original {key}: {a[key]}");
            }
            else if (!a.TryGetValue(key, out var printed))
            {
                counts.onlyB++;
                onlyIn[$"export: {column}"] = onlyIn.GetValueOrDefault($"export: {column}") + 1;
                if (section == "header") sectionDifferences.Add($"only in export {key}: {exported}");
            }
            else if (printed == exported || NormaliseValue(printed) == NormaliseValue(exported)) counts.same++;
            else if (AsNumber(printed) is { } x && AsNumber(exported) is { } y && x == y) counts.number++;
            else
            {
                counts.diff++;
                differing[column] = differing.GetValueOrDefault(column) + 1;

                // Producer rows are keyed by producer ID, so only whole-run figures and labels are shown with their values.
                if (!key.Split('|')[1].Contains(':'))
                    sectionDifferences.Add($"{key}: {printed} => {exported}");
            }

            sections[section] = counts;
        }

        return new Result(
            sections.OrderBy(s => s.Key).ToDictionary(s => s.Key, s => new SectionCounts(s.Value.same, s.Value.number, s.Value.diff, s.Value.onlyA, s.Value.onlyB)),
            [.. differing.OrderByDescending(d => d.Value).Select(d => $"{d.Key} x{d.Value}")],
            sectionDifferences,
            [.. onlyIn.OrderByDescending(o => o.Value).Select(o => $"{o.Key} x{o.Value}")]);
    }

    /// <summary>Every non-empty cell, keyed "section|row|column".</summary>
    private static Dictionary<string, string> Cells(BillingCsv csv)
    {
        var cells = new Dictionary<string, string>();
        var section = "header";
        var rowLabels = new Dictionary<string, int>();
        string[]? groups = null, subgroups = null, headers = null, sectionHeaders = null;

        // The two rows above each "Producer ID" header hold its section headings and the section totals, so they're keyed by column.
        var headerRows = Enumerable.Range(0, csv.Rows.Count).Where(r => csv.Cell(r, 0) == "Producer ID").ToHashSet();

        for (var r = 0; r < csv.Rows.Count; r++)
        {
            if (headerRows.Contains(r + 1) || headerRows.Contains(r + 2))
                continue;

            var first = csv.Cell(r, 0);
            var filled = Enumerable.Range(0, csv.Rows[r].Length).Count(c => csv.Cell(r, c).Length > 0);

            if (filled == 1 && first.Length > 0 && !first.Any(char.IsDigit) && r > 8)
            {
                section = Normalise(first);
                rowLabels.Clear();
                headers = null;
                sectionHeaders = null;
                continue;
            }

            if (first == "Producer ID")
            {
                headers = Forward(csv, r, fill: false);
                subgroups = Forward(csv, r - 1, fill: true, skipValues: true);
                groups = Forward(csv, r - 2, fill: true);

                for (var c = 0; c < headers.Length; c++)
                    if (csv.Cell(r - 1, c) is { Length: > 0 } total && (total.StartsWith('£') || total.EndsWith('%')))
                        cells.TryAdd($"{section}|(section totals)|{groups[c]}/{headers[c]}", total);
                continue;
            }

            if (filled == 0) continue;

            // A small section's first row names its columns, which moved when the RAM split was added.
            if (headers is null && sectionHeaders is null && first is "Material" or "")
            {
                sectionHeaders = [.. Enumerable.Range(0, csv.Rows[r].Length).Select(c => Normalise(csv.Cell(r, c)))];
                continue;
            }

            string rowKey;
            if (headers is not null && int.TryParse(first, out _))
                // Scaled-up producers have a row per submission period.
                rowKey = string.Join(':', Enumerable.Range(0, Math.Min(headers.Length, 10))
                    .Where(c => c is 0 or 1 || headers[c] is "level" or "submission period code")
                    .Select(c => csv.Cell(r, c)));
            else
            {
                var label = Normalise(first.Length > 0 ? first : "(blank)");
                rowLabels[label] = rowLabels.GetValueOrDefault(label) + 1;
                rowKey = rowLabels[label] == 1 ? label : $"{label}#{rowLabels[label]}";
            }

            for (var c = 1; c < csv.Rows[r].Length; c++)
            {
                if (csv.Cell(r, c) is not { Length: > 0 } value) continue;

                var column = headers is not null && c < headers.Length
                    ? $"{groups![c]}/{subgroups![c]}/{headers[c]}"
                    : sectionHeaders is not null && c < sectionHeaders.Length && sectionHeaders[c].Length > 0
                        ? sectionHeaders[c]
                        : $"#{c}";

                cells.TryAdd($"{section}|{rowKey}|{column}", value);
            }
        }

        return cells;
    }

    private static string[] Forward(BillingCsv csv, int row, bool fill, bool skipValues = false)
    {
        var width = csv.Rows[row + (fill ? 0 : 0)].Length;
        var result = new string[Math.Max(width, csv.Rows[Math.Min(row + 2, csv.Rows.Count - 1)].Length)];
        var current = "";
        for (var c = 0; c < result.Length; c++)
        {
            var text = csv.Cell(row, c);
            if (skipValues && (text.StartsWith('£') || text.EndsWith('%'))) text = "";
            if (text.Length > 0 || !fill) current = Normalise(text);
            result[c] = current;
        }
        return result;
    }

    // Headings reworded between layouts, so they match when comparing an older file with today's export.
    private static string Normalise(string text)
    {
        var normalised = ProducerSectionsParser.Normalise(text)
            .Replace("joiners date", "joiner's date")
            .Replace("leavers date", "leaver's date")
            .Replace(" %s", " %")
            .Replace("scheme set up cost yearly cost", "scheme annual set-up cost")
            .Replace("total uk", "total")
            .Replace("amount £s", "amount £")
            .Replace("disposal cost price per tonne", "disposal cost per tonne")
            .Replace("producer household tonnage", "household tonnage");

        return normalised == "late reporting tonnage" ? "total late reporting tonnage" : normalised;
    }

    private static string NormaliseValue(string value) => AsNumber(value) is null ? Normalise(value) : value;

    private static decimal? AsNumber(string text) =>
        decimal.TryParse(text.Replace("£", "").Replace(",", "").TrimEnd('%'), NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : null;
}
