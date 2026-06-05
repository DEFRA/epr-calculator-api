using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class CommsCost2aPercentageExporterTests
{
    private readonly ICalcResultSummaryPartExporter exporter = new CommsCost2aPercentageExporter();

    [TestMethod]
    public void CommsCost2aPercentageExporterTests_Export_CSV()
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
            [null],
            [null],
            ["Percentage of Producer Tonnage vs All Producers"],
            ["5.67415285%"]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 1);
    }
}
