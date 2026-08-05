using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Builder.ProjectedProducers
{
    public static class H1ProjectedProducersBuilderUtils
    {
        public static List<CalcResultH1ProjectedProducer> GetProjectedProducers(List<ProducerDetail> producerDetails, List<CalcResultH2ProjectedProducer> h2ProjectedProducers, IReadOnlyCollection<MaterialDetail> materials, string submissionPeriod)
        {
            CalcResultH2ProjectedProducer? GetH2Producer(ProducerDetail rm)
            {
                var prodGroup = h2ProjectedProducers.Where(p => p.ProducerId == rm.ProducerId);
                return prodGroup.FirstOrDefault(p => p.IsSubtotal) ?? prodGroup.FirstOrDefault(p => p.SubsidiaryId == rm.SubsidiaryId);
            }

            return producerDetails.Select(pd => new CalcResultH1ProjectedProducer
            {
                ProducerId                   = pd.ProducerId,
                SubsidiaryId                 = pd.SubsidiaryId,
                Level                        = string.Empty, // Level will be set later when subtotals are added
                SubmissionPeriodCode         = submissionPeriod,
                H1ProjectedTonnageByMaterial = GetProjectedTonnages(
                    materials,
                    pd.ProducerReportedMaterials.Where(rm => rm.SubmissionPeriod == submissionPeriod).ToList(),
                    GetH2Producer(pd)
                )
            }).ToList();
        }

        private static Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage> GetProjectedTonnages(IReadOnlyCollection<MaterialDetail> materials, List<ProducerReportedMaterial> reportedMaterials, CalcResultH2ProjectedProducer? h2ProjectedProducer) =>
            materials.ToDictionary(m => m.Code, m => GetProjectedTonnage(m, reportedMaterials.Where(rm => rm.MaterialId == m.Id).ToList(), h2ProjectedProducer));

        private static CalcResultH1ProjectedProducerMaterialTonnage GetProjectedTonnage(MaterialDetail material, List<ProducerReportedMaterial> reportedMaterials, CalcResultH2ProjectedProducer? h2ProjectedProducer)
        {
            if (!reportedMaterials.Any())
            {
                return GetEmptyH1MaterialTonnage(material.Code);
            }

            var hhTonnage = RamTonnage.GetReportedTonnage(reportedMaterials, PackagingTypes.Household, rm => rm.PackagingTonnage);
            var hhRAMTonnage = RamTonnage.ToRamTonnage(reportedMaterials, PackagingTypes.Household);
            var pbTonnage = RamTonnage.GetReportedTonnage(reportedMaterials, PackagingTypes.PublicBin, rm => rm.PackagingTonnage);
            var pbRAMTonnage = RamTonnage.ToRamTonnage(reportedMaterials, PackagingTypes.PublicBin);
            decimal? hdcTonnage = (material.Code == MaterialCodes.Glass) ? RamTonnage.GetReportedTonnage(reportedMaterials, PackagingTypes.HouseholdDrinksContainers, rm => rm.PackagingTonnage) : null;
            var hdcRAMTonnage = (material.Code == MaterialCodes.Glass) ? RamTonnage.ToRamTonnage(reportedMaterials, PackagingTypes.HouseholdDrinksContainers) : null;

            var hhTonnageWithoutRAM = CalcResultProjectedProducersBuilder.TonnageWithoutRAM(hhTonnage, hhRAMTonnage);
            var pbTonnageWithoutRAM = CalcResultProjectedProducersBuilder.TonnageWithoutRAM(pbTonnage, pbRAMTonnage);
            decimal? hdcTonnageWithoutRAM = (hdcTonnage != null && hdcRAMTonnage != null) ? CalcResultProjectedProducersBuilder.TonnageWithoutRAM(hdcTonnage.Value, hdcRAMTonnage) : null;

            var h2ProjectedTotalTonnage = h2ProjectedProducer?.H2ProjectedTonnageByMaterial[material.Code].TotalTonnage() ?? 0;
            var h2RamProportions = ComputeProportionsFromH2(h2ProjectedProducer, material.Code);

            var h1ProportionateRAMTonnage = (decimal tonnage, RamTonnage ramTonnage, decimal tonnageWithoutRAM, decimal h2TotalTonnage)
                => GetProjectedTonnage(tonnage, ramTonnage, tonnageWithoutRAM, h2RamProportions, h2TotalTonnage);

            return new CalcResultH1ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage                            = hhTonnage,
                HouseholdRAMTonnage                         = hhRAMTonnage,
                PublicBinTonnage                            = pbTonnage,
                PublicBinRAMTonnage                         = pbRAMTonnage,
                HouseholdDrinksContainerTonnage             = hdcTonnage,
                HouseholdDrinksContainerRAMTonnage          = hdcRAMTonnage,
                HouseholdTonnageWithoutRAM                  = hhTonnageWithoutRAM,
                PublicBinTonnageWithoutRAM                  = pbTonnageWithoutRAM,
                HouseholdDrinksContainerTonnageWithoutRAM   = hdcTonnageWithoutRAM,
                H2RamProportions                            = RAMProportions.Empty,
                ProjectedHouseholdTonnage                   = hhTonnage,
                ProjectedHouseholdRAMTonnage                = h1ProportionateRAMTonnage(hhTonnage, hhRAMTonnage, hhTonnageWithoutRAM, h2ProjectedTotalTonnage),
                ProjectedPublicBinTonnage                   = pbTonnage,
                ProjectedPublicBinRAMTonnage                = h1ProportionateRAMTonnage(pbTonnage, pbRAMTonnage, pbTonnageWithoutRAM, h2ProjectedTotalTonnage),
                ProjectedHouseholdDrinksContainerTonnage    = hdcTonnage,
                ProjectedHouseholdDrinksContainerRAMTonnage = (hdcTonnage != null && hdcRAMTonnage != null) ? h1ProportionateRAMTonnage(hdcTonnage.Value, hdcRAMTonnage, hdcTonnageWithoutRAM!.Value, h2ProjectedTotalTonnage) : null
            };
        }

        private static RAMProportions ComputeProportionsFromH2(CalcResultH2ProjectedProducer? h2Producer, string materialCode)
        {
            var t = h2Producer != null ? h2Producer.H2ProjectedTonnageByMaterial[materialCode] : GetEmptyH2MaterialTonnage(materialCode);
            var total = t.TotalTonnage();
            return new RAMProportions
            {
                Red          = GetH2RAMProportion(t.GetTotalProjectedRedTonnage()         , total),
                Amber        = GetH2RAMProportion(t.GetTotalProjectedAmberTonnage()       , total),
                Green        = GetH2RAMProportion(t.GetTotalProjectedGreenTonnage()       , total),
                RedMedical   = GetH2RAMProportion(t.GetTotalProjectedRedMedicalTonnage()  , total),
                AmberMedical = GetH2RAMProportion(t.GetTotalProjectedAmberMedicalTonnage(), total),
                GreenMedical = GetH2RAMProportion(t.GetTotalProjectedGreenMedicalTonnage(), total)
            };
        }

        private static decimal GetH2RAMProportion(decimal totalH2MatTonnage, decimal totalH2Tonnage)
        {
            if(totalH2Tonnage <= 0) return 0;

            return MathUtils.RoundAwayFromZero(totalH2MatTonnage / totalH2Tonnage, 6);
        }

        public static RamTonnage GetProportionateRam(RamTonnage h1RAMTonnage, decimal tonnageWithoutRAM, RAMProportions h2RamProportions) =>
            new RamTonnage
            {
                Red          = MathUtils.RoundAwayFromZero(h1RAMTonnage.Red          + (tonnageWithoutRAM * h2RamProportions.Red         ), 3),
                Amber        = MathUtils.RoundAwayFromZero(h1RAMTonnage.Amber        + (tonnageWithoutRAM * h2RamProportions.Amber       ), 3),
                Green        = MathUtils.RoundAwayFromZero(h1RAMTonnage.Green        + (tonnageWithoutRAM * h2RamProportions.Green       ), 3),
                RedMedical   = MathUtils.RoundAwayFromZero(h1RAMTonnage.RedMedical   + (tonnageWithoutRAM * h2RamProportions.RedMedical  ), 3),
                AmberMedical = MathUtils.RoundAwayFromZero(h1RAMTonnage.AmberMedical + (tonnageWithoutRAM * h2RamProportions.AmberMedical), 3),
                GreenMedical = MathUtils.RoundAwayFromZero(h1RAMTonnage.GreenMedical + (tonnageWithoutRAM * h2RamProportions.GreenMedical), 3)
            };

        private static RamTonnage GetProjectedTonnage(decimal tonnage, RamTonnage h1RAMTonnage, decimal tonnageWithoutRAM, RAMProportions h2RamProportions, decimal h2TotalTonnage)
        {
            if (h2TotalTonnage > 0) {
                var projectedTonnage = GetProportionateRam(h1RAMTonnage, tonnageWithoutRAM, h2RamProportions);
                return ReconcileRoundingDifference(tonnage, projectedTonnage);
            }
            else
            {
                return h1RAMTonnage with { Red = h1RAMTonnage.Red + tonnageWithoutRAM };
            }
        }

        public static RamTonnage ReconcileRoundingDifference(decimal tonnage, RamTonnage projectedTonnage)
        {
            var diffTonnage = tonnage - projectedTonnage.TotalRamTonnage();
            if (diffTonnage != 0)
            {
                var dominantRamTonnage = new[]
                {
                    (Priority: 1, Tonnage: projectedTonnage.Amber       , Apply: (Func<RamTonnage, RamTonnage>)(t => t with { Amber = t.Amber + diffTonnage })),
                    (Priority: 2, Tonnage: projectedTonnage.AmberMedical, Apply: t => t with { AmberMedical = t.AmberMedical + diffTonnage }),
                    (Priority: 3, Tonnage: projectedTonnage.Red         , Apply: t => t with { Red          = t.Red + diffTonnage }),
                    (Priority: 4, Tonnage: projectedTonnage.RedMedical  , Apply: t => t with { RedMedical   = t.RedMedical + diffTonnage }),
                    (Priority: 5, Tonnage: projectedTonnage.Green       , Apply: t => t with { Green        = t.Green + diffTonnage }),
                    (Priority: 6, Tonnage: projectedTonnage.GreenMedical, Apply: t => t with { GreenMedical = t.GreenMedical + diffTonnage }),
                }
                .OrderByDescending(kv => kv.Tonnage)
                .ThenBy(kv => kv.Priority)
                .First();

                return dominantRamTonnage.Apply(projectedTonnage);
            }

            return projectedTonnage;
        }

        public static CalcResultH1ProjectedProducer CreateParentProducer(CalcResultH1ProjectedProducer p, List<CalcResultH2ProjectedProducer> h2Producers)
        {
            var h2Subtotal = h2Producers.FirstOrDefault(h2 => h2.IsSubtotal) ?? h2Producers.FirstOrDefault();
            return new CalcResultH1ProjectedProducer
                {
                    ProducerId                   = p.ProducerId,
                    SubsidiaryId                 = null,
                    Level                        = CommonConstants.LevelOne.ToString(),
                    SubmissionPeriodCode         = p.SubmissionPeriodCode,
                    IsSubtotal                   = true,
                    H1ProjectedTonnageByMaterial = p.H1ProjectedTonnageByMaterial.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value with { H2RamProportions = ComputeProportionsFromH2(h2Subtotal, kvp.Key) })
                };
        }

        public static CalcResultH1ProjectedProducer SumProducerGroupTonnages(List<CalcResultH1ProjectedProducer> prodGroup, List<CalcResultH2ProjectedProducer> h2Producers)
        {
            var producer = prodGroup.First();
            var h2Subtotal = h2Producers.FirstOrDefault(h2 => h2.IsSubtotal);
            var sumRam = (string matKey, Func<CalcResultProjectedProducerMaterialTonnage, RamTonnage?> tonnageFunc) =>
                CalcResultProjectedProducersBuilder.SumRAMTonnages(prodGroup.Cast<ICalcResultProjectedProducer>().ToList(), matKey, tonnageFunc);

            return new CalcResultH1ProjectedProducer
            {
                ProducerId                   = producer.ProducerId,
                SubsidiaryId                 = null,
                Level                        = CommonConstants.LevelOne.ToString(),
                SubmissionPeriodCode         = producer.SubmissionPeriodCode,
                IsSubtotal                   = true,
                H1ProjectedTonnageByMaterial = producer.H1ProjectedTonnageByMaterial.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new CalcResultH1ProjectedProducerMaterialTonnage {
                        HouseholdTonnage                            = prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].HouseholdTonnage),
                        HouseholdRAMTonnage                         = sumRam(kvp.Key, p => p.HouseholdRAMTonnage),
                        PublicBinTonnage                            = prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].PublicBinTonnage),
                        PublicBinRAMTonnage                         = sumRam(kvp.Key, p => p.PublicBinRAMTonnage),
                        HouseholdDrinksContainerTonnage             = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].HouseholdDrinksContainerTonnage) : null,
                        HouseholdDrinksContainerRAMTonnage          = kvp.Key == MaterialCodes.Glass ? sumRam(kvp.Key, p => p.HouseholdDrinksContainerRAMTonnage) : null,
                        HouseholdTonnageWithoutRAM                  = prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].HouseholdTonnageWithoutRAM),
                        PublicBinTonnageWithoutRAM                  = prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].PublicBinTonnageWithoutRAM),
                        HouseholdDrinksContainerTonnageWithoutRAM   = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].HouseholdDrinksContainerTonnageWithoutRAM ?? 0) : null,
                        H2RamProportions                            = ComputeProportionsFromH2(h2Subtotal, kvp.Key),
                        ProjectedHouseholdTonnage                   = prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].ProjectedHouseholdTonnage),
                        ProjectedHouseholdRAMTonnage                = sumRam(kvp.Key, p => p.ProjectedHouseholdRAMTonnage),
                        ProjectedPublicBinTonnage                   = prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].ProjectedPublicBinTonnage),
                        ProjectedPublicBinRAMTonnage                = sumRam(kvp.Key, p => p.ProjectedPublicBinRAMTonnage),
                        ProjectedHouseholdDrinksContainerTonnage    = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].ProjectedHouseholdDrinksContainerTonnage) : null,
                        ProjectedHouseholdDrinksContainerRAMTonnage = kvp.Key == MaterialCodes.Glass ? sumRam(kvp.Key, p => p.ProjectedHouseholdDrinksContainerRAMTonnage) : null
                    })
            };
        }

        private static CalcResultH1ProjectedProducerMaterialTonnage GetEmptyH1MaterialTonnage(string materialCode) =>
            new CalcResultH1ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage                            = 0,
                HouseholdRAMTonnage                         = RamTonnage.Empty,
                PublicBinTonnage                            = 0,
                PublicBinRAMTonnage                         = RamTonnage.Empty,
                HouseholdDrinksContainerTonnage             = materialCode == MaterialCodes.Glass ? 0 : null,
                HouseholdDrinksContainerRAMTonnage          = materialCode == MaterialCodes.Glass ? RamTonnage.Empty : null,
                HouseholdTonnageWithoutRAM                  = 0,
                PublicBinTonnageWithoutRAM                  = 0,
                HouseholdDrinksContainerTonnageWithoutRAM   = materialCode == MaterialCodes.Glass ? 0 : null,
                H2RamProportions                            = RAMProportions.Empty,
                ProjectedHouseholdTonnage                   = 0,
                ProjectedHouseholdRAMTonnage                = RamTonnage.Empty,
                ProjectedPublicBinTonnage                   = 0,
                ProjectedPublicBinRAMTonnage                = RamTonnage.Empty,
                ProjectedHouseholdDrinksContainerTonnage    = materialCode == MaterialCodes.Glass ? 0 : null,
                ProjectedHouseholdDrinksContainerRAMTonnage = materialCode == MaterialCodes.Glass ? RamTonnage.Empty : null
            };

        private static CalcResultH2ProjectedProducerMaterialTonnage GetEmptyH2MaterialTonnage(string materialCode) =>
            new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage                            = 0,
                HouseholdRAMTonnage                         = RamTonnage.Empty,
                PublicBinTonnage                            = 0,
                PublicBinRAMTonnage                         = RamTonnage.Empty,
                HouseholdDrinksContainerTonnage             = materialCode == MaterialCodes.Glass ? 0 : null,
                HouseholdDrinksContainerRAMTonnage          = materialCode == MaterialCodes.Glass ? RamTonnage.Empty : null,
                HouseholdTonnageWithoutRAM                  = 0,
                PublicBinTonnageWithoutRAM                  = 0,
                HouseholdDrinksContainerTonnageWithoutRAM   = materialCode == MaterialCodes.Glass ? 0 : null,
                ProjectedHouseholdTonnage                   = 0,
                ProjectedHouseholdRAMTonnage                = RamTonnage.Empty,
                ProjectedPublicBinTonnage                   = 0,
                ProjectedPublicBinRAMTonnage                = RamTonnage.Empty,
                ProjectedHouseholdDrinksContainerTonnage    = materialCode == MaterialCodes.Glass ? 0 : null,
                ProjectedHouseholdDrinksContainerRAMTonnage = materialCode == MaterialCodes.Glass ? RamTonnage.Empty : null
            };
    }
}
