using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class Section1DisposalExporterTests
{
    private readonly ICalcResultSummaryPartExporter exporter = new Section1DisposalExporter();

    [TestMethod]
    public void Section1DisposalExporter_Export_CSV()
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
            ["1 Fee for LA Disposal Costs w/o Bad Debt provision",
             "Bad Debt Provision",
             "1 Fee for LA Disposal Costs with Bad Debt provision",
             null,
             null,
             null,
             null
            ],
            ["£4423.39", "£6021.37", "£4688.80", null, null, null, null],
            ["1 Total Producer Fee for LA Disposal Costs w/o Bad Debt provision",
             "Bad Debt Provision for 1",
             "1 Total Producer Fee for LA Disposal Costs with Bad Debt provision",
             "England Total with Bad Debt provision",
             "Wales Total with Bad Debt provision",
             "Scotland Total with Bad Debt provision",
             "Northern Ireland Total with Bad Debt provision"],
            ["£4423.39", "£265.40", "£4688.80", "£2534.24", "£571.24", "£1137.87", "£445.45"]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 7);
    }
}
