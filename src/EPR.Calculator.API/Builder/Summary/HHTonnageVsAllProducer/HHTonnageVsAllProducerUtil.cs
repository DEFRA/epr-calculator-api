using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.Builder.Summary.HHTonnageVsAllProducer;

public static class HHTonnageVsAllProducerUtil
{
    public static decimal GetPercentageofProducerReportedHHTonnagevsAllProducersTotal(List<ProducerDetail> producers, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
    {
        decimal totalPercentageofProducerReportedHH = 0;

        foreach (var producer in producers)
        {
            totalPercentageofProducerReportedHH += GetPercentageofProducerReportedHHTonnagevsAllProducers(producer, allResults);
        }

        return totalPercentageofProducerReportedHH;
    }

    public static decimal GetPercentageofProducerReportedHHTonnagevsAllProducers(ProducerDetail producer,
        IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
    {
        var allProducerDetails = allResults.Select(x => x.ProducerDetail);
        var allProducerReportedMaterials = allResults.Select(x => x.ProducerReportedMaterial);

        var result =
            (from p in allProducerDetails
             join m in allProducerReportedMaterials
                 on p.Id equals m.ProducerDetailId
             where p.CalculatorRunId == producer.CalculatorRunId && m.PackagingType == "HH"
             group new { m, p } by new { p.ProducerId, p.SubsidiaryId }
                into g
             select new
             {
                 ProducerId = g.Key,
                 g.Key.SubsidiaryId,
                 TotalPackagingTonnage = g.Sum(x => x.m.PackagingTonnage)
             }).ToList();

        var totalTonnage = result.Sum(x => x.TotalPackagingTonnage);
        var producerData = result.FirstOrDefault(r => r.ProducerId.ProducerId == producer.ProducerId && r.ProducerId.SubsidiaryId == producer.SubsidiaryId);
        var PercentageofHHTonnage = producerData != null && totalTonnage > 0
            ? producerData.TotalPackagingTonnage / totalTonnage * 100
            : 0;

        return PercentageofHHTonnage;
    }
}