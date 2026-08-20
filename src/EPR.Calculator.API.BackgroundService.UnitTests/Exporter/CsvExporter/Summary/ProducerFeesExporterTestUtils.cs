using System.Text;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;
using EPR.Calculator.API.BackgroundService.Misc;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Exporter.CsvExporter.Summary;

public class ProducerFeesExporterTestUtils
{
    public static void Render(IProducerFeesPartExporter exporter, IReadOnlyList<MaterialDetail> materials, bool applyModulation, ProducerFees producerFees, StringBuilder csvContent)
    {
        exporter.AppendSectionHeader(csvContent, producerFees, materials, applyModulation);
        csvContent.AppendLine();
        exporter.AppendGroupHeader(csvContent, producerFees, materials, applyModulation);
        csvContent.AppendLine();
        foreach (var header in exporter.GetColumnHeaders(materials, applyModulation))
            csvContent.Append(CsvSanitiser.SanitiseData(header));
        csvContent.AppendLine();

        foreach (var producer in producerFees.Details)
        {
            exporter.AppendRow(csvContent, new ProducerFeeExportRow(producer.FeeDetail.Level, producer.FeeDetail), applyModulation, isOverallTotal: false);
            csvContent.AppendLine();
        }

        exporter.AppendRow(csvContent, new ProducerFeeExportRow(string.Empty, producerFees.Total), applyModulation,  isOverallTotal: true);
    }
}
