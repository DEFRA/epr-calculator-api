using EPR.Calculator.API.Builder.Summary;
using EPR.Calculator.API.Builder.Summary.Common;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Builder.ScaledupProducers
{
    public class CalcResultScaledupProducersBuilder : ICalcResultScaledupProducersBuilder
    {
        private readonly ApplicationDBContext context;

        public CalcResultScaledupProducersBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<CalcResultScaledupProducers> Construct(CalcResultsRequestDto resultsRequestDto,
            CalcResult calcResult,
            IEnumerable<ScaledupProducer> scaledupProducers)
        {
            var runId = resultsRequestDto.RunId;
            var materialsFromDb = await context.Material.ToListAsync();
            var materials = Mappers.MaterialMapper.Map(materialsFromDb);

            var scaledupProducerIds = scaledupProducers.Select((p) => p.ProducerId) ?? [];

            var runProducerMaterialDetails = await (from pd in context.ProducerDetail
                                                    join prm in context.ProducerReportedMaterial on pd.Id equals prm.ProducerDetailId
                                                    where pd.CalculatorRunId == runId
                                                    select new CalcResultsProducerAndReportMaterialDetail
                                                    {
                                                        ProducerDetail = pd,
                                                        ProducerReportedMaterial = prm
                                                    }).ToListAsync();

            var producerDetails = runProducerMaterialDetails
                .Select(p => p.ProducerDetail)
                .Distinct()
                .OrderBy(p => p.ProducerId)
                .ToList();

            return GetCalcResultScaledupProducers(producerDetails, materials, runProducerMaterialDetails, calcResult);
        }

        private static CalcResultScaledupProducers GetCalcResultScaledupProducers(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            IEnumerable<CalcResultsProducerAndReportMaterialDetail> runProducerMaterialDetails,
            CalcResult calcResult)
        {
            var scaledupProducersSummary = new CalcResultScaledupProducers();
            if (producers.Any())
            {
                var scaledupProducers = new List<CalcResultScaledupProducer>();

                foreach (var producer in producers)
                {
                    // We have to write an additional row if a producer have at least one subsidiary
                    // This additional row will be the total of this producer and its subsidiaries
                    var producersAndSubsidiaries = producers.Where(pd => pd.ProducerId == producer.ProducerId);
                    // Make sure the total row is written only once
                    if (producersAndSubsidiaries.Count() > 1 &&
                        scaledupProducers.Find(p => p.ProducerId == producer.ProducerId.ToString()) == null)
                    {
                        var totalRow = GetProducerTotalRow(producersAndSubsidiaries, materials, false);
                        scaledupProducers.Add(totalRow);
                    }

                    // Calculate the values for the producer
                    var scaledupProducer = GetProducerRow(producer, materials);
                    scaledupProducers.Add(scaledupProducer);
                }

                // Calculate the total for all the producers
                var allTotalRow = GetProducerTotalRow(producers, materials, true);
                scaledupProducers.Add(allTotalRow);

                scaledupProducersSummary.ScaledupProducers = scaledupProducers;
            }

            return scaledupProducersSummary;
        }

        private static CalcResultScaledupProducer GetProducerRow(ProducerDetail producer,
            IEnumerable<MaterialDetail> materials)
        {
            return new CalcResultScaledupProducer
            {
                ProducerId = producer.ProducerId.ToString(),
                ProducerName = producer.ProducerName ?? string.Empty,
                SubsidiaryId = producer.SubsidiaryId ?? string.Empty,
                Level = "1", // CalcResultSummaryUtil.GetLevelIndex(calcResultScaledupProducers, producer).ToString(),
                SubmissonPeriodCode = string.Empty,
                DaysInSubmissionPeriod = 0,
                DaysInWholePeriod = 0,
                ScaleupFactor = 0,
                ScaledupProducerTonnageByMaterial = GetScaledupProducerTonnages(materials)
            };
        }

        private static CalcResultScaledupProducer GetProducerTotalRow(IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            bool isOverAllTotalRow)
        {
            var producers = producersAndSubsidiaries.ToList();
            return new CalcResultScaledupProducer
            {
                ProducerId = isOverAllTotalRow ? string.Empty : producers[0].ProducerId.ToString(),
                ProducerName = isOverAllTotalRow ? string.Empty : producers[0].ProducerName ?? string.Empty,
                SubsidiaryId = string.Empty,
                Level = isOverAllTotalRow ? "Totals" : "1", // CalcResultSummaryUtil.GetLevelIndex(calcResultScaledupProducers, producer).ToString(),
                SubmissonPeriodCode = string.Empty,
                DaysInSubmissionPeriod = 0,
                DaysInWholePeriod = 0,
                ScaleupFactor = 0,
                ScaledupProducerTonnageByMaterial = GetScaledupProducerTonnages(materials)
            };
        }

        private static Dictionary<string, CalcResultScaledupProducerTonnage> GetScaledupProducerTonnages(IEnumerable<MaterialDetail> materials)
        {
            var scaledupProducerTonnages = new Dictionary<string, CalcResultScaledupProducerTonnage>();
            foreach (var material in materials)
            {
                var scaledupProducerTonnage = new CalcResultScaledupProducerTonnage
                {
                    ReportedHouseholdPackagingWasteTonnage = ScaledupUtil.GetReportedHouseholdPackagingWasteTonnage(),
                    ReportedPublicBinTonnage = ScaledupUtil.GetReportedPublicBinTonnage(),
                    TotalReportedTonnage = ScaledupUtil.GetTotalReportedTonnage(),
                    ReportedSelfManagedConsumerWasteTonnage = ScaledupUtil.GetReportedSelfManagedConsumerWasteTonnage(),
                    NetReportedTonnage = ScaledupUtil.GetNetReportedTonnage(),
                    ScaledupReportedHouseholdPackagingWasteTonnage = ScaledupUtil.GetScaledupReportedHouseholdPackagingWasteTonnage(),
                    ScaledupReportedPublicBinTonnage = ScaledupUtil.GetScaledupReportedPublicBinTonnage(),
                    ScaledupTotalReportedTonnage = ScaledupUtil.GetScaledupTotalReportedTonnage(),
                    ScaledupReportedSelfManagedConsumerWasteTonnage = ScaledupUtil.GetScaledupReportedSelfManagedConsumerWasteTonnage(),
                    ScaledupNetReportedTonnage = ScaledupUtil.GetScaledupNetReportedTonnage()
                };
                scaledupProducerTonnages.Add(material.Name, scaledupProducerTonnage);
            }
            return scaledupProducerTonnages;
        }
    }
}
