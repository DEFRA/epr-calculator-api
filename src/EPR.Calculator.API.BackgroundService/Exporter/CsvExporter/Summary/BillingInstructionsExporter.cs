using System.Text;
using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Enums;
using EPR.Calculator.API.BackgroundService.Misc;
using EPR.Calculator.API.BackgroundService.Utils;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;

public class BillingInstructionsExporter : IProducerFeesPartExporter
{
    // Shared with Summary Exporter
    internal static readonly string Title = "Calculation of Suggested Billing Instructions and Invoice Amounts";

    internal static readonly string CurrentYearInvoiceTotalToDateHeader = "Current Year Invoiced Total To Date";
    internal static readonly string TonnageChangeSinceLastInvoiceHeader = "Tonnage Change Since Last Invoice";
    internal static readonly string PercentageLiabilityDifferenceHeader = "% Liability Difference (Calc vs Prev)";
    internal static readonly string SuggestedBillingInstructionHeader = "Suggested Billing Instruction";
    internal static readonly string SuggestedInvoiceAmountHeader = "Suggested Invoice Amount";

    public IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return [
            CurrentYearInvoiceTotalToDateHeader,
            TonnageChangeSinceLastInvoiceHeader,
            "Liability Difference (Calc vs Prev)",
            "Material £ Threshold Breached",
            "Tonnage £ Threshold Breached (if tonnage changed)",
            PercentageLiabilityDifferenceHeader,
            "Material % Threshold Breached",
            "Tonnage % Threshold Breached (if tonnage changed)",
            SuggestedBillingInstructionHeader,
            SuggestedInvoiceAmountHeader
        ];
    }

    // Shared with Summary Exporter
    internal static string FormatCurrentYearInvoiceTotalToDate(BillingInstruction s) =>
        CsvSanitiser.SanitiseData(s.CurrentYearInvoiceTotalToDate, DecimalPlaces.Two, null, isCurrency: true, canBeEmpty: true);

    internal static string FormatTonnageChangeSinceLastInvoice(BillingInstruction s) =>
        CsvSanitiser.SanitiseData(s.TonnageChangeSinceLastInvoice ?? CommonConstants.Hyphen);

    internal static string FormatPercentageLiabilityDifference(BillingInstruction s) =>
        CsvSanitiser.SanitiseData(s.PercentageLiabilityDifference, DecimalPlaces.Two, null, isPercentage: true, canBeEmpty: true);

    internal static string FormatSuggestedBillingInstruction(BillingInstruction s) =>
        CsvSanitiser.SanitiseData(s.SuggestedBillingInstruction);

    internal static string FormatSuggestedInvoiceAmount(BillingInstruction s) =>
        CsvSanitiser.SanitiseData(s.SuggestedInvoiceAmount, DecimalPlaces.Two, null, isCurrency: true, canBeEmpty: true);

    public void AppendSectionHeader(StringBuilder csvContent, ProducerFees producerFees, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData(Title));
        csvContent.Append(',', count - 1);
    }

    public void AppendRow(StringBuilder csvContent, ProducerFeeExportRow producer, bool applyModulation, bool isOverallTotal)
    {
        var s = producer.FeeDetail.BillingInstruction!;
        csvContent.Append(FormatCurrentYearInvoiceTotalToDate(s));
        csvContent.Append(FormatTonnageChangeSinceLastInvoice(s));
        csvContent.Append(CsvSanitiser.SanitiseData(s.LiabilityDifference, DecimalPlaces.Two, null, isCurrency: true, canBeEmpty: true));
        csvContent.Append(CsvSanitiser.SanitiseData(isOverallTotal ? string.Empty : LiabilityDirectionUtils.ToThresholdBreachedString(s.MaterialityLiabilityDirection), appendLrmCharacterToPreventRenderedAsFormula: true));
        csvContent.Append(CsvSanitiser.SanitiseData(isOverallTotal ? string.Empty : LiabilityDirectionUtils.ToThresholdBreachedString(s.TonnageAmountLiabilityDirection), appendLrmCharacterToPreventRenderedAsFormula: true));
        csvContent.Append(FormatPercentageLiabilityDifference(s));
        csvContent.Append(CsvSanitiser.SanitiseData(isOverallTotal ? string.Empty : LiabilityDirectionUtils.ToThresholdBreachedString(s.MaterialityPercentageLiabilityDirection), appendLrmCharacterToPreventRenderedAsFormula: true));
        csvContent.Append(CsvSanitiser.SanitiseData(isOverallTotal ? string.Empty : LiabilityDirectionUtils.ToThresholdBreachedString(s.TonnageAmountPercentageLiabilityDirection), appendLrmCharacterToPreventRenderedAsFormula: true));
        csvContent.Append(FormatSuggestedBillingInstruction(s));
        csvContent.Append(FormatSuggestedInvoiceAmount(s));
    }
}
