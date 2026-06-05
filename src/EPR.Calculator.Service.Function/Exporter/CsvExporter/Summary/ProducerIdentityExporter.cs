using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class ProducerIdentityExporter : ICalcResultSummaryPartExporter
{
    public IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return
        [
            "Producer ID",
            "Subsidiary ID",
            "Producer / Subsidiary Name",
            "Trading Name",
            "Level",
            "Scaled-up tonnages?",
            "Partial Calculation?",
            "Registration Status Code",
            "Joiners Date",
            "Leavers Date"
        ];
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerName));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TradingName));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.IsProducerScaledup));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.IsPartialObligation));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.StatusCode));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.JoinerDate));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.LeaverDate));
    }
}
