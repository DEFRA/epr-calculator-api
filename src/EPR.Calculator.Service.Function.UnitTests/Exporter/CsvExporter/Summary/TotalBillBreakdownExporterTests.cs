using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class TotalBillBreakdownExporterTests
{
    private readonly ICalcResultSummaryPartExporter exporter = new TotalBillBreakdownExporter();

    [TestMethod]
    public void TotalBillBreakdownExporter_Export_CSV()
    {
        // Arrange
        var materials = TestDataHelper.GetMaterialDetails();
        const bool applyModulation = false;
        var resultSummary = TestDataHelper.GetCalcResultSummary();
        var csvContent = new StringBuilder();

        // Act
        SummaryExporterTestUtils.Render(exporter, materials, applyModulation, resultSummary, csvContent);
        var result = csvContent.ToString().ReplaceLineEndings("\n").Split("\n").ToArray();
        Console.WriteLine(string.Join("\n", result));

        var expected = new string?[][] {
            ["Total Producer Bill Breakdown", null, null, null, null, null, null],
            new string?[7],
            ["Total Producer Bill (1+2a+2b+2c+3+4+5) w/o Bad Debt Provision",
             "Bad Debt Provision for Total Producer Bill",
             "Total Producer Bill (1+2a+2b+2c+3+4+5) with Bad Debt Provision",
             "England Total with Bad Debt provision",
             "Wales Total with Bad Debt provision",
             "Scotland Total with Bad Debt provision",
             "Northern Ireland Total with Bad Debt provision"],
            ["£9897.33", "£593.84", "£10491.17", "£5442.45", "£1452.64", "£2564.98", "£1031.09"]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 7);
    }
}
