using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public interface ICalcResultSummaryExporter
{
    void Export(
        RunContext runContext,
        CalcResultSummary resultSummary,
        IImmutableList<MaterialDetail> materials,
        StringBuilder csvContent
    );
}

public class CalcResultSummaryExporter : ICalcResultSummaryExporter
{
    private static readonly IReadOnlyList<ICalcResultSummaryPartExporter> PartExporters =
    [
        new ProducerIdentityExporter(),
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

    public void Export(
        RunContext runContext,
        CalcResultSummary resultSummary,
        IImmutableList<MaterialDetail> materials,
        StringBuilder csvContent
    )
    {
        csvContent.AppendLine();
        csvContent.AppendLine();

        AddSummaryDataHeader(resultSummary, materials, runContext.RequiresModulation, csvContent);

        foreach (var producer in resultSummary.ProducerDisposalFees)
        {
            AddNewRow(csvContent, producer, runContext.RequiresModulation);
        }
    }

    public void AddNewRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        foreach (var exporter in PartExporters)
        {
            exporter.AppendRow(csvContent, producer, applyModulation);
        }
        csvContent.AppendLine();
    }

    private static void AddSummaryDataHeader(CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation, StringBuilder csvContent)
    {
        csvContent.AppendLine(CsvSanitiser.SanitiseData("Calculation Result"))
            .AppendLine()
            .AppendLine();

        csvContent.AppendLine(CsvSanitiser.SanitiseData("NOTE: Rows with 'Scaled-up tonnages?' = " +
            "Yes include reported tonnages for a period that have been scaled-up to a full 6 month equivalent period. " +
            "See 'Scaled-up Producers' table for details."));

        foreach (var exporter in PartExporters)
            exporter.AppendSectionHeader(csvContent, resultSummary, materials, applyModulation);
        csvContent.AppendLine();

        foreach (var exporter in PartExporters)
            exporter.AppendGroupHeader(csvContent, resultSummary, materials, applyModulation);
        csvContent.AppendLine();

        foreach (var exporter in PartExporters)
            foreach (var header in exporter.GetColumnHeaders(materials, applyModulation))
                csvContent.Append(CsvSanitiser.SanitiseData(header));
        csvContent.AppendLine();
    }
}
