using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class Section1DisposalFeeExporterTests
{
    private readonly ICalcResultSummaryPartExporter exporter = new Section1DisposalFeeExporter();

    [TestMethod]
    public void Section1DisposalFeeExporter_Export_CSV()
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
            new string?[9],
            ["Disposal Fee Summary", null, null, null, null, null, null, null, null],
            ["1 Total Producer Disposal Fee w/o Bad Debt Provision",
             "Bad Debt Provision",
             "1 Total Producer Disposal Fee with Bad Debt Provision",
             "England Total",
             "Wales Total",
             "Scotland Total",
             "Northern Ireland Total",
             "Tonnage Change Count",
             "Tonnage Change Advice"],
            ["£4423.39", "£265.40", "£4688.80", "£2534.24", "£571.24", "£1137.87", "£445.45", "0", ""]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 9);
    }
}
