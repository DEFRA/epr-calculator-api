using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public interface ISelfManagedConsumerWasteService
    {
        Task<SelfManagedConsumerWaste>
         Calculate(
            RunContext runContext,
            IEnumerable<MaterialDetail> materialDetails);
    }

    public class SelfManagedConsumerWasteService: ISelfManagedConsumerWasteService
    {
        private readonly ApplicationDBContext context;

        public SelfManagedConsumerWasteService(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<SelfManagedConsumerWaste> Calculate(
            RunContext runContext,
            IEnumerable<MaterialDetail> materialDetails
        )
        {
            // TODO also used by CalcResultSummaryBuilder - look up in CalcResultBuilder...
            var producerMaterialDetails = await (
                from pd in context.ProducerDetail
                join prm in context.ProducerMaterialPackaging on pd.Id equals prm.ProducerDetailId
                where pd.CalculatorRunId == runContext.RunId
                select new CalcResultProducerAndReportMaterialDetail
                {
                    ProducerDetail = pd,
                    ProducerMaterialPackaging = prm,
                }
            ).ToListAsync();

            var projectedMaterialsLookup = producerMaterialDetails
                .ToLookup(
                    x => (x.ProducerDetail.ProducerId, x.ProducerDetail.SubsidiaryId),
                    x => x.ProducerMaterialPackaging
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
                                .Calculate(BuildL1(projectedMaterialsLookup, group, material), runContext.RequiresModulation)
                                .Select(r => (material, result: r))
                        )
                        .GroupBy(x => (x.result.OrgId, x.result.SubsidiaryId, x.result.Level))
                        .Select(g =>
                            new ProducerSelfManagedConsumerWaste
                            {
                                ProducerId    = g.Key.OrgId,
                                SubsidiaryId  = g.Key.SubsidiaryId,
                                Level         = g.Key.Level,
                                SmcwByMaterial = g.ToDictionary(
                                    x => x.material.Code,
                                    x => MapResultToData(x.result)
                                )
                            }
                        )
                )
                .ToList();

            return new SelfManagedConsumerWaste
            {
                CalculatorRunId = runContext.RunId,
                ProducerTotals  = producerTotals,
                TotalByMaterial = materialDetails.ToDictionary(
                    m => m.Code,
                    m => producerTotals
                            .Where(x => x.Level == 1)
                            .Select(x => x.SmcwByMaterial.GetValueOrDefault(m.Code))
                            .Sum()
                )
            };
        }

        private IL1 BuildL1(
            ILookup<(int, string?), ProducerMaterialPackaging> projectedMaterialsLookup,
            IGrouping<int, ProducerDetail> group,
            MaterialDetail material
        )
        {
            if (group.Count() == 1 && group.First().SubsidiaryId == null)
            {
                var p = group.First();
                var (R, A, G, Total) = ProducerFeesUtil.GetReportedTonnagesByRag(projectedMaterialsLookup, p, material);
                return new SingleL1(
                    OrgId: p.ProducerId,
                    R:     R,
                    A:     A,
                    G:     G,
                    Total: Total,
                    Smcw:  ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, p, material, PackagingTypes.ConsumerWaste)
                );
            }

            var l2s = group
                .OrderBy(p => p.ProducerId)
                .ThenBy(p => p.SubsidiaryId)
                .Select(p =>
                {
                    var (R, A, G, Total) = ProducerFeesUtil.GetReportedTonnagesByRag(projectedMaterialsLookup, p, material);
                    return new L2(
                        OrgId:        p.ProducerId,
                        SubsidiaryId: p.SubsidiaryId,
                        R:            R,
                        A:            A,
                        G:            G,
                        Total:        Total,
                        Smcw:         ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, p, material, PackagingTypes.ConsumerWaste)
                    );
                })
                .ToList();

            return new HC(group.Key, l2s);
        }

        private static SelfManagedConsumerWasteData MapResultToData(Result r)
        {
            return new SelfManagedConsumerWasteData
            {
                SmcwTonnage         = r.Smcw,
                ActionedSmcwTonnage = new RamTonnageGroup
                {
                    Total = r.ActionedSmcwTotal,
                    Red   = r.ActionedSmcwR,
                    Amber = r.ActionedSmcwA,
                    Green = r.ActionedSmcwG
                },
                ResidualSmcwTonnage = r.Residual,
                NetTonnage = new RamTonnageGroup
                {
                    Total = r.NetTotal,
                    Red   = r.NetR,
                    Amber = r.NetA,
                    Green = r.NetG
                }
            };
        }
    }
}
