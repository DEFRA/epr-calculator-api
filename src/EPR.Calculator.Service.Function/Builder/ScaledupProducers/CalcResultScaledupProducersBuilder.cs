using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.ScaledupProducers
{
    public interface ICalcResultScaledupProducersBuilder
    {
        Task<(List<L1Producer>, CalcResultScaledupProducers)> ConstructAsync(
            RunContext runContext,
            IImmutableList<MaterialDetail> materialDetails,
            List<L1Producer> producers
        );
    }

    public class CalcResultScaledupProducersBuilder : ICalcResultScaledupProducersBuilder
    {
        private const decimal NormalScaleup = 1.0M;

        private readonly ApplicationDBContext dbContext;

        public CalcResultScaledupProducersBuilder(ApplicationDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public static void AddExtraRows(List<CalcResultScaledupProducer> scaledUpProducers, IReadOnlyCollection<Organisation> scaledupOrganisations)
        {
            var level2Rows = scaledUpProducers
                .Where(x => string.IsNullOrEmpty(x.SubsidiaryId))
                .GroupBy(x => new { x.ProducerId, x.SubsidiaryId }).ToList();

            foreach (var row in level2Rows)
            {
                if (scaledUpProducers.Exists(x => x.ProducerId == row.Key.ProducerId && x.SubsidiaryId != null)
                    &&
                    row.Any())
                {
                    var levelRows = scaledUpProducers.Where(x => x.ProducerId == row.Key.ProducerId && string.IsNullOrEmpty(x.SubsidiaryId));
                    foreach (var level2Row in levelRows)
                    {
                        level2Row.Level = CommonConstants.LevelTwo.ToString();
                    }
                }
            }

            var groupByResult = scaledUpProducers
                .Where(x => x.SubsidiaryId != null)
                .GroupBy(x => new { x.ProducerId, x.SubmissionPeriodCode })
                .ToList();

            foreach (var pair in groupByResult)
            {
                var first = pair.First();

                var parentProducer = scaledupOrganisations.FirstOrDefault(so => so.OrganisationId == pair.Key.ProducerId);

                if (parentProducer != null)
                {
                    var extraRow = new CalcResultScaledupProducer
                    {
                        ProducerId             = pair.Key.ProducerId,
                        SubsidiaryId           = string.Empty,
                        ProducerName           = parentProducer.OrganisationName,
                        TradingName            = parentProducer.TradingName,
                        ScaleupFactor          = first.ScaleupFactor,
                        SubmissionPeriodCode   = pair.Key.SubmissionPeriodCode,
                        DaysInSubmissionPeriod = first.DaysInSubmissionPeriod,
                        DaysInWholePeriod      = first.DaysInWholePeriod,
                        Level                  = CommonConstants.LevelOne.ToString(),
                        IsSubtotalRow          = true,
                    };

                    scaledUpProducers.Add(extraRow);
                }
            }
        }

        [SuppressMessage(
            "Critical Code Smell",
            "S3776:Cognitive Complexity of methods should not be too high",
            Justification = "Temporaraly suppress - will refactor later.")]
        public async Task<(List<L1Producer>, CalcResultScaledupProducers)> ConstructAsync(
            RunContext runContext,
            IImmutableList<MaterialDetail> materialDetails,
            List<L1Producer> producers
        )
        {
            var (scaledUpProducers, scaledupOrganisations) = await GetScaledUpDataAsync(runContext.RunId);

            if (!scaledUpProducers.Any())
            {
                var emptyResult = new CalcResultScaledupProducers { ScaledupProducers = [] };
                return (producers, emptyResult);
            }

            AddExtraRows(scaledUpProducers, scaledupOrganisations);

            // ScaleupFactor is period-based, so it is identical across all subsidiaries of the same
            // ProducerId (SubsidiaryId not needed in lookup).
            var scaleupFactorByProducer = scaledUpProducers
                .Where(s => !s.IsSubtotalRow)
                .GroupBy(s => s.ProducerId)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(s => s.SubmissionPeriodCode ?? "N/A")
                          .ToDictionary(sg => sg.Key, sg => sg.First().ScaleupFactor)
                );

            var displayRowLookup = scaledUpProducers
                .Where(s => !s.IsSubtotalRow)
                .GroupBy(s => (s.ProducerId, s.SubsidiaryId))
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(s => s.SubmissionPeriodCode ?? "N/A", s => s)
                );

            var subtotalLookup = scaledUpProducers
                .Where(s => s.IsSubtotalRow)
                .ToDictionary(s => (s.ProducerId, s.SubmissionPeriodCode ?? "N/A"));

            // Aggregates original (pre-scale) POM data per (ProducerId, Period) for subtotal rows
            var subtotalAccumulator = new Dictionary<(int, string), List<ScaledupPomEntry>>();
            var updatedL1Producers = new List<L1Producer>(producers.Count);

            foreach (var l1 in producers)
            {
                if (!scaleupFactorByProducer.TryGetValue(l1.OrganisationId, out var factorByPeriod))
                {
                    updatedL1Producers.Add(l1);
                    continue;
                }
                var updatedPds = new List<ProducerDetail>(l1.Producers.Count);
                foreach (var pd in l1.Producers)
                {
                    // Scale each material once: the scaled result feeds both the pipeline and the display entry
                    var scaledWithEntries = pd.ProducerReportedMaterials
                        .Select(rm =>
                        {
                            var scaledRm = factorByPeriod.TryGetValue(rm.SubmissionPeriod, out var factor) ? Scale(rm, factor) : rm;
                            return (scaledRm, entry: new ScaledupPomEntry(rm.MaterialId, rm.PackagingType, rm.PackagingTonnage, scaledRm.PackagingTonnage));
                        })
                        .ToList();

                    if (displayRowLookup.TryGetValue((pd.ProducerId, pd.SubsidiaryId), out var periodToRow))
                    {
                        foreach (var (period, row) in periodToRow)
                        {
                            var entries = scaledWithEntries
                                .Where(x => x.scaledRm.SubmissionPeriod == period)
                                .Select(x => x.entry)
                                .ToList();
                            row.PomData = entries;

                            var key = (pd.ProducerId, period);
                            if (!subtotalAccumulator.ContainsKey(key))
                                subtotalAccumulator[key] = [];
                            subtotalAccumulator[key].AddRange(entries);
                        }

                        updatedPds.Add(CalcResultPartialObligationBuilder.UpdateReportedMaterials(
                            pd,
                            _ => scaledWithEntries.Select(x => x.scaledRm).ToList()
                        ));
                    }
                }
                updatedL1Producers.Add(new L1Producer(l1.OrganisationId, updatedPds));
            }

            foreach (var (key, subtotalRow) in subtotalLookup)
            {
                if (subtotalAccumulator.TryGetValue(key, out var pomData))
                {
                    subtotalRow.PomData = pomData
                        .GroupBy(e => (e.MaterialId, e.PackagingType))
                        .Select(g => new ScaledupPomEntry(
                            g.Key.MaterialId, g.Key.PackagingType,
                            g.Sum(e => e.Tonnage), g.Sum(e => e.ScaledTonnage)))
                        .ToList();
                }
            }

            var orderedRows = scaledUpProducers
                .OrderBy(p => p.ProducerId)
                .ThenBy(p => p.Level)
                .ThenBy(p => p.SubsidiaryId)
                .ThenBy(p => p.SubmissionPeriodCode)
                .ToList();

            var scaledupProducersSummary = new CalcResultScaledupProducers {  ScaledupProducers = orderedRows.ToImmutableList() };
            return (updatedL1Producers, scaledupProducersSummary);
        }

        private ProducerReportedMaterial Scale(ProducerReportedMaterial reportedMaterial, decimal scaleupFactor)
        {
            // only scale total - Ram doesn't apply to 2025 relative year (2024 pom)
            var tonnage  = Math.Round(scaleupFactor * reportedMaterial.PackagingTonnage, 3);
            return new ProducerReportedMaterial
            {
                Id               = reportedMaterial.Id,
                MaterialId       = reportedMaterial.MaterialId,
                ProducerDetailId = reportedMaterial.ProducerDetailId,
                PackagingType    = reportedMaterial.PackagingType,
                PackagingTonnage = tonnage,
                SubmissionPeriod = reportedMaterial.SubmissionPeriod,
	            ProducerDetail   = reportedMaterial.ProducerDetail,
	            Material         = reportedMaterial.Material
            };
        }

        private async Task<(List<CalcResultScaledupProducer> producers, ImmutableList<Organisation> organisations)> GetScaledUpDataAsync(int runId)
        {
            var scaledProducerIds = await (
                from run in dbContext.CalculatorRuns.AsNoTracking()
                join crpdd in dbContext.CalculatorRunPomDataDetails.AsNoTracking() on run.CalculatorRunPomDataMasterId equals crpdd.CalculatorRunPomDataMasterId
                join spl in dbContext.SubmissionPeriodLookup.AsNoTracking() on crpdd.SubmissionPeriod equals spl.SubmissionPeriod
                where run.Id == runId && spl.ScaleupFactor > NormalScaleup
                select crpdd.OrganisationId
            ).Distinct().ToListAsync();

            if (scaledProducerIds.Count == 0)
                return ([], []);

            var rows = await (
                from run in dbContext.CalculatorRuns.AsNoTracking()
                join crpdd in dbContext.CalculatorRunPomDataDetails.AsNoTracking() on run.CalculatorRunPomDataMasterId equals crpdd.CalculatorRunPomDataMasterId
                join spl in dbContext.SubmissionPeriodLookup.AsNoTracking() on crpdd.SubmissionPeriod equals spl.SubmissionPeriod
                join pd in dbContext.ProducerDetail.AsNoTracking() on crpdd.OrganisationId equals pd.ProducerId
                join crodm in dbContext.CalculatorRunOrganisationDataMaster.AsNoTracking() on run.CalculatorRunOrganisationDataMasterId equals crodm.Id
                join org in dbContext.CalculatorRunOrganisationDataDetails.AsNoTracking()
                  on new { crodm.Id, pd.ProducerId, pd.SubsidiaryId, crpdd.SubmitterId }
                    equals new { Id = org.CalculatorRunOrganisationDataMasterId, ProducerId = org.OrganisationId, org.SubsidiaryId, org.SubmitterId }
                where run.Id == runId && scaledProducerIds.Contains(crpdd.OrganisationId)
                  && pd.CalculatorRunId == runId && org.ObligationStatus == ObligationStates.Obligated
                select new
                {
                    ProducerId             = pd.ProducerId,
                    SubsidiaryId           = pd.SubsidiaryId,
                    ProducerName           = pd.ProducerName,
                    TradingName            = pd.TradingName,
                    OrgName                = org.OrganisationName,
                    ScaleupFactor          = spl.ScaleupFactor,
                    SubmissionPeriodCode   = spl.SubmissionPeriod,
                    DaysInSubmissionPeriod = spl.DaysInSubmissionPeriod,
                    DaysInWholePeriod      = spl.DaysInWholePeriod,
                }
            ).Distinct().ToImmutableListAsync();

            var producers = rows.Select(r => new CalcResultScaledupProducer
            {
                ProducerId             = r.ProducerId,
                SubsidiaryId           = r.SubsidiaryId,
                ProducerName           = r.ProducerName,
                TradingName            = r.TradingName,
                ScaleupFactor          = r.ScaleupFactor,
                SubmissionPeriodCode   = r.SubmissionPeriodCode,
                DaysInSubmissionPeriod = r.DaysInSubmissionPeriod,
                DaysInWholePeriod      = r.DaysInWholePeriod,
                Level                  = string.IsNullOrEmpty(r.SubsidiaryId) ? CommonConstants.LevelOne.ToString() : CommonConstants.LevelTwo.ToString(),
            }).ToList();

            var organisations = rows
                .Where(r => string.IsNullOrEmpty(r.SubsidiaryId))
                .DistinctBy(o => o.ProducerId)
                .Select(r => new Organisation { OrganisationId = r.ProducerId, OrganisationName = r.OrgName, TradingName = r.TradingName })
                .ToImmutableList();

            return (producers, organisations);
        }
    }
}
