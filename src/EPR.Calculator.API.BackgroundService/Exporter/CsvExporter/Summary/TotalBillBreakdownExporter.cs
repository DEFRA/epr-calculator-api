using System.Text;
using EPR.Calculator.API.BackgroundService.Enums;
using EPR.Calculator.API.BackgroundService.Misc;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;

public class TotalBillBreakdownExporter : IProducerFeesPartExporter
{
    // Shared with Summary Exporter
    internal static readonly string TotalWithBadDebtProvisionHeader = "Total Producer Bill (1+2a+2b+2c+3+4+5) with Bad Debt Provision";

    public IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return [
            "Total Producer Bill (1+2a+2b+2c+3+4+5) w/o Bad Debt Provision",
            "Bad Debt Provision for Total Producer Bill",
            TotalWithBadDebtProvisionHeader,
            "England Total with Bad Debt provision",
            "Wales Total with Bad Debt provision",
            "Scotland Total with Bad Debt provision",
            "Northern Ireland Total with Bad Debt provision"
        ];
    }

    // Shared with Summary Exporter
    internal static string FormatTotalWithBadDebtProvision(FeeWithBadDebt costs) =>
        CsvSanitiser.SanitiseData(costs.ByCountry.Total, DecimalPlaces.Two, null, isCurrency: true);

    public void AppendSectionHeader(StringBuilder csvContent, ProducerFees producerFees, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData("Total Producer Bill Breakdown"));
        csvContent.Append(',', count - 1);
    }

    public void AppendRow(StringBuilder csvContent, ProducerFeeExportRow producer, bool applyModulation, bool isOverallTotal)
    {
        var costs = producer.FeeDetail.TotalBillBreakdown;
        csvContent.Append(CsvSanitiser.SanitiseData(costs.FeeWithoutBadDebt        , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs.BadDebt                  , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(FormatTotalWithBadDebtProvision(costs));
        csvContent.Append(CsvSanitiser.SanitiseData(costs.ByCountry.England        , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs.ByCountry.Wales          , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs.ByCountry.Scotland       , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs.ByCountry.NorthernIreland, DecimalPlaces.Two, null, isCurrency: true));
    }
}
