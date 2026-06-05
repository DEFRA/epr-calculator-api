using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class ProducerIdentityExporterTests
{
    private readonly ICalcResultSummaryPartExporter exporter = new ProducerIdentityExporter();

    [TestMethod]
    public void ProducerIdentityExporter_Export_CSV()
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
            new string?[10],
            new string?[10],
            ["Producer ID",
             "Subsidiary ID",
             "Producer / Subsidiary Name",
             "Trading Name",
             "Level",
             "Scaled-up tonnages?",
             "Partial Calculation?",
             "Registration Status Code",
             "Joiners Date",
             "Leavers Date"
            ],
            ["1",
             "",
             "Allied Packaging",
             null,
             "1",
             "No",
             "No",
             null,
             null,
             null
            ]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 10);
    }
}
