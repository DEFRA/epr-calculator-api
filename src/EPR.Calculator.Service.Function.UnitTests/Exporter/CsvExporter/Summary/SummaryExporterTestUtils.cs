using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.Builder;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

public class SummaryExporterTestUtils
{
    public static void Render(ICalcResultSummaryPartExporter exporter, IReadOnlyList<MaterialDetail> materials, bool applyModulation, CalcResultSummary resultSummary, StringBuilder csvContent)
    {
        exporter.AppendSectionHeader(csvContent, resultSummary, materials, applyModulation);
        csvContent.AppendLine();
        exporter.AppendGroupHeader(csvContent, resultSummary, materials, applyModulation);
        csvContent.AppendLine();
        foreach (var header in exporter.GetColumnHeaders(materials, applyModulation))
            csvContent.Append(CsvSanitiser.SanitiseData(header));
        csvContent.AppendLine();

        foreach (var producer in resultSummary.ProducerDisposalFees)
            exporter.AppendRow(csvContent, producer, applyModulation);
    }
}
