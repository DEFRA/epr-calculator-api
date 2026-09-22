using System.Text;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class SummaryExporterTests
{
    private readonly IProducerFeesPartExporter exporter = new SummaryExporter();

    [TestMethod]
    public void SummaryExporter_Export_CSV()
    {
        // Arrange
        var materials = TestDataHelper.GetMaterialDetails();
        const bool applyModulation = false;
        var producerFees = TestDataHelper.GetProducerFees();
        var csvContent = new StringBuilder();

        // Act
        ProducerFeesExporterTestUtils.Render(exporter, materials, applyModulation, producerFees, csvContent);
        var result = csvContent.ToString().ReplaceLineEndings("\n").Split("\n").ToArray();
        Console.WriteLine(string.Join("\n", result));

        var expected = new string?[][] {
            ["Summary", null, null, null, null, null],
            new string?[6],
            ["Current Year Invoiced Total To Date",
             "Tonnage Change Since Last Invoice",
             "Total Producer Bill (1+2a+2b+2c+3+4+5) with Bad Debt Provision",
             "% Liability Difference (Calc vs Prev)",
             "Suggested Billing Instruction",
             "Suggested Invoice Amount"],
            ["£1250.89", "Tonnage Changed", "£10491.17", "123.45%", "", "£4039.00"],
            ["-", "-", "£10491.17", "-", "", "£4039.00"]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 6);
    }
}
