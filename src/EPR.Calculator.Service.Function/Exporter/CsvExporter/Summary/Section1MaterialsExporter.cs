using System.Diagnostics.CodeAnalysis;
using System.Text;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class Section1MaterialsExporter : IProducerFeesPartExporter
{
    public IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return materials.SelectMany(material =>
        {
            return Section1MaterialsHeaders(material, applyModulation);
        });
    }

    private static IEnumerable<string> Section1MaterialsHeaders(MaterialDetail material, bool applyModulation)
    {
        var headers = new List<string>();
        headers.Add("Previous Invoiced Tonnage");

        headers.Add("Household Packaging Tonnage");
        if (applyModulation)
        {
            headers.AddRange([
                "Household Red Material Tonnage",
                "Household Amber Material Tonnage",
                "Household Green Material Tonnage",
                "Household Red Medical Material Tonnage",
                "Household Amber Medical Material Tonnage",
                "Household Green Medical Material Tonnage"
            ]);
        }

        headers.Add("Public Bin Tonnage");
        if (applyModulation)
        {
            headers.AddRange([
                "Public Bin Red Material Tonnage",
                "Public Bin Amber Material Tonnage",
                "Public Bin Green Material Tonnage",
                "Public Bin Red Medical Material Tonnage",
                "Public Bin Amber Medical Material Tonnage",
                "Public Bin Green Medical Material Tonnage"
            ]);
        }

        if (material.Code == MaterialCodes.Glass)
        {
            headers.Add("Household Drinks Containers Tonnage - Glass");
            if (applyModulation)
            {
                headers.AddRange([
                    "Household Drinks Containers Red Material Tonnage",
                    "Household Drinks Containers Amber Material Tonnage",
                    "Household Drinks Containers Green Material Tonnage",
                    "Household Drinks Containers Red Medical Material Tonnage",
                    "Household Drinks Containers Amber Medical Material Tonnage",
                    "Household Drinks Containers Green Medical Material Tonnage"
                ]);
            }
        }

        if (applyModulation) {
            headers.AddRange([
                "Total Tonnage",
                "Red Total Tonnage",
                "Amber Total Tonnage",
                "Green Total Tonnage",
                "Red Medical Total Tonnage",
                "Amber Medical Total Tonnage",
                "Green Medical Total Tonnage",
                "Red + Red Medical Total Tonnage",
                "Amber + Amber Medical Total Tonnage",
                "Green + Green Medical Total Tonnage",
                "Self Managed Consumer Waste Tonnage",
                "Actioned Self Managed Consumer Waste Tonnage",
                "Red + Red Medical Actioned Self Managed Consumer Waste Tonnage",
                "Amber + Amber Medical Actioned Self Managed Consumer Waste Tonnage",
                "Green + Green Medical Actioned Self Managed Consumer Waste Tonnage",
                "Net Tonnage",
                "Red + Red Medical Net Tonnage",
                "Amber + Amber Medical Net Tonnage",
                "Green + Green Medical Net Tonnage",
                "Residual SMCW"
            ]);
        } else {
            headers.AddRange([
                "Total Tonnage",
                "Self Managed Consumer Waste Tonnage",
                "Net Tonnage"
            ]);
        }

        headers.Add("Tonnage Change");
        if (applyModulation) {
            headers.AddRange([
                "Red + Red Medical Material Price per Tonne",
                "Amber + Amber Medical Material Price per Tonne",
                "Green + Green Medical Material Price per Tonne",
                "Producer Red + Red Medical Material Disposal Cost",
                "Producer Amber + Amber Medical Material Disposal Cost",
                "Producer Green + Green Medical Material Disposal Cost"
            ]);
        } else {
            headers.Add("Price per Tonne");
        }

        headers.AddRange([
            "Producer Disposal Fee w/o Bad Debt Provision",
            "Bad Debt Provision",
            "Producer Disposal Fee with Bad Debt Provision",
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
        csvContent.Append(CsvSanitiser.SanitiseData("1 Producer Disposal Fees with Bad Debt Provision"));
        csvContent.Append(',', count - 1);
    }

    public void AppendGroupHeader(StringBuilder csvContent, ProducerFees producerFees, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        foreach (var material in materials)
        {
            int count = Section1MaterialsHeaders(material, applyModulation).Count();
            csvContent.Append(CsvSanitiser.SanitiseData($"{material.Name} Breakdown"));
            csvContent.Append(',', count - 1);
        }
    }

    public void AppendRow(StringBuilder csvContent, ProducerFeeExportRow producer, bool applyModulation, bool isOverallTotal)
    {
        foreach (var (key, disposalFee) in producer.FeeDetail.DisposalFeesByMaterial)
        {
            AppendProducerDisposalFeesByMaterial(csvContent, producer, key, disposalFee, applyModulation, isOverallTotal);
        }
    }

    [SuppressMessage(
        "Critical Code Smell",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "Temporaraly suppress - will refactor later.")]
    private void AppendProducerDisposalFeesByMaterial(
        StringBuilder csvContent,
        ProducerFeeExportRow producer,
        string key,
        DisposalFee disposalFee,
        bool applyModulation,
        bool isOverallTotal)
    {
        csvContent.Append(
            !isOverallTotal && (producer.Level != "1" || disposalFee.PreviousInvoicedTonnage == null)
                ? CsvSanitiser.SanitiseData(CommonConstants.Hyphen)
                : CsvSanitiser.SanitiseData(disposalFee.PreviousInvoicedTonnage, DecimalPlaces.Three, DecimalFormats.F3));

        foreach (var tonnage in MaterialTonnagePackages(key, disposalFee, applyModulation)) {
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage, DecimalPlaces.Three, DecimalFormats.F3));
        }

        if (applyModulation) {
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.TotalTonnage.TotalRamTonnage(), DecimalPlaces.Three, DecimalFormats.F3));

            var totalRed = disposalFee.TotalTonnage.Red;
            var totalAmber = disposalFee.TotalTonnage.Amber;
            var totalGreen = disposalFee.TotalTonnage.Green;
            var totalRedMedical = disposalFee.TotalTonnage.RedMedical;
            var totalAmberMedical = disposalFee.TotalTonnage.AmberMedical;
            var totalGreenMedical = disposalFee.TotalTonnage.GreenMedical;
            csvContent.Append(CsvSanitiser.SanitiseData(totalRed, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(totalAmber, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(totalGreen, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(totalRedMedical, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(totalAmberMedical, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(totalGreenMedical, DecimalPlaces.Three, DecimalFormats.F3));

            csvContent.Append(CsvSanitiser.SanitiseData(totalRed + totalRedMedical, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(totalAmber + totalAmberMedical, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(totalGreen + totalGreenMedical, DecimalPlaces.Three, DecimalFormats.F3));

            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.SmcwTonnage              , DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ActionedSmcwTonnage.Total, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ActionedSmcwTonnage.Red  , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ActionedSmcwTonnage.Amber, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ActionedSmcwTonnage.Green, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetTonnage.Total                     , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetTonnage.Red                       , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetTonnage.Amber                     , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetTonnage.Green                     , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ResidualSmcwTonnage      , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
        } else {
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.TotalTonnage.TotalRamTonnage(), DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.SmcwTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetTonnage.Total, DecimalPlaces.Three, DecimalFormats.F3));
        }

        AppendCsvValue(csvContent, disposalFee.TonnageChange, isOverallTotal, DecimalPlaces.Three, DecimalFormats.F3);

        if (applyModulation) {
            csvContent.Append(!isOverallTotal ? CsvSanitiser.SanitiseData(disposalFee.PricePerTonne.Red  , DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true, canBeEmpty: true) : CommonConstants.CsvFileDelimiter);
            csvContent.Append(!isOverallTotal ? CsvSanitiser.SanitiseData(disposalFee.PricePerTonne.Amber, DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true, canBeEmpty: true) : CommonConstants.CsvFileDelimiter);
            csvContent.Append(!isOverallTotal ? CsvSanitiser.SanitiseData(disposalFee.PricePerTonne.Green, DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true, canBeEmpty: true) : CommonConstants.CsvFileDelimiter);
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Fee.Red  , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Fee.Amber, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Fee.Green, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true, canBeEmpty: true));
        } else {
            csvContent.Append(!isOverallTotal ? CsvSanitiser.SanitiseData(disposalFee.PricePerTonne.Total ?? 0, DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true) : CommonConstants.CsvFileDelimiter);
        }

        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Fee.Total ?? 0         , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.BadDebt                       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.FeeWithBadDebtByCountry.Total              , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.FeeWithBadDebtByCountry.England            , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.FeeWithBadDebtByCountry.Wales              , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.FeeWithBadDebtByCountry.Scotland           , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.FeeWithBadDebtByCountry.NorthernIreland    , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
    }

    private static IEnumerable<decimal> MaterialTonnagePackages(string materialCode, DisposalFee mb, bool applyModulation)
    {
        yield return mb.HhTonnage.TotalRamTonnage();

        if (applyModulation)
        {
            yield return mb.HhTonnage.Red;
            yield return mb.HhTonnage.Amber;
            yield return mb.HhTonnage.Green;
            yield return mb.HhTonnage.RedMedical;
            yield return mb.HhTonnage.AmberMedical;
            yield return mb.HhTonnage.GreenMedical;
        }

        yield return mb.PbTonnage.TotalRamTonnage();

        if (applyModulation)
        {
            yield return mb.PbTonnage.Red;
            yield return mb.PbTonnage.Amber;
            yield return mb.PbTonnage.Green;
            yield return mb.PbTonnage.RedMedical;
            yield return mb.PbTonnage.AmberMedical;
            yield return mb.PbTonnage.GreenMedical;
        }

        if (materialCode == MaterialCodes.Glass)
        {
            yield return mb.HdcTonnage.TotalRamTonnage();

            if (applyModulation)
            {
                yield return mb.HdcTonnage.Red;
                yield return mb.HdcTonnage.Amber;
                yield return mb.HdcTonnage.Green;
                yield return mb.HdcTonnage.RedMedical;
                yield return mb.HdcTonnage.AmberMedical;
                yield return mb.HdcTonnage.GreenMedical;
            }
        }
    }

    private static void AppendCsvValue(
        StringBuilder csvContent,
        decimal? value,
        bool isOverallTotalRow = false,
        DecimalPlaces decimalPlaces = DecimalPlaces.Zero,
        DecimalFormats decimalFormat = DecimalFormats.F2)
    {
        if (value == null && !isOverallTotalRow)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Hyphen));
        } else {
            csvContent.Append(CsvSanitiser.SanitiseData(value, decimalPlaces, decimalFormat));
        }
    }
}
