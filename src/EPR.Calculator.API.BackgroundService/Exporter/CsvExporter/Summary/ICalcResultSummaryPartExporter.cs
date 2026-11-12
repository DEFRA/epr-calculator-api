using System.Text;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;

// A per-row view for CSV export: pairs the structural Level (only meaningful for a
// per-producer row - empty for the overall total row) with the FeeDetail business data.
public sealed record ProducerFeeExportRow(string? Level, FeeDetail FeeDetail);

public interface IProducerFeesPartExporter
{
    IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation);

    void AppendSectionHeader(StringBuilder csvContent, FeeDetail producerFeesTotal, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        foreach (var _ in GetColumnHeaders(materials, applyModulation))
            csvContent.Append(',');
    }

    void AppendGroupHeader(StringBuilder csvContent, FeeDetail producerFeesTotal, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        foreach (var _ in GetColumnHeaders(materials, applyModulation))
            csvContent.Append(',');
    }

    void AppendRow(StringBuilder csvContent, ProducerFeeExportRow producer, bool applyModulation, bool isOverallTotal);
}
