using System.Text;
using EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class BillingInstructionsExporter : ICalcResultSummaryPartExporter
{
    public static readonly string Title = "Calculation of Suggested Billing Instructions and Invoice Amounts";

    public IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return [
            "Current Year Invoiced Total To Date",
            "Tonnage Change Since Last Invoice",
            "Liability Difference (Calc vs Prev)",
            "Material £ Threshold Breached",
            "Tonnage £ Threshold Breached (if tonnage changed)",
            "% Liability Difference (Calc vs Prev)",
            "Material % Threshold Breached",
            "Tonnage % Threshold Breached (if tonnage changed)",
            "Suggested Billing Instruction",
            "Suggested Invoice Amount"
        ];
    }

    public void AppendSectionHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData(Title));
        csvContent.Append(',', count - 1);
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        var s = producer.BillingInstructionSection!;
        csvContent.Append(CsvSanitiser.SanitiseData(CurrencyConverterUtils.FormattedCurrencyValue(s.CurrentYearInvoiceTotalToDate), DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(s.TonnageChangeSinceLastInvoice ?? CommonConstants.Hyphen));
        csvContent.Append(CsvSanitiser.SanitiseData(CurrencyConverterUtils.FormattedCurrencyValue(s.LiabilityDifference), DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(s.MaterialThresholdBreached, appendLrmCharacterToPreventRenderedAsFormula: true));
        csvContent.Append(CsvSanitiser.SanitiseData(s.TonnageThresholdBreached , appendLrmCharacterToPreventRenderedAsFormula: true));
        csvContent.Append(CsvSanitiser.SanitiseData(s.PercentageLiabilityDifference != null ? s.PercentageLiabilityDifference.ToString() : CommonConstants.Hyphen, DecimalPlaces.Two, null, false, true));
        csvContent.Append(CsvSanitiser.SanitiseData(s.MaterialPercentageThresholdBreached, appendLrmCharacterToPreventRenderedAsFormula: true));
        csvContent.Append(CsvSanitiser.SanitiseData(s.TonnagePercentageThresholdBreached , appendLrmCharacterToPreventRenderedAsFormula: true));
        csvContent.Append(CsvSanitiser.SanitiseData(s.SuggestedBillingInstruction));
        csvContent.Append(CsvSanitiser.SanitiseData(CurrencyConverterUtils.FormattedCurrencyValue(s.SuggestedInvoiceAmount), DecimalPlaces.Two, null, isCurrency: true));
    }
}
