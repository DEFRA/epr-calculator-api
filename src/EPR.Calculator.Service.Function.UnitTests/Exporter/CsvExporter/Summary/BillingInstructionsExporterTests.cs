using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class BillingInstructionsExporterTests
{
    private readonly IProducerFeesPartExporter exporter = new BillingInstructionsExporter();

    [TestMethod]
    public void BillingInstructionsExporter_Export_CSV()
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
            ["Calculation of Suggested Billing Instructions and Invoice Amounts",
             null, null, null, null, null, null, null, null, null],
            new string?[10],
            ["Current Year Invoiced Total To Date",
             "Tonnage Change Since Last Invoice",
             "Liability Difference (Calc vs Prev)",
             "Material £ Threshold Breached",
             "Tonnage £ Threshold Breached (if tonnage changed)",
             "% Liability Difference (Calc vs Prev)",
             "Material % Threshold Breached",
             "Tonnage % Threshold Breached (if tonnage changed)",
             "Suggested Billing Instruction",
             "Suggested Invoice Amount"],
            ["£1250.89", "Tonnage Changed", "£580.73", "-", "-", "123.45%", "-", "-", "", "£4039.00"],
            ["-", "-", "£580.73", "", "", "-", "", "", "", "£4039.00"]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 10);
    }
}
