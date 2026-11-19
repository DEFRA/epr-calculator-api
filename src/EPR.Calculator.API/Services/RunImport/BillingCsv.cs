using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace EPR.Calculator.API.Services.RunImport;

/// <summary>
///     The rows of a results or billing CSV, with the lookups and value parsing the layouts share.
/// </summary>
public sealed class BillingCsv(IReadOnlyList<string[]> rows, IReadOnlySet<(int Row, int Column)>? quotedEmptyCells = null)
{
    public IReadOnlyList<string[]> Rows { get; } = rows;

    public static BillingCsv Read(TextReader reader)
    {
        var rows = new List<string[]>();
        var quotedEmptyCells = new HashSet<(int, int)>();
        while (ReadRecord(reader, rows.Count, quotedEmptyCells) is { } record)
            rows.Add(record);
        return new BillingCsv(rows, quotedEmptyCells);
    }

    public string Cell(int row, int column) =>
        row < Rows.Count && column < Rows[row].Length ? Clean(Rows[row][column]) : string.Empty;

    /// <summary>The cell's text; an empty cell is null unless it's quoted, which the export writes for an empty string.</summary>
    public string? Text(int row, int column) =>
        Cell(row, column) is { Length: > 0 } text ? text
        : quotedEmptyCells?.Contains((row, column)) == true ? string.Empty
        : null;

    /// <summary>The first row at or after <paramref name="start" /> whose first cell starts with <paramref name="label" />.</summary>
    public int FindRow(string label, int start = 0)
    {
        for (var i = start; i < Rows.Count; i++)
            if (Cell(i, 0).StartsWith(label, StringComparison.OrdinalIgnoreCase))
                return i;

        throw new FormatException($"No row starting '{label}' after row {start}");
    }

    public int? TryFindRow(string label, int start = 0)
    {
        for (var i = start; i < Rows.Count; i++)
            if (Cell(i, 0).StartsWith(label, StringComparison.OrdinalIgnoreCase))
                return i;

        return null;
    }

    public decimal Number(int row, int column) =>
        NullableNumber(row, column) ?? throw new FormatException($"Expected a number at row {row}, column {column}");

    public decimal? NullableNumber(int row, int column)
    {
        var text = Cell(row, column).Replace("£", "").Replace(",", "").TrimEnd('%');
        return text is "" or "-" ? null : decimal.Parse(text, NumberStyles.Number, CultureInfo.InvariantCulture);
    }

    public DateTime? Date(int row, int column)
    {
        var text = Cell(row, column);
        return text is "" or "NA" ? null : DateTime.ParseExact(text, ["dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm", "dd/MM/yyyy"], CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    // CsvSanitiser adds a left-to-right mark to stop spreadsheets reading a field as a formula.
    private static string Clean(string value) => value.Replace("‎", "").Trim();

    [SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "A CSV reader's branches are the format's rules; splitting them up would hide them.")]
    private static string[]? ReadRecord(TextReader reader, int row, HashSet<(int, int)> quotedEmptyCells)
    {
        if (reader.Peek() < 0)
            return null;

        var fields = new List<string>();
        var field = new System.Text.StringBuilder();
        var quoted = false;
        var wasQuoted = false;

        void EndField()
        {
            if (wasQuoted && field.Length == 0)
                quotedEmptyCells.Add((row, fields.Count));
            fields.Add(field.ToString());
            field.Clear();
            wasQuoted = false;
        }

        while (true)
        {
            var c = reader.Read();
            if (c < 0)
                break;

            if (quoted)
            {
                if (c == '"' && reader.Peek() == '"') { reader.Read(); field.Append('"'); }
                else if (c == '"') quoted = false;
                else field.Append((char)c);
                continue;
            }

            if (c == '"') quoted = wasQuoted = true;
            else if (c == ',') EndField();
            else if (c == '\n') break;
            else if (c != '\r') field.Append((char)c);
        }

        EndField();
        return [.. fields];
    }
}
