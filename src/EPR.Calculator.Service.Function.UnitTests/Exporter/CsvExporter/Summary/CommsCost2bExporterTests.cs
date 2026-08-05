using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class CommsCost2bExporterTests
{
    private readonly IProducerFeesPartExporter exporter = new CommsCost2bExporter();

    [TestMethod]
    public void CommsCost2bExporterTests_Export_CSV()
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
            ["2b Comms Costs - UK wide w/o Bad Debt provision",
             "Bad Debt provision",
             "2b Comms Costs - UK wide with Bad Debt provision",
             null,
             null,
             null,
             null
            ],
            ["£2844.06", "£170.64", "£3014.70", null, null, null, null],
            ["2b Total Producer Fee for Comms Costs - UK wide In proportion to Producer Tonnage w/o Bad Debt provision",
             "Bad Debt Provision for 2b",
             "2b Total Producer Fee for Comms Costs - UK wide In proportion to Producer Tonnage with Bad Debt provision",
             "England Total with Bad Debt provision",
             "Wales Total with Bad Debt provision",
             "Scotland Total with Bad Debt provision",
             "Northern Ireland Total with Bad Debt provision"],
            ["£2844.06", "£170.64", "£3014.70", "£1582.51", "£399.40", "£733.39", "£299.39"],
            ["£2844.06", "£170.64", "£3014.70", "£1582.51", "£399.40", "£733.39", "£299.39"]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 7);
    }
}
