using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using static EPR.Calculator.Service.Function.Constants.MaterialCodes;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class Section2aMaterialsExporter : ICalcResultSummaryPartExporter
{
    public IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return materials.SelectMany(material =>
        {
            return Section2aMaterialsHeaders(material);
        });
    }

    private static IEnumerable<string> Section2aMaterialsHeaders(MaterialDetail material)
    {
        var headers = new List<string>();
        headers.AddRange([
            "Household Packaging Tonnage",
            "Public Bin Tonnage"
        ]);

        if (material.Code == MaterialCodes.Glass)
        {
            headers.Add("Household Drinks Containers Tonnage - Glass");
        }

        headers.AddRange([
            "Total Tonnage",
            "Price per Tonne",
            "Producer Total Cost w/o Bad Debt Provision",
            "Bad Debt Provision",
            "Producer Total Cost with Bad Debt Provision",
            "England with Bad Debt Provision",
            "Wales with Bad Debt Provision",
            "Scotland with Bad Debt Provision",
            "Northern Ireland with Bad Debt Provision"
        ]);

        return headers;
    }

    public void AppendSectionHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData("2a Fees for Comms Costs - by Material with Bad Debt provision"));
        for (int i = 1; i < count; i++)
            csvContent.Append(',');
    }

    public void AppendGroupHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        foreach (var material in materials)
        {
            int count = Section2aMaterialsHeaders(material).Count();
            csvContent.Append(CsvSanitiser.SanitiseData($"{material.Name} Breakdown"));
            for (int i = 1; i < count; i++)
                csvContent.Append(',');
        }
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        if (producer.ProducerCommsFeesByMaterial == null) { return; }

        bool isNotTotal = producer.LeaverDate != CommonConstants.Totals;

        foreach (var disposalFee in producer.ProducerCommsFeesByMaterial!)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.PublicBinTonnage              , DecimalPlaces.Three, DecimalFormats.F3));
            if (disposalFee.Key == MaterialCodes.Glass)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdDrinksContainersTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            }
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.TotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(isNotTotal ? CsvSanitiser.SanitiseData(disposalFee.Value.PriceperTonne, DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true) : CommonConstants.CsvFileDelimiter);
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ProducerTotalCostWithoutBadDebtProvision, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.BadDebtProvision                        , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ProducerTotalCostwithBadDebtProvision   , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.EnglandWithBadDebtProvision             , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.WalesWithBadDebtProvision               , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ScotlandWithBadDebtProvision            , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.NorthernIrelandWithBadDebtProvision     , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        }
    }
}
