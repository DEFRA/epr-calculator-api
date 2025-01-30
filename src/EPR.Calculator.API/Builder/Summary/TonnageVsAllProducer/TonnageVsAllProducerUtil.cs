using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.Builder.Summary.TonnageVsAllProducer;

public static class TonnageVsAllProducerUtil
{
    public static decimal GetPercentageofProducerReportedTonnagevsAllProducersTotal(List<ProducerDetail> producers, IEnumerable<TotalPackagingTonnagePerRun> totalPackagingTonnage)
    {
        decimal totalPercentageofProducerReportedHH = 0;

        foreach (var producer in producers)
        {
            totalPercentageofProducerReportedHH += GetPercentageofProducerReportedTonnagevsAllProducers(producer, totalPackagingTonnage);
        }

        return totalPercentageofProducerReportedHH;
    }

    public static decimal GetPercentageofProducerReportedTonnagevsAllProducers(ProducerDetail producer, IEnumerable<TotalPackagingTonnagePerRun> totalPackagingTonnage)
    {
        var totalTonnage = totalPackagingTonnage.Sum(x => x.TotalPackagingTonnage);
        var producerData = totalPackagingTonnage.FirstOrDefault(r => r.ProducerId == producer.ProducerId && r.SubsidiaryId == producer.SubsidiaryId);
        var PercentageofHHTonnage = producerData != null && totalTonnage > 0
            ? producerData.TotalPackagingTonnage / totalTonnage * 100
            : 0;
        return PercentageofHHTonnage;
    }
}