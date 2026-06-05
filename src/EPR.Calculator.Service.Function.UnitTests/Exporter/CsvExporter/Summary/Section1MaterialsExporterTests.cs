using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class Section1MaterialsExporterTests
{
    private readonly ICalcResultSummaryPartExporter exporter = new Section1MaterialsExporter();

    [TestMethod]
    public void Section1MaterialsExporter_Export_CSV_Aluminium()
    {
        // Arrange
        var materials = TestDataHelper.GetMaterialDetails().Where(m => m.Code == "AL").ToList();
        const bool applyModulation = false;
        var resultSummary = TestDataHelper.GetCalcResultSummary();
        var producer = resultSummary.ProducerDisposalFees.First();
        producer.ProducerDisposalFeesByMaterial =
            producer.ProducerDisposalFeesByMaterial
                .Where(kv => kv.Key == "AL")
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        var csvContent = new StringBuilder();

        // Act
        SummaryExporterTestUtils.Render(exporter, materials, applyModulation, resultSummary, csvContent);
        var result = csvContent.ToString().ReplaceLineEndings("\n").Split("\n").ToArray();
        Console.WriteLine(string.Join("\n", result));

        var expected = new string?[][] {
            ["1 Producer Disposal Fees with Bad Debt Provision",
             null, null, null, null, null, null, null, null, null, null, null, null, null, null],
            ["Aluminium Breakdown",
             null, null, null, null, null, null, null, null, null, null, null, null, null, null],
            ["Previous Invoiced Tonnage",
             "Household Packaging Tonnage",
             "Public Bin Tonnage",
             "Total Tonnage",
             "Self Managed Consumer Waste Tonnage",
             "Net Tonnage",
             "Tonnage Change",
             "Price per Tonne",
             "Producer Disposal Fee w/o Bad Debt Provision",
             "Bad Debt Provision",
             "Producer Disposal Fee with Bad Debt Provision",
             "England with Bad Debt Provision",
             "Wales with Bad Debt Provision",
             "Scotland with Bad Debt Provision",
             "Northern Ireland with Bad Debt Provision"],
            ["-",
             "1000.000",
             "0.000",
             "0.000",
             "90.000",
             "910.000",
             "0.000",
             "£0.6676",
             "£607.52",
             "£36.45",
             "£643.97",
             "£348.06",
             "£78.46",
             "£156.28",
             "£61.18"]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 15);
    }

    [TestMethod]
    public void Section1MaterialsExporter_Export_CSV_Glass()
    {
        // Arrange — Glass has an extra "Household Drinks Containers Tonnage" column (16 columns total)
        var materials = TestDataHelper.GetMaterialDetails().Where(m => m.Code == "GL").ToList();
        const bool applyModulation = false;
        var resultSummary = TestDataHelper.GetCalcResultSummary();
        var producer = resultSummary.ProducerDisposalFees.First();
        producer.ProducerDisposalFeesByMaterial =
            producer.ProducerDisposalFeesByMaterial
                .Where(kv => kv.Key == "GL")
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        var csvContent = new StringBuilder();

        // Act
        SummaryExporterTestUtils.Render(exporter, materials, applyModulation, resultSummary, csvContent);
        var result = csvContent.ToString().ReplaceLineEndings("\n").Split("\n").ToArray();
        Console.WriteLine(string.Join("\n", result));

        var expected = new string?[][] {
            ["1 Producer Disposal Fees with Bad Debt Provision",
             null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
            ["Glass Breakdown",
             null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
            ["Previous Invoiced Tonnage",
             "Household Packaging Tonnage",
             "Public Bin Tonnage",
             "Household Drinks Containers Tonnage - Glass",
             "Total Tonnage",
             "Self Managed Consumer Waste Tonnage",
             "Net Tonnage",
             "Tonnage Change",
             "Price per Tonne",
             "Producer Disposal Fee w/o Bad Debt Provision",
             "Bad Debt Provision",
             "Producer Disposal Fee with Bad Debt Provision",
             "England with Bad Debt Provision",
             "Wales with Bad Debt Provision",
             "Scotland with Bad Debt Provision",
             "Northern Ireland with Bad Debt Provision"],
            ["0.000",
             "500.000",
             "0.000",
             "220.000",
             "0.000",
             "150.000",
             "350.000",
             "0.000",
             "£6.4404",
             "£2254.14",
             "£135.25",
             "£2389.39",
             "£1291.43",
             "£291.10",
             "£579.85",
             "£227.00"]
        };

        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 16);
    }

    [TestMethod]
    public void Section1MaterialsExporter_Export_CSV_Aluminium_WithModulation()
    {
        // Arrange — 49 columns with modulation
        var materials = TestDataHelper.GetMaterialDetails().Where(m => m.Code == "AL").ToList();
        const bool applyModulation = true;
        var resultSummary = TestDataHelper.GetCalcResultSummary(applyModulation);
        var producer = resultSummary.ProducerDisposalFees.First();
        producer.ProducerDisposalFeesByMaterial =
            producer.ProducerDisposalFeesByMaterial
                .Where(kv => kv.Key == "AL")
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        var csvContent = new StringBuilder();

        // Act
        SummaryExporterTestUtils.Render(exporter, materials, applyModulation, resultSummary, csvContent);
        var result = csvContent.ToString().ReplaceLineEndings("\n").Split("\n").ToArray();
        Console.WriteLine(string.Join("\n", result));

        string?[] sectionHeader = ["1 Producer Disposal Fees with Bad Debt Provision",
                                   .. Enumerable.Repeat<string?>(null, 48)];
        string?[] groupHeader   = ["Aluminium Breakdown",
                                   .. Enumerable.Repeat<string?>(null, 48)];

        string?[] columnHeaders = [
            "Previous Invoiced Tonnage",
            "Household Packaging Tonnage",
            "Household Red Material Tonnage",
            "Household Amber Material Tonnage",
            "Household Green Material Tonnage",
            "Household Red Medical Material Tonnage",
            "Household Amber Medical Material Tonnage",
            "Household Green Medical Material Tonnage",
            "Public Bin Tonnage",
            "Public Bin Red Material Tonnage",
            "Public Bin Amber Material Tonnage",
            "Public Bin Green Material Tonnage",
            "Public Bin Red Medical Material Tonnage",
            "Public Bin Amber Medical Material Tonnage",
            "Public Bin Green Medical Material Tonnage",
            "Total Tonnage",
            "Red Total Tonnage",
            "Amber Total Tonnage",
            "Green Total Tonnage",
            "Red Medical Total Tonnage",
            "Amber Medical Total Tonnage",
            "Green Medical Total Tonnage",
            "Red + Red Medical Total Tonnage",
            "Amber + Amber Medical Total Tonnage",
            "Green + Green Medical Total Tonnage",
            "Self Managed Consumer Waste Tonnage",
            "Actioned Self Managed Consumer Waste Tonnage",
            "Red + Red Medical Actioned Self Managed Consumer Waste Tonnage",
            "Amber + Amber Medical Actioned Self Managed Consumer Waste Tonnage",
            "Green + Green Medical Actioned Self Managed Consumer Waste Tonnage",
            "Net Tonnage",
            "Red + Red Medical Net Tonnage",
            "Amber + Amber Medical Net Tonnage",
            "Green + Green Medical Net Tonnage",
            "Residual SMCW",
            "Tonnage Change",
            "Red + Red Medical Material Price per Tonne",
            "Amber + Amber Medical Material Price per Tonne",
            "Green + Green Medical Material Price per Tonne",
            "Producer Red + Red Medical Material Disposal Cost",
            "Producer Amber + Amber Medical Material Disposal Cost",
            "Producer Green + Green Medical Material Disposal Cost",
            "Producer Disposal Fee w/o Bad Debt Provision",
            "Bad Debt Provision",
            "Producer Disposal Fee with Bad Debt Provision",
            "England with Bad Debt Provision",
            "Wales with Bad Debt Provision",
            "Scotland with Bad Debt Provision",
            "Northern Ireland with Bad Debt Provision"];

        // Data row: 49 values.
        string?[] dataRow = [
            "-",                                                  // PIT
            "1000.000",                                           // HH Tonnage
            "11.000", "12.000", "13.000", "14.000", "15.000", "16.000", // HH RAG
            "0.000",                                              // PB Tonnage
            "21.000", "22.000", "23.000", "24.000", "25.000", "26.000", // PB RAG
            "0.000",                                              // TotalReportedTonnage
            "1.000", "2.000", "3.000", "4.000", "5.000", "6.000", // TotalRag ordered
            "5.000", "7.000", "9.000",                            // grouped: Red+RM, Amber+AM, Green+GM
            "90.000",                                             // SMCW
            "90.000" ,   "0.000",  "90.000",   "0.000",           // ActionedSMCW: total,red,amber,green
            "910.000", "300.000", "200.000", "410.000",           // Net: total,red,amber,green
            "-",                                                  // ResidualSMCW (null)
            "0.000",                                              // TonnageChange
            "£1.0000", "£2.0000", "£3.0000",                      // Price: red,amber,green
            "£4.53"  ,   "£5.00",   "£6.00",                      // DisposalFee: red(4.525001→4.53),amber,green
            "£607.52", "£36.45", "£643.97", "£348.06", "£78.46", "£156.28", "£61.18",  // common 7
        ];

        var expected = new string?[][] { sectionHeader, groupHeader, columnHeaders, dataRow };
        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 49);
    }

    [TestMethod]
    public void Section1MaterialsExporter_Export_CSV_Glass_WithModulation()
    {
        // Arrange — 56 columns with modulation (Glass adds 7 extra: HDC + 6 rag bands)
        var materials = TestDataHelper.GetMaterialDetails().Where(m => m.Code == "GL").ToList();
        const bool applyModulation = true;
        var resultSummary = TestDataHelper.GetCalcResultSummary(applyModulation);
        var producer = resultSummary.ProducerDisposalFees.First();
        producer.ProducerDisposalFeesByMaterial =
            producer.ProducerDisposalFeesByMaterial
                .Where(kv => kv.Key == "GL")
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        var csvContent = new StringBuilder();

        // Act
        SummaryExporterTestUtils.Render(exporter, materials, applyModulation, resultSummary, csvContent);
        var result = csvContent.ToString().ReplaceLineEndings("\n").Split("\n").ToArray();
        Console.WriteLine(string.Join("\n", result));

        string?[] sectionHeader = ["1 Producer Disposal Fees with Bad Debt Provision",
                                   .. Enumerable.Repeat<string?>(null, 55)];
        string?[] groupHeader   = ["Glass Breakdown",
                                   .. Enumerable.Repeat<string?>(null, 55)];

        string?[] columnHeaders = [
            "Previous Invoiced Tonnage",
            "Household Packaging Tonnage",
            "Household Red Material Tonnage",
            "Household Amber Material Tonnage",
            "Household Green Material Tonnage",
            "Household Red Medical Material Tonnage",
            "Household Amber Medical Material Tonnage",
            "Household Green Medical Material Tonnage",
            "Public Bin Tonnage",
            "Public Bin Red Material Tonnage",
            "Public Bin Amber Material Tonnage",
            "Public Bin Green Material Tonnage",
            "Public Bin Red Medical Material Tonnage",
            "Public Bin Amber Medical Material Tonnage",
            "Public Bin Green Medical Material Tonnage",
            "Household Drinks Containers Tonnage - Glass",
            "Household Drinks Containers Red Material Tonnage",
            "Household Drinks Containers Amber Material Tonnage",
            "Household Drinks Containers Green Material Tonnage",
            "Household Drinks Containers Red Medical Material Tonnage",
            "Household Drinks Containers Amber Medical Material Tonnage",
            "Household Drinks Containers Green Medical Material Tonnage",
            "Total Tonnage",
            "Red Total Tonnage",
            "Amber Total Tonnage",
            "Green Total Tonnage",
            "Red Medical Total Tonnage",
            "Amber Medical Total Tonnage",
            "Green Medical Total Tonnage",
            "Red + Red Medical Total Tonnage",
            "Amber + Amber Medical Total Tonnage",
            "Green + Green Medical Total Tonnage",
            "Self Managed Consumer Waste Tonnage",
            "Actioned Self Managed Consumer Waste Tonnage",
            "Red + Red Medical Actioned Self Managed Consumer Waste Tonnage",
            "Amber + Amber Medical Actioned Self Managed Consumer Waste Tonnage",
            "Green + Green Medical Actioned Self Managed Consumer Waste Tonnage",
            "Net Tonnage",
            "Red + Red Medical Net Tonnage",
            "Amber + Amber Medical Net Tonnage",
            "Green + Green Medical Net Tonnage",
            "Residual SMCW",
            "Tonnage Change",
            "Red + Red Medical Material Price per Tonne",
            "Amber + Amber Medical Material Price per Tonne",
            "Green + Green Medical Material Price per Tonne",
            "Producer Red + Red Medical Material Disposal Cost",
            "Producer Amber + Amber Medical Material Disposal Cost",
            "Producer Green + Green Medical Material Disposal Cost",
            "Producer Disposal Fee w/o Bad Debt Provision",
            "Bad Debt Provision",
            "Producer Disposal Fee with Bad Debt Provision",
            "England with Bad Debt Provision",
            "Wales with Bad Debt Provision",
            "Scotland with Bad Debt Provision",
            "Northern Ireland with Bad Debt Provision"];

        // Data row: 56 values.
        string?[] dataRow = [
            "0.000",                                              // PIT=0
            "500.000",                                            // HH Tonnage
            "0.000", "0.000", "0.000", "0.000", "0.000", "0.000", // HH RAG (padded zeros)
            "0.000",                                              // PB Tonnage
            "0.000", "0.000", "0.000", "0.000", "0.000", "0.000", // PB RAG (padded zeros)
            "220.000",                                            // HDC Tonnage
            "0.000", "0.000", "0.000", "0.000", "0.000", "0.000", // HDC RAG (padded zeros)
            "0.000",                                              // TotalReportedTonnage
            "1.000", "2.000", "3.000", "4.000", "5.000", "6.000", // TotalRag ordered
            "5.000", "7.000", "9.000",                            // grouped: Red+RM, Amber+AM, Green+GM
            "150.000",                                            // SMCW
            "150.000",  "50.000", "100.000", "0.000",             // ActionedSMCW: total,red,amber,green
            "350.000", "300.000",  "50.000", "0.000",             // Net: total,red,amber,green
            "-",                                                  // ResidualSMCW (null)
            "0.000",                                              // TonnageChange
            "£1.0000", "£2.0000", "£3.0000",                      // Price: red,amber,green
            "£4.00"  ,   "£5.00",   "£6.00",                      // DisposalFee: red,amber,green
            "£2254.14", "£135.25", "£2389.39", "£1291.43", "£291.10", "£579.85", "£227.00",  // common 7
        ];

        var expected = new string?[][] { sectionHeader, groupHeader, columnHeaders, dataRow };
        CsvTestUtils.AssertSquareCsv(expected, result, expectedLength: 56);
    }
}
