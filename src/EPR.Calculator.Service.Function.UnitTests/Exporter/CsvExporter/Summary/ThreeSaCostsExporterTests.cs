using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class ThreeSaCostsExporterTests
{
    private readonly ICalcResultSummaryPartExporter exporter = new ThreeSaCostsExporter();

    [TestMethod]
    public void ThreeSaCostsExporter_Export_CSV()
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
            ["3 SA Operating Costs w/o Bad Debt provision",
              "Bad Debt provision",
              "3 SA Operating Costs with Bad Debt provision",
              null,
              null,
              null,
              null
            ],
            ["£3077.22", "£3900.00", "£3261.86", null, null, null, null],
            ["3 Total Producer Fee for SA Operating Costs In proportion to Percentage of Overall Producer Cost of (1+2a+2b+2c) w/o Bad Debt provision",
             "Bad Debt provision for 3",
             "3 Total Producer Fee for SA Operating Costs In proportion to Percentage of Overall Producer Cost of (1+2a+2b+2c) with Bad Debt provision",
             "England Total with Bad Debt provision",
             "Wales Total with Bad Debt provision",
             "Scotland Total with Bad Debt provision",
             "Northern Ireland Total with Bad Debt provision"],
            ["£3077.22", "£184.63", "£3261.86", "£1712.25", "£432.15", "£793.52", "£323.94"]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 7);
    }
}
