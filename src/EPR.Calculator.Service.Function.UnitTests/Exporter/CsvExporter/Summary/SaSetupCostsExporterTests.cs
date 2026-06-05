using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class SaSetupCostsExporterTests
{
    private readonly ICalcResultSummaryPartExporter exporter = new SaSetupCostsExporter();

    [TestMethod]
    public void SaSetupCostsExporter_Export_CSV()
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
            ["5 One-off fee for SA Set Up Costs w/o Bad Debt provision",
             "Bad Debt provision",
             "5 One-off fee for SA Set Up Costs with Bad Debt provision",
             null,
             null,
             null,
             null
            ],
            ["£17500.00", "£1050.00", "£18550.00", null, null, null, null],
            ["5 Total Producer One-off fee for SA Set Up Costs In proportion to Percentage of Overall Producer Cost of (1+2a+2b+2c) w/o Bad Debt provision",
             "Bad Debt Provision for 5",
             "5 Total Producer One-off fee for SA Set Up Costs In proportion to Percentage of Overall Producer Cost of (1+2a+2b+2c) with Bad Debt provision",
             "England Total with Bad Debt provision",
             "Wales Total with Bad Debt provision",
             "Scotland Total with Bad Debt provision",
             "Northern Ireland Total with Bad Debt provision"],
            ["£2970.71", "£178.24", "£3148.95", "£1652.98", "£417.19", "£766.05", "£312.73"]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 7);
    }
}
