using System.Text;
using EPR.Calculator.API.BackgroundService.Enums;
using EPR.Calculator.API.BackgroundService.Misc;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;

public class OnePlus2a2b2cExporter : IProducerFeesPartExporter
{
    public IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return [
            "Producer Total (1+2a+2b+2c) with Bad Debt provision",
            "Producer Percentage of Overall Producer Cost for (1+2a+2b+2c)"
        ];
    }

    public void AppendSectionHeader(StringBuilder csvContent, ProducerFees producerFees, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData("Total (1+2a+2b+2c) with Bad Debt provision"));
        csvContent.Append(',', count - 1);
    }

    public void AppendGroupHeader(StringBuilder csvContent, ProducerFees producerFees, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData(producerFees.Total.TotalOnePlus2A2B2CWithBadDebt(), DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(',', count - 1);
    }

    public void AppendRow(StringBuilder csvContent, ProducerFeeExportRow producer, bool applyModulation, bool isOverallTotal)
    {
        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.TotalOnePlus2A2B2CWithBadDebt(), DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.TotalOnePlus2A2B2CWithBadDebtPercentage, DecimalPlaces.Eight, DecimalFormats.F8, isCurrency: false, isPercentage: true));
    }
}
