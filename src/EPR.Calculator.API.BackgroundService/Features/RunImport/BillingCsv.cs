using System.Globalization;

namespace EPR.Calculator.API.BackgroundService.Features.RunImport;

/// <summary>
///     The rows of a results or billing CSV, with the lookups and value parsing the layouts share.
/// </summary>
public sealed class BillingCsv(IReadOnlyList<string[]> rows)
{
    public IReadOnlyList<string[]> Rows { get; } = rows;

    public static BillingCsv Read(TextReader reader)
    {
        var rows = new List<string[]>();
        while (ReadRecord(reader) is { } record)
            rows.Add(record);
        return new BillingCsv(rows);
    }

    public string Cell(int row, int column) =>
        row < Rows.Count && column < Rows[row].Length ? Clean(Rows[row][column]) : string.Empty;

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

    private static string[]? ReadRecord(TextReader reader)
    {
        if (reader.Peek() < 0)
            return null;

        var fields = new List<string>();
        var field = new System.Text.StringBuilder();
        var quoted = false;

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

            if (c == '"') quoted = true;
            else if (c == ',') { fields.Add(field.ToString()); field.Clear(); }
            else if (c == '\n') break;
            else if (c != '\r') field.Append((char)c);
        }

        fields.Add(field.ToString());
        return [.. fields];
    }
}
