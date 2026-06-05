using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class Section2aMaterialsExporterTests
{
    private readonly ICalcResultSummaryPartExporter exporter = new Section2aMaterialsExporter();

    [TestMethod]
    public void Section2aMaterialsExporter_Export_CSV_Aluminium()
    {
        // Arrange
        var materials = TestDataHelper.GetMaterialDetails().Where(m => m.Code == "AL").ToList();
        const bool applyModulation = false;
        var resultSummary = TestDataHelper.GetCalcResultSummary();
        var producer = resultSummary.ProducerDisposalFees.First();
        producer.ProducerCommsFeesByMaterial =
            producer.ProducerCommsFeesByMaterial!
                .Where(kv => kv.Key == "AL")
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        var csvContent = new StringBuilder();

        // Act
        SummaryExporterTestUtils.Render(exporter, materials, applyModulation, resultSummary, csvContent);
        var result = csvContent.ToString().ReplaceLineEndings("\n").Split("\n").ToArray();
        Console.WriteLine(string.Join("\n", result));

        var expected = new string?[][] {
            ["2a Fees for Comms Costs - by Material with Bad Debt provision",
             null, null, null, null, null, null, null, null, null, null],
            ["Aluminium Breakdown",
             null, null, null, null, null, null, null, null, null, null],
            ["Household Packaging Tonnage",
             "Public Bin Tonnage",
             "Total Tonnage",
             "Price per Tonne",
             "Producer Total Cost w/o Bad Debt Provision",
             "Bad Debt Provision",
             "Producer Total Cost with Bad Debt Provision",
             "England with Bad Debt Provision",
             "Wales with Bad Debt Provision",
             "Scotland with Bad Debt Provision",
             "Northern Ireland with Bad Debt Provision"
            ],
            ["1000.000",
             "0.000",
             "0.000",
             "£0.1916",
             "£191.60",
             "£11.50",
             "£203.10",
             "£106.61",
             "£26.91",
             "£49.41",
             "£20.17"
            ]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 11);
    }

    [TestMethod]
    public void Section2aMaterialsExporter_Export_CSV_Glass()
    {
        // Arrange
        var materials = TestDataHelper.GetMaterialDetails().Where(m => m.Code == "GL").ToList();
        const bool applyModulation = false;
        var resultSummary = TestDataHelper.GetCalcResultSummary();
        var producer = resultSummary.ProducerDisposalFees.First();
        producer.ProducerCommsFeesByMaterial =
            producer.ProducerCommsFeesByMaterial!
                .Where(kv => kv.Key == "GL")
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        var csvContent = new StringBuilder();

        // Act
        SummaryExporterTestUtils.Render(exporter, materials, applyModulation, resultSummary, csvContent);
        var result = csvContent.ToString().ReplaceLineEndings("\n").Split("\n").ToArray();
        Console.WriteLine(string.Join("\n", result));

        var expected = new string?[][] {
            ["2a Fees for Comms Costs - by Material with Bad Debt provision",
             null, null, null, null, null, null, null, null, null, null, null],
            ["Glass Breakdown",
             null, null, null, null, null, null, null, null, null, null, null],
            ["Household Packaging Tonnage",
             "Public Bin Tonnage",
             "Household Drinks Containers Tonnage - Glass",
             "Total Tonnage",
             "Price per Tonne",
             "Producer Total Cost w/o Bad Debt Provision",
             "Bad Debt Provision",
             "Producer Total Cost with Bad Debt Provision",
             "England with Bad Debt Provision",
             "Wales with Bad Debt Provision",
             "Scotland with Bad Debt Provision",
             "Northern Ireland with Bad Debt Provision"
            ],
            ["500.000",
             "0.000",
             "0.000",
             "0.000",
             "£0.4404",
             "£220.20",
             "£13.21",
             "£233.41",
             "£122.53",
             "£30.92",
             "£56.78",
             "£23.18"
            ]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 12);
    }
}
