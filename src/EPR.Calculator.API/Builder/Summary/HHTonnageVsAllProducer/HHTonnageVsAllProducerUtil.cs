using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.Builder.Summary.HHTonnageVsAllProducer;

public static class HHTonnageVsAllProducerUtil
{
    public static decimal GetPercentageofProducerReportedHHTonnagevsAllProducersTotal(List<ProducerDetail> producers, IEnumerable<HHTotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
    {
        decimal totalPercentageofProducerReportedHH = 0;

        foreach (var producer in producers)
        {
            totalPercentageofProducerReportedHH += GetPercentageofProducerReportedHHTonnagevsAllProducers(producer, hhTotalPackagingTonnage);
        }

        return totalPercentageofProducerReportedHH;
    }

    public static decimal GetPercentageofProducerReportedHHTonnagevsAllProducers(ProducerDetail producer, IEnumerable<HHTotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
    {
        var totalTonnage = hhTotalPackagingTonnage.Sum(x => x.TotalPackagingTonnage);
        var producerData = hhTotalPackagingTonnage.FirstOrDefault(r => r.ProducerId == producer.ProducerId && r.SubsidiaryId == producer.SubsidiaryId);
        var PercentageofHHTonnage = producerData != null && totalTonnage > 0
            ? producerData.TotalPackagingTonnage / totalTonnage * 100
            : 0;
        return PercentageofHHTonnage;
    }
}