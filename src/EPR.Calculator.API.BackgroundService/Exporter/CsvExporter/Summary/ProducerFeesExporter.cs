using System.Text;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Misc;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;

public interface IProducerFeesExporter
{
    Task Export(
        RunContext runContext,
        FeeDetail producerFeesTotal,
        IImmutableList<MaterialDetail> materials,
        IReadOnlyList<int> scaledupProducerIds,
        IReadOnlyList<(int, string?)> partialProducerSubsidiaryIds,
        TextWriter writer,
        IEnumerable<FeeDetail> producerFeeDetails
    );
}

public class ProducerFeesExporter : IProducerFeesExporter
{
    [ActivityTrace]
    public async Task Export(
        RunContext runContext,
        FeeDetail producerFeesTotal,
        IImmutableList<MaterialDetail> materials,
        IReadOnlyList<int> scaledupProducerIds,
        IReadOnlyList<(int, string?)> partialProducerSubsidiaryIds,
        TextWriter writer,
        IEnumerable<FeeDetail> producerFeeDetails
    )
    {
        var partExporters = BuildPartExporters(scaledupProducerIds, partialProducerSubsidiaryIds);
        var csvContent = new StringBuilder();

        csvContent.AppendLine();
        csvContent.AppendLine();

        AddSummaryDataHeader(producerFeesTotal, materials, runContext.RequiresModulation, csvContent, partExporters);
        await Flush(csvContent, writer);

        foreach (var producer in producerFeeDetails)
        {
            AddNewRow(csvContent, new ProducerFeeExportRow(producer.Level, producer), runContext.RequiresModulation, partExporters, isOverallTotal: false);
            await Flush(csvContent, writer);
        }

        var totalRow = new ProducerFeeExportRow(string.Empty, producerFeesTotal);

        // A billing file's total row stops at its "Totals" label (the Leaver Date column), with no values and no line break.
        if (runContext.RunType is RunType.Billing)
            partExporters.OfType<ProducerIdentityExporter>().Single().AppendRow(csvContent, totalRow, runContext.RequiresModulation, isOverallTotal: true);
        else
            AddNewRow(csvContent, totalRow, runContext.RequiresModulation, partExporters, isOverallTotal: true);

        await Flush(csvContent, writer);
    }

    private static async Task Flush(StringBuilder csvContent, TextWriter writer)
    {
        await writer.WriteAsync(csvContent);
        csvContent.Clear();
    }

    private static void AddNewRow(StringBuilder csvContent, ProducerFeeExportRow producer, bool applyModulation, IReadOnlyList<IProducerFeesPartExporter> partExporters, bool isOverallTotal)
    {
        foreach (var exporter in partExporters)
            exporter.AppendRow(csvContent, producer, applyModulation, isOverallTotal);
        csvContent.AppendLine();
    }

    private static IReadOnlyList<IProducerFeesPartExporter> BuildPartExporters(
        IReadOnlyList<int> scaledupProducerIds,
        IReadOnlyList<(int, string?)> partialProducerSubsidiaryIds
    ) =>
    [
        new ProducerIdentityExporter(scaledupProducerIds, partialProducerSubsidiaryIds),
        new SummaryExporter(),
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

    private static void AddSummaryDataHeader(FeeDetail producerFeesTotal, IReadOnlyList<MaterialDetail> materials, bool applyModulation, StringBuilder csvContent, IReadOnlyList<IProducerFeesPartExporter> partExporters)
    {
        csvContent.AppendLine(CsvSanitiser.SanitiseData("Calculation Result"))
            .AppendLine()
            .AppendLine();

        csvContent.AppendLine(CsvSanitiser.SanitiseData("NOTE: Rows with 'Scaled-up tonnages?' = " +
            "Yes include reported tonnages for a period that have been scaled-up to a full 6 month equivalent period. " +
            "See 'Scaled-up Producers' table for details."));

        foreach (var exporter in partExporters)
            exporter.AppendSectionHeader(csvContent, producerFeesTotal, materials, applyModulation);
        csvContent.AppendLine();

        foreach (var exporter in partExporters)
            exporter.AppendGroupHeader(csvContent, producerFeesTotal, materials, applyModulation);
        csvContent.AppendLine();

        foreach (var exporter in partExporters)
            foreach (var header in exporter.GetColumnHeaders(materials, applyModulation))
                csvContent.Append(CsvSanitiser.SanitiseData(header));
        csvContent.AppendLine();
    }
}
