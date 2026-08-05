using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class OnePlus2a2b2cExporterTests
{
    private readonly IProducerFeesPartExporter exporter = new OnePlus2a2b2cExporter();

    [TestMethod]
    public void OnePlus2a2b2cExporter_Export_CSV()
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
            ["Total (1+2a+2b+2c) with Bad Debt provision", null],
            ["£10491.17", null],
            ["Producer Total (1+2a+2b+2c) with Bad Debt provision",
             "Producer Percentage of Overall Producer Cost for (1+2a+2b+2c)"],
            ["£10491.17", "4.73419134%"],
            ["£10491.17", "4.73419134%"]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 2);
    }
}
