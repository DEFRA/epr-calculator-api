using System.Text;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

// A per-row view for CSV export: pairs the structural Level (only meaningful for a
// Details row - empty for the overall total) with the FeeDetail business data, since
// ProducerFees.Details items and ProducerFees.Total are different CLR types.
public sealed record ProducerFeeExportRow(string? Level, FeeDetail FeeDetail);

public interface IProducerFeesPartExporter
{
    IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation);

    void AppendSectionHeader(StringBuilder csvContent, ProducerFees producerFees, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        foreach (var _ in GetColumnHeaders(materials, applyModulation))
            csvContent.Append(',');
    }

    void AppendGroupHeader(StringBuilder csvContent, ProducerFees producerFees, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        foreach (var _ in GetColumnHeaders(materials, applyModulation))
            csvContent.Append(',');
    }

    void AppendRow(StringBuilder csvContent, ProducerFeeExportRow producer, bool applyModulation, bool isOverallTotal);
}
