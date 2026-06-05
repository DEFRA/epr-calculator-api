using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class LaDataPrepCostsExporterTests
{
    private readonly ICalcResultSummaryPartExporter exporter = new LaDataPrepCostsExporter();

    [TestMethod]
    public void LaDataPrepCostsExporter_Export_CSV()
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
            ["4 LA Data Prep Costs w/o Bad Debt provision",
             "Bad Debt provision",
             "4 LA Data Prep Costs with Bad Debt provision",
             null,
             null,
             null,
             null
            ],
            ["£1727.98", "£103.68", "£1831.66", null, null, null, null],
            ["4 Total Producer Fee for LA Data Prep Costs In proportion to Percentage of Overall Producer Cost of (1+2a+2b+2c) w/o Bad Debt provision",
             "Bad Debt Provision for 4",
             "4 Total Producer Fee for LA Data Prep Costs In proportion to Percentage of Overall Producer Cost of (1+2a+2b+2c) with Bad Debt provision",
             "England Total with Bad Debt provision",
             "Wales Total with Bad Debt provision",
             "Scotland Total with Bad Debt provision",
             "Northern Ireland Total with Bad Debt provision"],
            ["£1727.98", "£103.68", "£1831.66", "£802.92", "£351.28", "£451.64", "£225.82"]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 7);
    }
}
