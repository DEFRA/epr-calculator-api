using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class   Section2aMaterialsExporter : IProducerFeesPartExporter
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

    public void AppendSectionHeader(StringBuilder csvContent, ProducerFees producerFees, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData("2a Fees for Comms Costs - by Material with Bad Debt provision"));
        for (int i = 1; i < count; i++)
            csvContent.Append(',');
    }

    public void AppendGroupHeader(StringBuilder csvContent, ProducerFees producerFees, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        foreach (var material in materials)
        {
            int count = Section2aMaterialsHeaders(material).Count();
            csvContent.Append(CsvSanitiser.SanitiseData($"{material.Name} Breakdown"));
            for (int i = 1; i < count; i++)
                csvContent.Append(',');
        }
    }

    public void AppendRow(StringBuilder csvContent, ProducerFeeExportRow producer, bool applyModulation, bool isOverallTotal)
    {
        if (producer.FeeDetail.FeesByMaterial == null) { return; }

        foreach (var disposalFee in producer.FeeDetail.CommsFeesByMaterial!)
        {
            var commCost = disposalFee.Value;
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.HhTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.PbTonnage              , DecimalPlaces.Three, DecimalFormats.F3));
            if (disposalFee.Key == MaterialCodes.Glass)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(commCost.HdcTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            }
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.TotalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(!isOverallTotal ? CsvSanitiser.SanitiseData(commCost.PricePerTonne, DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true) : CommonConstants.CsvFileDelimiter);
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.Costs.FeeWithoutBadDebt             , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.Costs.BadDebt                       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.Costs.ByCountry.Total          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.Costs.ByCountry.England        , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.Costs.ByCountry.Wales          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.Costs.ByCountry.Scotland       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.Costs.ByCountry.NorthernIreland, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        }
    }
}
