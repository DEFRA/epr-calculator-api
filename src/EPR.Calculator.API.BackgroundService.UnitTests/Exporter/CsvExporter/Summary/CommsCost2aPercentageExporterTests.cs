using System.Text;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class CommsCost2aPercentageExporterTests
{
    private readonly IProducerFeesPartExporter exporter = new CommsCost2aPercentageExporter();

    [TestMethod]
    public void CommsCost2aPercentageExporterTests_Export_CSV()
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
            [null],
            [null],
            ["Percentage of Producer Tonnage vs All Producers"],
            ["5.67415285%"],
            ["5.67415285%"]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 1);
    }
}
