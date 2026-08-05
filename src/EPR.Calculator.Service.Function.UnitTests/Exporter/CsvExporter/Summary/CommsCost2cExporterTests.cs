using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class CommsCost2cExporterTests
{
    private readonly IProducerFeesPartExporter exporter = new CommsCost2cExporter();

    [TestMethod]
    public void CommsCost2cExporter_Export_CSV()
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
            ["2c Comms Costs - by Country w/o Bad Debt provision",
             "Bad Debt provision",
             "2c Comms Costs - by Country with Bad Debt provision",
             null,
             null,
             null,
             null
            ],
            ["£1339.10", "£80.35", "£1419.45", null, null, null, null],
            ["2c Total Producer Fee for Comms Costs - by Country In proportion to Producer Tonnage w/o Bad Debt provision",
             "Bad Debt Provision for 2c",
             "2c Total Producer Fee for Comms Costs - by Country In proportion to Producer Tonnage with Bad Debt provision",
             "England Total with Bad Debt provision",
             "Wales Total with Bad Debt provision",
             "Scotland Total with Bad Debt provision",
             "Northern Ireland Total with Bad Debt provision"],
            ["£1339.10", "£80.35", "£1419.45", "£607.47", "£300.73", "£360.88", "£150.37"],
            ["£1339.10", "£80.35", "£1419.45", "£607.47", "£300.73", "£360.88", "£150.37"]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 7);
    }
}
