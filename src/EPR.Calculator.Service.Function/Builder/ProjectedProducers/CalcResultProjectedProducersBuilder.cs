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
            var h2Period = $"{runContext.RelativeYear.Value - 1}-H2";
            var h1Period = $"{runContext.RelativeYear.Value - 1}-H1";

            var allH2Rows = new List<CalcResultH2ProjectedProducer>();
            var allH1Rows = new List<CalcResultH1ProjectedProducer>();
            var updatedProducers = new List<L1Producer>(producers.Count);

            // H1 for a producer only depends on H2 from the same ProducerId group, so each group
            // can be processed fully in one pass.
            foreach (var l1 in producers)
            {
                var groupList = l1.Producers;

                var h2Rows = H2ProjectedProducersBuilderUtils.GetProjectedProducers(groupList, materialDetails, h2Period);
                var h2WithGroupSubtotals = AddSubtotals<CalcResultH2ProjectedProducer>(
                    h2Rows,
                    createSubtotal: H2ProjectedProducersBuilderUtils.CreateParentProducer,
                    sumProducerGroupTonnages: H2ProjectedProducersBuilderUtils.SumProducerGroupTonnages
                );

                var h1Rows = H1ProjectedProducersBuilderUtils.GetProjectedProducers(groupList, h2WithGroupSubtotals, materialDetails, h1Period);

                var updatedPds = new List<ProducerDetail>(groupList.Count);
                for (var i = 0; i < groupList.Count; i++)
                    updatedPds.Add(ApplyProjectedMaterials(groupList[i], h1Rows[i], h2Rows[i], materialDetails, h1Period, h2Period));
                updatedProducers.Add(new L1Producer(l1.OrganisationId, updatedPds));

                allH2Rows.AddRange(h2WithGroupSubtotals);
                allH1Rows.AddRange(AddSubtotals<CalcResultH1ProjectedProducer>(
                    h1Rows,
                    createSubtotal: H1ProjectedProducersBuilderUtils.CreateParentProducer,
                    sumProducerGroupTonnages: H1ProjectedProducersBuilderUtils.SumProducerGroupTonnages
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
            string h2Period)
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
                    PackagingTypes.Household => projected.ProjectedHouseholdRAMTonnage,
                    PackagingTypes.PublicBin => projected.ProjectedPublicBinRAMTonnage,
                    PackagingTypes.HouseholdDrinksContainers => projected.ProjectedHouseholdDrinksContainerRAMTonnage,
                    _ => null
                };
                if (projectedRam == null) return rm;
                return new ProducerReportedMaterial
                {
                    Id = rm.Id,
                    MaterialId = rm.MaterialId,
                    ProducerDetailId = rm.ProducerDetailId,
                    PackagingType = rm.PackagingType,
                    PackagingTonnage = rm.PackagingTonnage,
                    PackagingTonnageRed = projectedRam.RedTonnage,
                    PackagingTonnageAmber = projectedRam.AmberTonnage,
                    PackagingTonnageGreen = projectedRam.GreenTonnage,
                    PackagingTonnageRedMedical = projectedRam.RedMedicalTonnage,
                    PackagingTonnageAmberMedical = projectedRam.AmberMedicalTonnage,
                    PackagingTonnageGreenMedical = projectedRam.GreenMedicalTonnage,
                    SubmissionPeriod = rm.SubmissionPeriod,
                    ProducerDetail = rm.ProducerDetail,
                    Material = rm.Material
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


        public static decimal TonnageWithoutRAM(decimal tonnage, RAMTonnage ramTonnage)
        {
            return Math.Max(0, tonnage - ramTonnage.TotalRamTonnage());
        }

        public static RAMTonnage SumRAMTonnages(List<ICalcResultProjectedProducer> producers, string materialCode, Func<CalcResultProjectedProducerMaterialTonnage, RAMTonnage?> getRAMTonnage)
        {
            decimal red = 0, redMed = 0, amber = 0, amberMed = 0, green = 0, greenMed = 0;

            foreach (var p in producers)
            {
                CalcResultProjectedProducerMaterialTonnage? material = p.ProjectedTonnageByMaterial.FirstOrDefault(v => v.Key == materialCode).Value;
                if (material == null) continue;

                var ram = getRAMTonnage(material);
                if (ram == null) continue;

                red += ram.RedTonnage;
                redMed += ram.RedMedicalTonnage;
                amber += ram.AmberTonnage;
                amberMed += ram.AmberMedicalTonnage;
                green += ram.GreenTonnage;
                greenMed += ram.GreenMedicalTonnage;
            }

            return new RAMTonnage
            {
                RedTonnage = red,
                RedMedicalTonnage = redMed,
                AmberTonnage = amber,
                AmberMedicalTonnage = amberMed,
                GreenTonnage = green,
                GreenMedicalTonnage = greenMed
            };
        }

        private static List<TSubmissionPeriodProducer> AddSubtotals<TSubmissionPeriodProducer>(
            List<TSubmissionPeriodProducer> projectedProducers,
            Func<TSubmissionPeriodProducer, TSubmissionPeriodProducer> createSubtotal,
            Func<List<TSubmissionPeriodProducer>, TSubmissionPeriodProducer> sumProducerGroupTonnages)
            where TSubmissionPeriodProducer : ICalcResultProjectedProducer
        {
            var result = new List<TSubmissionPeriodProducer>();
            var producerGroups = projectedProducers.GroupBy(p => p.ProducerId);

            foreach (var group in producerGroups)
            {
                if (group.Count() > 1)
                {
                    var updatedGroup = group.Select(p => p with { Level = CommonConstants.LevelTwo.ToString()});

                    result.AddRange(updatedGroup);
                    result.Add(
                        sumProducerGroupTonnages(group.ToList())
                    );
                }
                else
                {
                    var producer = group.First();

                    if (producer.SubsidiaryId != null)
                    {
                        var levelTwoProd = producer with { Level = CommonConstants.LevelTwo.ToString() };

                        var subtotal = createSubtotal(producer) with {
                            Level = CommonConstants.LevelOne.ToString(),
                            IsSubtotal = true,
                            SubsidiaryId = null
                        };

                        result.Add(subtotal);
                        result.Add(levelTwoProd);
                    }
                    else
                    {
                        var levelOneProd = producer with { Level = CommonConstants.LevelOne.ToString()};
                        result.Add(levelOneProd);
                    }
                }
            }

            return result;
        }
    }
}
