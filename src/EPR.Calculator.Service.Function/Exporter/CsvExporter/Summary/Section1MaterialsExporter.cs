using System.Diagnostics.CodeAnalysis;
using System.Text;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class Section1MaterialsExporter : ICalcResultSummaryPartExporter
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

    public void AppendSectionHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData("1 Producer Disposal Fees with Bad Debt Provision"));
        csvContent.Append(',', count - 1);
    }

    public void AppendGroupHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        foreach (var material in materials)
        {
            int count = Section1MaterialsHeaders(material, applyModulation).Count();
            csvContent.Append(CsvSanitiser.SanitiseData($"{material.Name} Breakdown"));
            csvContent.Append(',', count - 1);
        }
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        bool isNotTotal = producer.LeaverDate != CommonConstants.Totals;

        foreach (var (key, disposalFee) in producer.ProducerDisposalFeesByMaterial)
        {
            AppendProducerDisposalFeesByMaterial(csvContent, producer, key, disposalFee, applyModulation, isNotTotal);
        }
    }

    private static RagRating GroupedRagRating(RagRating rating) => rating switch
    {
        RagRating.Red   or RagRating.RedMedical   => RagRating.Red,
        RagRating.Amber or RagRating.AmberMedical => RagRating.Amber,
        RagRating.Green or RagRating.GreenMedical => RagRating.Green,
        _ => throw new ArgumentOutOfRangeException(nameof(rating))
    };

    [SuppressMessage(
        "Critical Code Smell",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "Temporaraly suppress - will refactor later.")]
    private void AppendProducerDisposalFeesByMaterial(
        StringBuilder csvContent,
        CalcResultSummaryProducerDisposalFees producer,
        string key,
        CalcResultSummaryProducerDisposalFeesByMaterial disposalFee,
        bool applyModulation,
        bool isNotTotal)
    {
        csvContent.Append(
            !producer.isOverallTotalRow && (producer.Level != "1" || disposalFee.PreviousInvoicedTonnage == null)
                ? CsvSanitiser.SanitiseData(CommonConstants.Hyphen)
                : CsvSanitiser.SanitiseData(disposalFee.PreviousInvoicedTonnage, DecimalPlaces.Three, DecimalFormats.F3));

        foreach (var tonnage in MaterialTonnagePackages(key, disposalFee, applyModulation)) {
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage, DecimalPlaces.Three, DecimalFormats.F3));
        }

        if (applyModulation) {
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.TotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));

            foreach (var (_, v) in disposalFee.TotalReportedTonnageRagRating.OrderBy(x => x.Key))
            {
                csvContent.Append(CsvSanitiser.SanitiseData(v, DecimalPlaces.Three, DecimalFormats.F3));
            }

            foreach (var group in disposalFee.TotalReportedTonnageRagRating.GroupBy(x => GroupedRagRating(x.Key)).OrderBy(x => x.Key))
            {
                csvContent.Append(CsvSanitiser.SanitiseData(group.Sum(x => x.Value), DecimalPlaces.Three, DecimalFormats.F3));
            }

            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.SelfManagedConsumerWasteTonnage              , DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ActionedSelfManagedConsumerWasteTonnage.total, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ActionedSelfManagedConsumerWasteTonnage.red  , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ActionedSelfManagedConsumerWasteTonnage.amber, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ActionedSelfManagedConsumerWasteTonnage.green, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetReportedTonnage.total                     , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetReportedTonnage.red                       , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetReportedTonnage.amber                     , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetReportedTonnage.green                     , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ResidualSelfManagedConsumerWasteTonnage      , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
        } else {
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.TotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.SelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetReportedTonnage.total, DecimalPlaces.Three, DecimalFormats.F3));
        }

        AppendCsvValue(csvContent, disposalFee.TonnageChange, producer.isOverallTotalRow, DecimalPlaces.Three, DecimalFormats.F3);

        if (applyModulation) {
            csvContent.Append(isNotTotal ? CsvSanitiser.SanitiseData(disposalFee.PricePerTonne.red  , DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true, canBeEmpty: true) : CommonConstants.CsvFileDelimiter);
            csvContent.Append(isNotTotal ? CsvSanitiser.SanitiseData(disposalFee.PricePerTonne.amber, DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true, canBeEmpty: true) : CommonConstants.CsvFileDelimiter);
            csvContent.Append(isNotTotal ? CsvSanitiser.SanitiseData(disposalFee.PricePerTonne.green, DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true, canBeEmpty: true) : CommonConstants.CsvFileDelimiter);
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFee.red  , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFee.amber, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFee.green, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true, canBeEmpty: true));
        } else {
            csvContent.Append(isNotTotal ? CsvSanitiser.SanitiseData(disposalFee.PricePerTonne.total ?? 0, DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true) : CommonConstants.CsvFileDelimiter);
        }

        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFee.total ?? 0         , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.BadDebtProvision                       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFeeWithBadDebtProvision, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.EnglandWithBadDebtProvision            , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.WalesWithBadDebtProvision              , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ScotlandWithBadDebtProvision           , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NorthernIrelandWithBadDebtProvision    , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
    }

    private static IEnumerable<decimal> MaterialTonnagePackages(string materialCode, CalcResultSummaryProducerDisposalFeesByMaterial mb, bool applyModulation)
    {
        yield return mb.HouseholdPackagingWasteTonnage;

        if (applyModulation)
        {
            foreach (var rating in Enum.GetValues<RagRating>())
                yield return mb.HouseholdPackagingWasteTonnageRagRating.GetValueOrDefault(rating, 0);
        }

        yield return mb.PublicBinTonnage;

        if (applyModulation)
        {
            foreach (var rating in Enum.GetValues<RagRating>())
                yield return mb.PublicBinTonnageRagRating.GetValueOrDefault(rating, 0);
        }

        if (materialCode == MaterialCodes.Glass)
        {
            yield return mb.HouseholdDrinksContainersTonnage;

            if (applyModulation)
            {
                foreach (var rating in Enum.GetValues<RagRating>())
                    yield return mb.HouseholdDrinksContainersTonnageRagRating.GetValueOrDefault(rating, 0);
            }
        }
    }

    private static void AppendCsvValue(
        StringBuilder csvContent,
        object? value,
        bool isOverallTotalRow = false,
        DecimalPlaces decimalPlaces = DecimalPlaces.Zero,
        DecimalFormats decimalFormat = DecimalFormats.F2)
    {
        if (value == null && !isOverallTotalRow)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Hyphen));
        }
        else if (value is int or decimal or double)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(value, decimalPlaces, decimalFormat));
        }
        else
        {
            csvContent.Append(CsvSanitiser.SanitiseData(value));
        }
    }
}
