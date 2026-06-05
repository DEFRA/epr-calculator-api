using System.Text;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    public static class CsvTestUtils {

        public static string[][] GetRows(StringBuilder csvContent){
            return csvContent.ToString()
                        .Split(Environment.NewLine)
                        .Select(r => r.Split(','))
                        .ToArray();
        }
        public static Dictionary<string, List<string>> GetColumnHeaderValues(string[] headers, string[] values)
        {
            return headers
                    .Select((h, i) => new
                    {
                        Key = h?.Trim().Trim('"'),
                        Value = (i < values.Length ? values[i] : null)?
                                    .Trim()
                                    .Trim('"')
                                ?? string.Empty
                    })
                    .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                    .GroupBy(x => x.Key!)
                    .ToDictionary(
                        g => g.Key!,
                        g => g.Select(x => x.Value).ToList()
                    );
        }

        public static int[] FindAllHeaderIndexes(string[] columnHeaders, string headerToFind)
        {
            return columnHeaders
                        .Select((h, i) => new { h = h?.Trim().Trim('"'), i = i })
                        .Where(x => !string.IsNullOrWhiteSpace(x.h))
                        .Where(x => x.h!.Equals(headerToFind))
                        .Select(x => x.i)
                        .ToArray();
        }

        public static void AssertCsv(string?[][] expected, string[] actual)
        {
            var expected2 =
                expected.Select(row => string.Join(",", row.Select(cell => cell == null ? "" : $"\"{cell}\""))).ToArray();

            Assert.AreEqual(expected2.Length, actual.Length, $"CSV sizes differ\nExpected: {expected2}\nActual: {actual}");

            for (int i = 0; i < expected2.Length; i++)
            {
                var expectedRow = expected2[i];
                var actualRow = actual[i];
                Assert.AreEqual(expectedRow, actualRow, $"Row {i} differs:\nExpected: {expectedRow}\nActual  : {actualRow}");
            }
        }

        public static void AssertSquareCsv(string?[][] expected, string[] actual, int expectedLength)
        {
            var expected2 =
                expected.Select(row => string.Join(",", row.Select(cell => cell == null ? "" : $"\"{cell}\""))).ToArray();

            Assert.AreEqual(expected2.Length, actual.Length, $"CSV sizes differ\nExpected: {expected2}\nActual: {actual}");

            for (int i = 0; i < expected2.Length; i++)
            {
                var expectedRow = expected2[i];
                Assert.IsTrue(actual[i].EndsWith(','), $"Row {i} does not end with a trailing comma:\n{actual[i]}");
                var actualRow = actual[i][..^1];
                Assert.AreEqual(expectedLength, actualRow.Split(",").Length);
                Assert.AreEqual(expectedRow, actualRow, $"Row {i} differs:\nExpected: {expectedRow}\nActual  : {actualRow}");
            }
        }
    }
}
