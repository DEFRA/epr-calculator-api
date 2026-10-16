using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EPR.CommonDataService.DataApi.ObligationDetermination.UnitTests;

/// <summary>
///     Parses the shared business-rule scenario CSV from epr-data's epr-obligation-determination test
///     suite (src/test/myc-obligation-determination-test-cases.csv), mirroring
///     load_myc_obligation_determination_test_cases.py's parsing rules exactly so the same scenarios can
///     be run against the C# port. Each scenario ("sub-title") groups one or more registration rows and
///     their expected obligation outcome.
/// </summary>
public static partial class ObligationTestCaseLoader
{
    public sealed record TestCase(string Title, string SubTitle, IReadOnlyList<TestRecord> Records)
    {
        public override string ToString() => SubTitle;
    }

    public sealed record TestRecord(
        int OrganisationId,
        string? SubsidiaryId,
        string? ComplianceScheme,
        string OrganisationName,
        string? StatusCode,
        string? JoinerDate,
        string ExpectedObligation,
        short? ExpectedNumDaysObligated,
        string? ExpectedErrorCode,
        int SubmissionPeriodYear)
    {
        public string ExpectedObligationStatus => ExpectedObligation[..1];
    }

    [GeneratedRegex(@"^[0-9]+[ ]+.*")]
    private static partial Regex TitlePattern();

    [GeneratedRegex(@"^[0-9]+\.[0-9]+[ ]+.*")]
    private static partial Regex SubTitlePattern();

    public static IReadOnlyList<TestCase> Load(string csvPath)
    {
        var lines = File.ReadAllLines(csvPath);
        var cases = new List<TestCase>();
        var records = new List<TestRecord>();
        var lastTitle = "";
        var lastSubTitle = "";

        void Flush()
        {
            if (records.Count == 0)
            {
                return;
            }

            cases.Add(new TestCase(lastTitle, lastSubTitle, records.ToList()));
            records.Clear();
        }

        foreach (var line in lines.Skip(1)) // skip header
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var cols = SplitCsvLine(line);
            if (cols.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var col0 = cols[0].Trim();

            if (TitlePattern().IsMatch(col0))
            {
                Flush();
                lastTitle = col0;
                continue;
            }

            if (SubTitlePattern().IsMatch(col0))
            {
                Flush();
                lastSubTitle = col0;
                continue;
            }

            records.Add(ParseRecord(cols));
        }

        Flush();
        return cases;
    }

    private static TestRecord ParseRecord(string[] cols)
    {
        var organisationId = int.Parse(Field(cols, 0), CultureInfo.InvariantCulture);
        var subsidiaryId = NullIfEmpty(Field(cols, 1));
        var complianceScheme = NullIfEmpty(Field(cols, 2));
        var name = Field(cols, 3);
        var statusCode = NullIfEmpty(Field(cols, 4));
        var date = Field(cols, 5);
        var expected = Field(cols, 6);
        var additional = Field(cols, 7);
        var submissionPeriodYear = int.TryParse(Field(cols, 8), out var year) ? year : 2024;

        var joinerDate = string.IsNullOrEmpty(date)
            ? null
            : DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        string expectedObligation;
        short? expectedNumDaysObligated;
        string? expectedErrorCode;

        if (expected == "Obligated")
        {
            expectedObligation = "Obligated";
            expectedNumDaysObligated = null;
            expectedErrorCode = NullIfEmpty(additional);
        }
        else if (expected.StartsWith("Partial", StringComparison.Ordinal))
        {
            expectedObligation = "Obligated";
            expectedNumDaysObligated = string.IsNullOrEmpty(additional) ? null : short.Parse(additional, CultureInfo.InvariantCulture);
            expectedErrorCode = null;
        }
        else
        {
            expectedObligation = expected;
            expectedNumDaysObligated = null;
            expectedErrorCode = NullIfEmpty(additional);
        }

        return new TestRecord(
            organisationId,
            subsidiaryId,
            complianceScheme,
            name,
            statusCode,
            joinerDate,
            expectedObligation,
            expectedNumDaysObligated,
            expectedErrorCode,
            submissionPeriodYear);
    }

    private static string Field(string[] cols, int index) => index < cols.Length ? cols[index].Trim() : "";

    private static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s;

    private static string[] SplitCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == ',')
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());
        return fields.ToArray();
    }
}
