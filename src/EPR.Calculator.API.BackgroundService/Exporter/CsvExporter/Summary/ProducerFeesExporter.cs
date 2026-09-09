using System.Text;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Misc;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;

public interface IProducerFeesExporter
{
    Task Export(
        RunContext runContext,
        ProducerFees producerFees,
        IImmutableList<MaterialDetail> materials,
        IReadOnlyList<int> scaledupProducerIds,
        IReadOnlyList<(int, string?)> partialProducerSubsidiaryIds,
        TextWriter writer,
        IEnumerable<FeeDetail>? producerFeeDetails = null
    );
}

public class ProducerFeesExporter : IProducerFeesExporter
{
    [ActivityTrace]
    public async Task Export(
        RunContext runContext,
        ProducerFees producerFees,
        IImmutableList<MaterialDetail> materials,
        IReadOnlyList<int> scaledupProducerIds,
        IReadOnlyList<(int, string?)> partialProducerSubsidiaryIds,
        TextWriter writer,
        IEnumerable<FeeDetail>? producerFeeDetails = null
    )
    {
        var partExporters = BuildPartExporters(scaledupProducerIds, partialProducerSubsidiaryIds);

        // One reused buffer holds the current row (or the header block) - it is flushed and cleared
        // after each, so the ~10^4 producer rows are never all held in memory at once.
        var buffer = new StringBuilder();

        buffer.AppendLine();
        buffer.AppendLine();
        AddSummaryDataHeader(producerFees, materials, runContext.RequiresModulation, buffer, partExporters);
        await Flush(writer, buffer);

        // producerFeeDetails, when supplied, is a streamed DB read consumed once here; otherwise fall
        // back to the already-materialised collection.
        foreach (var producer in producerFeeDetails ?? producerFees.Details.Select(fee => fee.FeeDetail))
        {
            AppendRow(buffer, new ProducerFeeExportRow(producer.Level, producer), runContext.RequiresModulation, partExporters, isOverallTotal: false);
            buffer.AppendLine();
            await Flush(writer, buffer);
        }

        var total = new ProducerFeeExportRow(string.Empty, producerFees.Total);
        if (runContext.RunType is RunType.Billing)
        {
            // The billing file keeps only the identity columns of the overall-total row (its
            // Leaver's Date cell carries "Totals"), with no trailing newline - the rejected-producers
            // section that follows starts with its own blank lines.
            partExporters[0].AppendRow(buffer, total, runContext.RequiresModulation, isOverallTotal: true);
        }
        else
        {
            AppendRow(buffer, total, runContext.RequiresModulation, partExporters, isOverallTotal: true);
            buffer.AppendLine();
        }

        await Flush(writer, buffer);
    }

    private static void AppendRow(StringBuilder buffer, ProducerFeeExportRow producer, bool applyModulation, IReadOnlyList<IProducerFeesPartExporter> partExporters, bool isOverallTotal)
    {
        foreach (var exporter in partExporters)
            exporter.AppendRow(buffer, producer, applyModulation, isOverallTotal);
    }

    private static async Task Flush(TextWriter writer, StringBuilder buffer)
    {
        foreach (var chunk in buffer.GetChunks())
            await writer.WriteAsync(chunk);
        buffer.Clear();
    }

    private static IReadOnlyList<IProducerFeesPartExporter> BuildPartExporters(
        IReadOnlyList<int> scaledupProducerIds,
        IReadOnlyList<(int, string?)> partialProducerSubsidiaryIds
    ) =>
    [
        new ProducerIdentityExporter(scaledupProducerIds, partialProducerSubsidiaryIds),
        new Section1MaterialsExporter(),
        new Section1DisposalFeeExporter(),
        new Section2aMaterialsExporter(),
        new Section2aCommsExporter(),
        new Section1DisposalExporter(),
        new Section2aComms2aExporter(),
        new CommsCost2aPercentageExporter(),
        new CommsCost2bExporter(),
        new CommsCost2cExporter(),
        new OnePlus2a2b2cExporter(),
        new ThreeSaCostsExporter(),
        new LaDataPrepCostsExporter(),
        new SaSetupCostsExporter(),
        new TotalBillBreakdownExporter(),
        new BillingInstructionsExporter(),
    ];

    private static void AddSummaryDataHeader(ProducerFees producerFees, IReadOnlyList<MaterialDetail> materials, bool applyModulation, StringBuilder csvContent, IReadOnlyList<IProducerFeesPartExporter> partExporters)
    {
        csvContent.AppendLine(CsvSanitiser.SanitiseData("Calculation Result"))
            .AppendLine()
            .AppendLine();

        csvContent.AppendLine(CsvSanitiser.SanitiseData("NOTE: Rows with 'Scaled-up tonnages?' = " +
            "Yes include reported tonnages for a period that have been scaled-up to a full 6 month equivalent period. " +
            "See 'Scaled-up Producers' table for details."));

        foreach (var exporter in partExporters)
            exporter.AppendSectionHeader(csvContent, producerFees, materials, applyModulation);
        csvContent.AppendLine();

        foreach (var exporter in partExporters)
            exporter.AppendGroupHeader(csvContent, producerFees, materials, applyModulation);
        csvContent.AppendLine();

        foreach (var exporter in partExporters)
            foreach (var header in exporter.GetColumnHeaders(materials, applyModulation))
                csvContent.Append(CsvSanitiser.SanitiseData(header));
        csvContent.AppendLine();
    }
}
