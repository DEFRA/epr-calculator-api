using System.Text;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class ProducerIdentityExporter(
    IReadOnlyList<int> scaledupProducerIds,
    IReadOnlyList<(int, string?)> partialProducerSubsidiaryIds
) : IProducerFeesPartExporter
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

    public void AppendRow(StringBuilder csvContent, ProducerFeeExportRow producer, bool applyModulation, bool isOverallTotal)
    {
        string YesOrNo(bool isValueSet) {
            if(isOverallTotal)
                return string.Empty;

            return isValueSet ? CommonConstants.Yes : CommonConstants.No;
        }

        var isScaledup = scaledupProducerIds.Contains(producer.FeeDetail.ProducerId);
        var isPartialObligation = producer.Level == "1"
            ? partialProducerSubsidiaryIds.Any(p => p.Item1 == producer.FeeDetail.ProducerId)
            : partialProducerSubsidiaryIds.Contains((producer.FeeDetail.ProducerId, producer.FeeDetail.SubsidiaryId));

        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.ProducerId == 0 ? string.Empty : producer.FeeDetail.ProducerId.ToString()));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.SubsidiaryId));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.ProducerName));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.TradingName));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
        csvContent.Append(CsvSanitiser.SanitiseData(YesOrNo(isScaledup)));
        csvContent.Append(CsvSanitiser.SanitiseData(YesOrNo(isPartialObligation)));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.StatusCode));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.JoinerDate));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.FeeDetail.LeaverDate));
    }
}
