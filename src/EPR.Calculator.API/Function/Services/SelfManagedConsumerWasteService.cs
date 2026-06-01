using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public interface ISelfManagedConsumerWasteService
    {
        Task<SelfManagedConsumerWaste>
         Calculate(
            CalcResultsRequestDto resultsRequestDto,
            IEnumerable<MaterialDetail> materialDetails,
            bool applyModulation);
    }

    public class SelfManagedConsumerWasteService: ISelfManagedConsumerWasteService
    {
        private readonly ApplicationDBContext context;

        public SelfManagedConsumerWasteService(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<SelfManagedConsumerWaste> Calculate(
            CalcResultsRequestDto resultsRequestDto,
            IEnumerable<MaterialDetail> materialDetails,
            bool applyModulation
        )
        {
            // TODO also used by CalcResultSummaryBuilder - look up in CalcResultBuilder...
            var producerMaterialDetails = await (
                from pd in context.ProducerDetail
                join prm in context.ProducerReportedMaterialProjected on pd.Id equals prm.ProducerDetailId
                where pd.CalculatorRunId == resultsRequestDto.RunId
                select new CalcResultProducerAndReportMaterialDetail
                {
                    ProducerDetail = pd,
                    ProducerReportedMaterialProjected = prm,
                }
            ).ToListAsync();

            var projectedMaterialsLookup = producerMaterialDetails
                .ToLookup(
                    x => (x.ProducerDetail.ProducerId, x.ProducerDetail.SubsidiaryId),
                    x => x.ProducerReportedMaterialProjected
                );

            var producerDetails = producerMaterialDetails
               .Select(x => x.ProducerDetail)
               .DistinctBy(x => (x.ProducerId, x.SubsidiaryId))
               .ToList();

            var producerTotals = producerDetails
                .GroupBy(x => x.ProducerId)
                .SelectMany(group =>
                    materialDetails
                        .SelectMany(material =>
                            SelfManagedConsumerWasteServiceLevels
                                .Calculate(BuildL1(projectedMaterialsLookup, group, material), applyModulation)
                                .Select(r => (material, result: r))
                        )
                        .GroupBy(x => (x.result.OrgId, x.result.SubsidiaryId, x.result.Level))
                        .Select(g =>
                            new ProducerSelfManagedConsumerWaste
                            {
                                ProducerId                               = g.Key.OrgId,
                                SubsidiaryId                             = g.Key.SubsidiaryId,
                                Level                                    = g.Key.Level,
                                SelfManagedConsumerWasteDataPerMaterials = g.ToDictionary(x => x.material.Code, x => MapResultToData(x.result))
                            }
                        )
                )
                .ToList();

            return new SelfManagedConsumerWaste
            {
                ProducerTotals  = producerTotals,
                OverallTotalPerMaterials = materialDetails.ToDictionary(
                    m => m.Code,
                    m => producerTotals
                        .Where(x => x.Level == 1)
                        .Select(x => x.SelfManagedConsumerWasteDataPerMaterials.GetValueOrDefault(m.Code))
                        .Sum()
                )
            };
        }

        private IL1 BuildL1(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IGrouping<int, ProducerDetail> group,
            MaterialDetail material
        )
        {
            if (group.Count() == 1 && group.First().SubsidiaryId == null)
            {
                var p = group.First();

                return new SingleL1(
                    OrgId: p.ProducerId,
                    R:     CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material, RagRating.Red) +
                           CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material, RagRating.RedMedical),
                    A:     CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material, RagRating.Amber) +
                           CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material, RagRating.AmberMedical),
                    G:     CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material, RagRating.Green) +
                           CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material, RagRating.GreenMedical),
                    Total: CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material),
                    Smcw:  CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, p, material, PackagingTypes.ConsumerWaste)
                );
            }

            var l2s = group
                .OrderBy(p => p.ProducerId)
                .ThenBy(p => p.SubsidiaryId)
                .Select(p => new L2(
                    OrgId:        p.ProducerId,
                    SubsidiaryId: p.SubsidiaryId,
                    R:            CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material, RagRating.Red) +
                                  CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material, RagRating.RedMedical),
                    A:            CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material, RagRating.Amber) +
                                  CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material, RagRating.AmberMedical),
                    G:            CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material, RagRating.Green) +
                                  CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material, RagRating.GreenMedical),
                    Total:        CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material),
                    Smcw:         CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, p, material, PackagingTypes.ConsumerWaste)
                )).ToList();

            return new HC(group.Key, l2s);
        }

        private static SelfManagedConsumerWasteData MapResultToData(Result r)
        {
            return new SelfManagedConsumerWasteData
            {
                SelfManagedConsumerWasteTonnage         = r.Smcw,
                ActionedSelfManagedConsumerWasteTonnage = (
                    total: r.ActionedSmcwTotal,
                    red  : r.ActionedSmcwR,
                    amber: r.ActionedSmcwA,
                    green: r.ActionedSmcwG
                ),
                ResidualSelfManagedConsumerWasteTonnage = r.Residual,
                NetReportedTonnage = (
                    total: r.NetTotal,
                    red  : r.NetR,
                    amber: r.NetA,
                    green: r.NetG
                )
            };
        }
    }

    public record SelfManagedConsumerWaste
    {
        public required List<ProducerSelfManagedConsumerWaste> ProducerTotals { get; init; }
        public required Dictionary<string, SelfManagedConsumerWasteData> OverallTotalPerMaterials { get; init; }
    }

    public record ProducerSelfManagedConsumerWaste
    {
        public int ProducerId { get; set; }
        public string? SubsidiaryId { get; set; }
        public required int Level {get; init; }
        public required Dictionary<string, SelfManagedConsumerWasteData> SelfManagedConsumerWasteDataPerMaterials { get; init; }
    }

    public record SelfManagedConsumerWasteData
    {
        public required decimal SelfManagedConsumerWasteTonnage { get; init; }
        public required (decimal? total, decimal? red, decimal? amber, decimal? green) ActionedSelfManagedConsumerWasteTonnage { get; init; }
        public required decimal? ResidualSelfManagedConsumerWasteTonnage { get; init; }
        public required (decimal? total, decimal? red, decimal? amber, decimal? green) NetReportedTonnage { get; init; }

        public static SelfManagedConsumerWasteData Zero => new()
        {
            SelfManagedConsumerWasteTonnage         = 0,
            ActionedSelfManagedConsumerWasteTonnage = (0, 0, 0, 0),
            ResidualSelfManagedConsumerWasteTonnage = 0,
            NetReportedTonnage                      = (0, 0, 0, 0)
        };

        public static SelfManagedConsumerWasteData operator +(
            SelfManagedConsumerWasteData a,
            SelfManagedConsumerWasteData b)
        {
            return new SelfManagedConsumerWasteData
            {
                SelfManagedConsumerWasteTonnage =
                    a.SelfManagedConsumerWasteTonnage + b.SelfManagedConsumerWasteTonnage,

                ActionedSelfManagedConsumerWasteTonnage = (
                    (a.ActionedSelfManagedConsumerWasteTonnage.total ?? 0) + (b.ActionedSelfManagedConsumerWasteTonnage.total ?? 0),
                    (a.ActionedSelfManagedConsumerWasteTonnage.red   ?? 0) + (b.ActionedSelfManagedConsumerWasteTonnage.red   ?? 0),
                    (a.ActionedSelfManagedConsumerWasteTonnage.amber ?? 0) + (b.ActionedSelfManagedConsumerWasteTonnage.amber ?? 0),
                    (a.ActionedSelfManagedConsumerWasteTonnage.green ?? 0) + (b.ActionedSelfManagedConsumerWasteTonnage.green ?? 0)
                ),

                ResidualSelfManagedConsumerWasteTonnage =
                    (a.ResidualSelfManagedConsumerWasteTonnage ?? 0) +
                    (b.ResidualSelfManagedConsumerWasteTonnage ?? 0),

                NetReportedTonnage = (
                    (a.NetReportedTonnage.total ?? 0) + (b.NetReportedTonnage.total ?? 0),
                    (a.NetReportedTonnage.red   ?? 0) + (b.NetReportedTonnage.red   ?? 0),
                    (a.NetReportedTonnage.amber ?? 0) + (b.NetReportedTonnage.amber ?? 0),
                    (a.NetReportedTonnage.green ?? 0) + (b.NetReportedTonnage.green ?? 0)
                )
            };
        }
    }

    public static class SelfManagedConsumerWasteDataExtensions
    {
        public static SelfManagedConsumerWasteData Sum(this IEnumerable<SelfManagedConsumerWasteData?> source)
        {
            return source.Aggregate(
                SelfManagedConsumerWasteData.Zero,
                (acc, x) => acc + (x ?? SelfManagedConsumerWasteData.Zero)
            );
        }
    }
}
