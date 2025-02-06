﻿using EPR.Calculator.API.Builder.Summary;
using EPR.Calculator.API.Builder.Summary.Common;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Builder.ScaledupProducers
{
    public class CalcResultScaledupProducersBuilder : ICalcResultScaledupProducersBuilder
    {
        private const int MaterialsBreakdownHeaderInitialColumnIndex = 9;
        private const int MaterialsBreakdownHeaderIncrementalColumnIndex = 10;

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

            var scaledupProducersSummary = new CalcResultScaledupProducers();
            if (scaledupProducers.Any())
            {
                var scaleupProducerIds = await GetScaledUpProducerIds(resultsRequestDto.RunId);
                var producerIds = scaleupProducerIds.Select(x => x.ProducerId);
                var runProducerMaterialDetails = await (from pd in context.ProducerDetail
                                                        join prm in context.ProducerReportedMaterial on pd.Id equals prm.ProducerDetailId
                                                        where pd.CalculatorRunId == runId  && producerIds.Contains(pd.ProducerId)
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
                scaledupProducersSummary = GetCalcResultScaledupProducers(producerDetails, materials, runProducerMaterialDetails, calcResult, scaleupProducerIds);
            }
            SetHeaders(scaledupProducersSummary, materials);
            return scaledupProducersSummary;
        }

        private async Task<IEnumerable<CalcResultScaleupProducer>> GetScaledUpProducerIds(int runId)
        {
            var scaleupProducerIds = await (from run in context.CalculatorRuns
                                            join pd in context.ProducerDetail on run.Id equals pd.CalculatorRunId
                                            join prm in context.ProducerReportedMaterial on pd.Id equals prm.ProducerDetailId
                                            join crpdm in context.CalculatorRunPomDataMaster on run.CalculatorRunPomDataMasterId equals crpdm.Id
                                            join crpdd in context.CalculatorRunPomDataDetails on crpdm.Id equals crpdd.CalculatorRunPomDataMasterId
                                            join spl in context.SubmissionPeriodLookup on crpdd.SubmissionPeriod equals spl.SubmissionPeriod
                                            where run.Id == runId && spl.ScaleupFactor > 1 && crpdd.OrganisationId != null
                                            select new CalcResultScaleupProducer
                                            {
                                                ProducerId = crpdd.OrganisationId.Value,
                                                ScaleupFactor = spl.ScaleupFactor
                                            }
                               ).Distinct().ToListAsync();
            return scaleupProducerIds;
        }

        private static CalcResultScaledupProducers GetCalcResultScaledupProducers(IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            IEnumerable<CalcResultsProducerAndReportMaterialDetail> runProducerMaterialDetails,
            CalcResult calcResult,
            IEnumerable<CalcResultScaleupProducer> scaleupProducerIds)
        {
            var scaledupProducersSummary = new CalcResultScaledupProducers();
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
                var scaledupProducer = GetProducerRow(producer, materials, scaleupProducerIds);
                scaledupProducers.Add(scaledupProducer);
            }

            // Calculate the total for all the producers
            var allTotalRow = GetProducerTotalRow(producers, materials, true);
            scaledupProducers.Add(allTotalRow);

            scaledupProducersSummary.ScaledupProducers = scaledupProducers;

            return scaledupProducersSummary;
        }

        private static CalcResultScaledupProducer GetProducerRow(ProducerDetail producer,
            IEnumerable<MaterialDetail> materials,
            IEnumerable<CalcResultScaleupProducer> scaleupProducerIds)
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
                ScaledupProducerTonnageByMaterial = GetScaledupProducerTonnages(producer, materials, scaleupProducerIds)
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
                ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>()
            };
        }

        private static Dictionary<string, CalcResultScaledupProducerTonnage> GetScaledupProducerTonnages(
            ProducerDetail producer,
            IEnumerable<MaterialDetail> materials,
            IEnumerable<CalcResultScaleupProducer> scaleupProducerIds)
        {
            var scaledupProducerTonnages = new Dictionary<string, CalcResultScaledupProducerTonnage>();
            foreach (var material in materials)
            {
                var scaledupProducerTonnage = new CalcResultScaledupProducerTonnage();
                scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetHouseholdPackagingWasteTonnage(producer, material);
                scaledupProducerTonnage.ReportedPublicBinTonnage = CalcResultSummaryUtil.GetReportedPublicBinTonnage(producer, material);
                scaledupProducerTonnage.TotalReportedTonnage = scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage +
                    scaledupProducerTonnage.ReportedPublicBinTonnage;

                scaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage = ScaledupUtil.GetReportedSelfManagedConsumerWasteTonnage();
                scaledupProducerTonnage.NetReportedTonnage = ScaledupUtil.GetNetReportedTonnage();
                scaledupProducerTonnage.ScaledupReportedHouseholdPackagingWasteTonnage = ScaledupUtil.GetScaledupReportedHouseholdPackagingWasteTonnage();
                scaledupProducerTonnage.ScaledupReportedPublicBinTonnage = ScaledupUtil.GetScaledupReportedPublicBinTonnage();
                scaledupProducerTonnage.ScaledupTotalReportedTonnage = ScaledupUtil.GetScaledupTotalReportedTonnage();
                scaledupProducerTonnage.ScaledupReportedSelfManagedConsumerWasteTonnage = ScaledupUtil.GetScaledupReportedSelfManagedConsumerWasteTonnage();
                scaledupProducerTonnage.ScaledupNetReportedTonnage = ScaledupUtil.GetScaledupNetReportedTonnage();
                scaledupProducerTonnages.Add(material.Name, scaledupProducerTonnage);
            }
            return scaledupProducerTonnages;
        }

        private static void SetHeaders(CalcResultScaledupProducers producers, IEnumerable<MaterialDetail> materials)
        {
            producers.TitleHeader = new CalcResultScaledupProducerHeader
            {
                Name = CalcResultScaledupProducerHeaders.ScaledupProducers,
                ColumnIndex = 1
            };

            producers.MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(materials);

            producers.ColumnHeaders = GetColumnHeaders(materials);
        }

        public static List<CalcResultSummaryHeader> GetMaterialsBreakdownHeader(IEnumerable<MaterialDetail> materials)
        {
            var materialsBreakdownHeaders = new List<CalcResultSummaryHeader>();
            var columnIndex = MaterialsBreakdownHeaderInitialColumnIndex;

            materialsBreakdownHeaders.Add(new CalcResultSummaryHeader
            {
                Name = CalcResultScaledupProducerHeaders.EachSubmissionForTheYear,
                ColumnIndex = 1
            });

            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultSummaryHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex
                });

                columnIndex = material.Code == MaterialCodes.Glass
                    ? columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex + 1
                    : columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex;
            }

            return materialsBreakdownHeaders;
        }

        public static List<CalcResultSummaryHeader> GetColumnHeaders(IEnumerable<MaterialDetail> materials)
        {
            var columnHeaders = new List<CalcResultSummaryHeader>();

            columnHeaders.AddRange([
                new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.ProducerId },
                new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.SubsidiaryId },
                new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.ProducerOrSubsidiaryName },
                new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.Level },
                new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.SubmissionPeriodCode },
                new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.DaysInSubmissionPeriod },
                new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.DaysInWholePeriod },
                new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.ScaleupFactor }
            ]);

            foreach (var material in materials)
            {
                var columnHeadersList = new List<CalcResultSummaryHeader>
                {
                    new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.ReportedHouseholdPackagingWasteTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.ReportedPublicBinTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.TotalReportedTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.ReportedSelfManagedConsumerWasteTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.NetReportedTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.ScaledupReportedHouseholdPackagingWasteTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.ScaledupReportedPublicBinTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.ScaledupTotalReportedTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.ScaledupReportedSelfManagedConsumerWasteTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultScaledupProducerHeaders.ScaledupNetReportedTonnage },
                };

                columnHeaders.AddRange(columnHeadersList);
            }

            return columnHeaders;
        }
    }
}
