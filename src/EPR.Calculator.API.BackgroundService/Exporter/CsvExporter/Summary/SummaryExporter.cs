using System.Text;
using EPR.Calculator.API.BackgroundService.Misc;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;

public class SummaryExporter : IProducerFeesPartExporter
{
    private static readonly string Title = "Summary";

    public IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return [
            BillingInstructionsExporter.CurrentYearInvoiceTotalToDateHeader,
            BillingInstructionsExporter.TonnageChangeSinceLastInvoiceHeader,
            TotalBillBreakdownExporter.TotalWithBadDebtProvisionHeader,
            BillingInstructionsExporter.PercentageLiabilityDifferenceHeader,
            BillingInstructionsExporter.SuggestedBillingInstructionHeader,
            BillingInstructionsExporter.SuggestedInvoiceAmountHeader
        ];
    }

    public void AppendSectionHeader(StringBuilder csvContent, ProducerFees producerFees, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData(Title));
        csvContent.Append(',', count - 1);
    }

    public void AppendRow(StringBuilder csvContent, ProducerFeeExportRow producer, bool applyModulation, bool isOverallTotal)
    {
        var billing = producer.FeeDetail.BillingInstruction!;
        csvContent.Append(BillingInstructionsExporter.FormatCurrentYearInvoiceTotalToDate(billing));
        csvContent.Append(BillingInstructionsExporter.FormatTonnageChangeSinceLastInvoice(billing));
        csvContent.Append(TotalBillBreakdownExporter.FormatTotalWithBadDebtProvision(producer.FeeDetail.TotalBillBreakdown));
        csvContent.Append(BillingInstructionsExporter.FormatPercentageLiabilityDifference(billing));
        csvContent.Append(BillingInstructionsExporter.FormatSuggestedBillingInstruction(billing));
        csvContent.Append(BillingInstructionsExporter.FormatSuggestedInvoiceAmount(billing));
    }
}
