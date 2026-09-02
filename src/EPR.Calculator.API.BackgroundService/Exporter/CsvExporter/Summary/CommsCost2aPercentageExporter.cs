using System.Text;
using EPR.Calculator.API.BackgroundService.Enums;
using EPR.Calculator.API.BackgroundService.Misc;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;

public class CommsCost2aPercentageExporter : IProducerFeesPartExporter
{
    public IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return [
            "Percentage of Producer Tonnage vs All Producers"
        ];
    }

    public void AppendRow(StringBuilder csvContent, ProducerFeeExportRow producer, bool applyModulation, bool isOverallTotal)
    {
        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.ReportedTonnagePercentage, DecimalPlaces.Eight, null, false, true));
    }
}
