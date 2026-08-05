using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class Section1DisposalFeeExporter : IProducerFeesPartExporter
{
    public IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return [
            "1 Total Producer Disposal Fee w/o Bad Debt Provision",
            "Bad Debt Provision",
            "1 Total Producer Disposal Fee with Bad Debt Provision",
            "England Total",
            "Wales Total",
            "Scotland Total",
            "Northern Ireland Total",
            "Tonnage Change Count",
            "Tonnage Change Advice"
        ];
    }

    public void AppendGroupHeader(StringBuilder csvContent, ProducerFees producerFees, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData("Disposal Fee Summary"));
        csvContent.Append(',', count - 1);
    }

    public void AppendRow(StringBuilder csvContent, ProducerFeeExportRow producer, bool applyModulation, bool isOverallTotal)
    {
        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.LADisposalCostsSection1.FeeWithoutBadDebt             , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.LADisposalCostsSection1.BadDebt                       , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.LADisposalCostsSection1.ByCountry.Total          , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.LADisposalCostsSection1.ByCountry.England        , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.LADisposalCostsSection1.ByCountry.Wales          , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.LADisposalCostsSection1.ByCountry.Scotland       , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.LADisposalCostsSection1.ByCountry.NorthernIreland, DecimalPlaces.Two, null, isCurrency: true));
        AppendCsvValue(csvContent, producer.FeeDetail.TonnageChangeCount, isOverallTotal);
        AppendCsvValue(csvContent, producer.FeeDetail.TonnageChangeAdvice, isOverallTotal);
    }

    private static void AppendCsvValue(
        StringBuilder csvContent,
        string? value,
        bool isOverallTotalRow
    )
    {
        if (value == null && !isOverallTotalRow)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Hyphen));
        }
        else
        {
            csvContent.Append(CsvSanitiser.SanitiseData(value));
        }
    }
}
