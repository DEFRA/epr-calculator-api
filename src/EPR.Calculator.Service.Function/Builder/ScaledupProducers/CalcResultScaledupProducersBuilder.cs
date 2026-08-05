using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.ScaledupProducers;

public interface ICalcResultScaledupProducersBuilder
{
    Task<(List<L1Producer>, CalcResultScaledupProducers)> ConstructAsync(
        RunContext runContext,
        IImmutableList<MaterialDetail> materialDetails,
        List<L1Producer> producers
    );
}

public class CalcResultScaledupProducersBuilder(ApplicationDBContext dbContext) : ICalcResultScaledupProducersBuilder
{
    private const decimal NormalScaleup = 1.0M;

    private readonly ApplicationDBContext dbContext = dbContext;

    public static List<CalcResultScaledupProducer> BuildDisplayRows(
        IEnumerable<CalcResultScaledupProducer> producers,
        IReadOnlyDictionary<int, Organisation>? parentOrganisations = null)
    {
        var orderedRows = producers
            .OrderBy(x => x.ProducerId)
            .ThenBy(x => x.SubsidiaryId)
            .ToList();

        var parentProducerLookup = orderedRows
            .DistinctBy(x => x.ProducerId)
            .ToDictionary(x => x.ProducerId);

        var producersWithSubsidiaries = orderedRows
            .Where(x => !string.IsNullOrEmpty(x.SubsidiaryId))
            .Select(x => x.ProducerId)
            .ToHashSet();

        var displayRows = orderedRows
            .Select(row =>
                string.IsNullOrEmpty(row.SubsidiaryId) &&
                producersWithSubsidiaries.Contains(row.ProducerId)
                    ? row with { Level = CommonConstants.LevelTwo.ToString() }
                    : row)
            .ToList();

        displayRows.AddRange(
            orderedRows
                .Where(x => !string.IsNullOrEmpty(x.SubsidiaryId))
                .GroupBy(x => new { x.ProducerId, x.SubmissionPeriodCode })
                .Select(group =>
                {
                    // Prefer the registered holding company (SubsidiaryId is null) organisation name,
                    // even when the parent itself has no POM data of its own. Only fall back to a
                    // subsidiary's name when the parent isn't separately registered at all.
                    var (producerName, tradingName) =
                        parentOrganisations != null && parentOrganisations.TryGetValue(group.Key.ProducerId, out var parentOrg)
                            ? (parentOrg.OrganisationName, parentOrg.TradingName)
                            : (parentProducerLookup[group.Key.ProducerId].ProducerName, parentProducerLookup[group.Key.ProducerId].TradingName);

                    return group.First() with
                    {
                        ProducerName  = producerName,
                        TradingName   = tradingName,
                        SubsidiaryId  = string.Empty,
                        Level         = CommonConstants.LevelOne.ToString(),
                        IsSubtotalRow = true,
                    };
                }));

        return displayRows;
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
        var (scaledUpProducers, parentOrganisations) = await GetScaledUpDataAsync(runContext.RunId);

        if (!scaledUpProducers.Any())
        {
            var emptyResult = new CalcResultScaledupProducers { ScaledupProducers = [] };
            return (producers, emptyResult);
        }

        scaledUpProducers = BuildDisplayRows(scaledUpProducers, parentOrganisations);

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
            .ThenBy(p => p.SubmissionPeriodCode)
            .ThenBy(p => p.Level)
            .ThenBy(p => p.SubsidiaryId)
            .ToList();

        var scaledupProducersSummary = new CalcResultScaledupProducers {  ScaledupProducers = orderedRows.ToImmutableList() };
        return (updatedL1Producers, scaledupProducersSummary);
    }

    private static ProducerReportedMaterial Scale(ProducerReportedMaterial reportedMaterial, decimal scaleupFactor)
    {
        // only scale total - Ram doesn't apply to 2025 relative year (2024 pom)
        var tonnage  = MathUtils.RoundAwayFromZero(scaleupFactor * reportedMaterial.PackagingTonnage, 3);
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

    private async Task<(List<CalcResultScaledupProducer> Producers, Dictionary<int, Organisation> ParentOrganisations)> GetScaledUpDataAsync(int runId)
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
                ScaleupFactor          = spl.ScaleupFactor,
                SubmissionPeriodCode   = spl.SubmissionPeriod,
                DaysInSubmissionPeriod = spl.DaysInSubmissionPeriod,
                DaysInWholePeriod      = spl.DaysInWholePeriod,
            }
        ).Distinct().ToImmutableListAsync();

        // The registered holding company (SubsidiaryId is null) may not submit its own POM data -
        // its subsidiaries may report on its behalf - so this is looked up independently of `rows`
        // above, which is driven off ProducerDetail (POM) data.
        var parentOrganisations = await (
            from run in dbContext.CalculatorRuns.AsNoTracking()
            join crodm in dbContext.CalculatorRunOrganisationDataMaster.AsNoTracking() on run.CalculatorRunOrganisationDataMasterId equals crodm.Id
            join org in dbContext.CalculatorRunOrganisationDataDetails.AsNoTracking() on crodm.Id equals org.CalculatorRunOrganisationDataMasterId
            where run.Id == runId && scaledProducerIds.Contains(org.OrganisationId)
              && org.SubsidiaryId == null && org.ObligationStatus == ObligationStates.Obligated
            select new Organisation
            {
                OrganisationId   = org.OrganisationId,
                OrganisationName = org.OrganisationName,
                TradingName      = org.TradingName,
            }
        ).Distinct().ToDictionaryAsync(o => o.OrganisationId);

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

        return (producers, parentOrganisations);
    }
}
