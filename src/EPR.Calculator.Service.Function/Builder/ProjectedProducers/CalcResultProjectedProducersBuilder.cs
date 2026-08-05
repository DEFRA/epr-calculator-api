using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Builder.ProjectedProducers
{
    public interface ICalcResultProjectedProducersBuilder
    {
        (List<L1Producer>, CalcResultProjectedProducers) Construct(
            RunContext runContext,
            IImmutableList<MaterialDetail> materialDetails,
            IReadOnlyCollection<L1Producer> producers
        );
    }

    public class CalcResultProjectedProducersBuilder : ICalcResultProjectedProducersBuilder
    {
        public (List<L1Producer>, CalcResultProjectedProducers) Construct(
            RunContext runContext,
            IImmutableList<MaterialDetail> materialDetails,
            IReadOnlyCollection<L1Producer> producers
        )
        {
            var h2Period = $"{runContext.RelativeYear - 1}-H2";
            var h1Period = $"{runContext.RelativeYear - 1}-H1";

            var allH2Rows = new List<CalcResultH2ProjectedProducer>();
            var allH1Rows = new List<CalcResultH1ProjectedProducer>();
            var updatedProducers = new List<L1Producer>(producers.Count);

            // H1 for a producer only depends on H2 from the same ProducerId group, so each group
            // can be processed fully in one pass.
            foreach (var l1 in producers)
            {
                var groupList = l1.Producers;

                var h2Rows = H2ProjectedProducersBuilderUtils.GetProjectedProducers(groupList, materialDetails, h2Period);
                var h2WithGroupSubtotals = AssignLevels(
                    h2Rows,
                    deriveL1: H2ProjectedProducersBuilderUtils.CreateParentProducer,
                    sumProducerGroupTonnages: H2ProjectedProducersBuilderUtils.SumProducerGroupTonnages
                );

                var h1Rows = H1ProjectedProducersBuilderUtils.GetProjectedProducers(groupList, h2WithGroupSubtotals, materialDetails, h1Period);

                var updatedPds = new List<ProducerDetail>(groupList.Count);
                for (var i = 0; i < groupList.Count; i++)
                    updatedPds.Add(ApplyProjectedMaterials(groupList[i], h1Rows[i], h2Rows[i], materialDetails, h1Period, h2Period));
                updatedProducers.Add(new L1Producer(l1.OrganisationId, updatedPds));

                allH2Rows.AddRange(h2WithGroupSubtotals);
                allH1Rows.AddRange(AssignLevels(
                    h1Rows,
                    deriveL1: p => H1ProjectedProducersBuilderUtils.CreateParentProducer(p, h2WithGroupSubtotals),
                    sumProducerGroupTonnages: group => H1ProjectedProducersBuilderUtils.SumProducerGroupTonnages(group, h2WithGroupSubtotals)
                ));
            }

            var result = new CalcResultProjectedProducers
            {
                H2ProjectedProducers = allH2Rows.OrderBy(p => p.ProducerId).ThenBy(p => p.Level).ThenBy(p => p.SubsidiaryId).ToImmutableList(),
                H1ProjectedProducers = allH1Rows.OrderBy(p => p.ProducerId).ThenBy(p => p.Level).ThenBy(p => p.SubsidiaryId).ToImmutableList()
            };

            return (updatedProducers, result);
        }

        private ProducerDetail ApplyProjectedMaterials(
            ProducerDetail pd,
            ICalcResultProjectedProducer h1Row,
            ICalcResultProjectedProducer h2Row,
            IImmutableList<MaterialDetail> materials,
            string h1Period,
            string h2Period
        )
        {
            var h1ById = h1Row.ProjectedTonnageByMaterial
                .ToDictionary(kvp => materials.FirstOrDefault(m => m.Code == kvp.Key)?.Id ?? -1, kvp => kvp.Value);
            var h2ById = h2Row.ProjectedTonnageByMaterial
                .ToDictionary(kvp => materials.FirstOrDefault(m => m.Code == kvp.Key)?.Id ?? -1, kvp => kvp.Value);

            ProducerReportedMaterial Apply(ProducerReportedMaterial rm, Dictionary<int, CalcResultProjectedProducerMaterialTonnage> projectedById)
            {
                if (!projectedById.TryGetValue(rm.MaterialId, out var projected)) return rm;
                var projectedRam = rm.PackagingType switch
                {
                    PackagingTypes.Household                 => projected.ProjectedHouseholdRAMTonnage,
                    PackagingTypes.PublicBin                 => projected.ProjectedPublicBinRAMTonnage,
                    PackagingTypes.HouseholdDrinksContainers => projected.ProjectedHouseholdDrinksContainerRAMTonnage,
                    _                                        => null
                };
                if (projectedRam == null) return rm;
                return new ProducerReportedMaterial
                {
                    Id                           = rm.Id,
                    MaterialId                   = rm.MaterialId,
                    ProducerDetailId             = rm.ProducerDetailId,
                    PackagingType                = rm.PackagingType,
                    PackagingTonnage             = rm.PackagingTonnage,
                    PackagingTonnageRed          = projectedRam.Red,
                    PackagingTonnageAmber        = projectedRam.Amber,
                    PackagingTonnageGreen        = projectedRam.Green,
                    PackagingTonnageRedMedical   = projectedRam.RedMedical,
                    PackagingTonnageAmberMedical = projectedRam.AmberMedical,
                    PackagingTonnageGreenMedical = projectedRam.GreenMedical,
                    SubmissionPeriod            = rm.SubmissionPeriod,
                    ProducerDetail              = rm.ProducerDetail,
                    Material                    = rm.Material
                };
            }

            return CalcResultPartialObligationBuilder.UpdateReportedMaterials(
                pd,
                reportedMaterials =>
                    reportedMaterials.Where(rm => rm.SubmissionPeriod == h1Period).Select(rm => Apply(rm, h1ById))
                    .Concat(reportedMaterials.Where(rm => rm.SubmissionPeriod == h2Period).Select(rm => Apply(rm, h2ById)))
                    .ToList()
            );
        }


        public static decimal TonnageWithoutRAM(decimal tonnage, RamTonnage ramTonnage)
        {
            return Math.Max(0, tonnage - ramTonnage.TotalRamTonnage());
        }

        public static RamTonnage SumRAMTonnages(List<ICalcResultProjectedProducer> producers, string materialCode, Func<CalcResultProjectedProducerMaterialTonnage, RamTonnage?> getRAMTonnage)
        {
            decimal red = 0, redMed = 0, amber = 0, amberMed = 0, green = 0, greenMed = 0;

            foreach (var p in producers)
            {
                CalcResultProjectedProducerMaterialTonnage? material = p.ProjectedTonnageByMaterial.FirstOrDefault(v => v.Key == materialCode).Value;
                if (material == null) continue;

                var ram = getRAMTonnage(material);
                if (ram == null) continue;

                red      += ram.Red;
                redMed   += ram.RedMedical;
                amber    += ram.Amber;
                amberMed += ram.AmberMedical;
                green    += ram.Green;
                greenMed += ram.GreenMedical;
            }

            return new RamTonnage
            {
                Red          = red,
                RedMedical   = redMed,
                Amber        = amber,
                AmberMedical = amberMed,
                Green        = green,
                GreenMedical = greenMed
            };
        }

        private static List<TSubmissionPeriodProducer> AssignLevels<TSubmissionPeriodProducer>(
            List<TSubmissionPeriodProducer> projectedProducers,
            Func<TSubmissionPeriodProducer, TSubmissionPeriodProducer> deriveL1,
            Func<List<TSubmissionPeriodProducer>, TSubmissionPeriodProducer> sumProducerGroupTonnages)
            where TSubmissionPeriodProducer : ICalcResultProjectedProducer
        {
            List<TSubmissionPeriodProducer> result = [];
            var producerGroups = projectedProducers.GroupBy(p => p.ProducerId);

            foreach (var group in producerGroups)
            {
                if (group.Count() > 1)
                {
                    result.AddRange(group.Select(p => p with { Level = CommonConstants.LevelTwo.ToString() }));
                    result.Add(sumProducerGroupTonnages(group.ToList()));
                }
                else
                {
                    var producer = group.First();

                    if (producer.SubsidiaryId != null)
                    {
                        result.Add(deriveL1(producer) with { Level = CommonConstants.LevelOne.ToString(), IsSubtotal = true, SubsidiaryId = null });
                        result.Add(producer with { Level = CommonConstants.LevelTwo.ToString() });
                    }
                    else
                    {
                        result.Add(deriveL1(producer) with { Level = CommonConstants.LevelOne.ToString() });
                    }
                }
            }

            return result;
        }
    }
}
