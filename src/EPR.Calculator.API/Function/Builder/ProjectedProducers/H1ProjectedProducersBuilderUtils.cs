using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.ProjectedProducers
{
    public static class H1ProjectedProducersBuilderUtils
    {
        public static List<CalcResultH1ProjectedProducer> GetProjectedProducers(List<ProducerDetail> producerDetails, List<CalcResultH2ProjectedProducer> h2ProjectedProducers, IReadOnlyCollection<MaterialDetail> materials, string submissionPeriod)
        {
            CalcResultH2ProjectedProducer? GetH2Producer(ProducerDetail rm)
            {
                var prodGroup = h2ProjectedProducers.Where(p => p.ProducerId == rm.ProducerId);
                var maybeSubtotal = prodGroup.Where(p => p.IsSubtotal);
                return maybeSubtotal.Any() ? maybeSubtotal.First() : prodGroup.FirstOrDefault(p => p.SubsidiaryId == rm.SubsidiaryId);
            }

            return producerDetails.Select(pd => new CalcResultH1ProjectedProducer
            {
                ProducerId = pd.ProducerId,
                SubsidiaryId = pd.SubsidiaryId,
                Level = string.Empty, // Level will be set later when subtotals are added
                SubmissionPeriodCode = submissionPeriod,
                H1ProjectedTonnageByMaterial = GetProjectedTonnages(
                    materials,
                    pd.ProducerReportedMaterials.Where(rm => rm.SubmissionPeriod == submissionPeriod).ToList(),
                    GetH2Producer(pd)
                )
            }).ToList();
        }

        private static Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage> GetProjectedTonnages(IReadOnlyCollection<MaterialDetail> materials, List<ProducerReportedMaterial> reportedMaterials, CalcResultH2ProjectedProducer? h2ProjectedProducer)
        {
            return materials.ToDictionary(m => m.Code, m => GetProjectedTonnage(m, reportedMaterials.Where(rm => rm.MaterialId == m.Id).ToList(), h2ProjectedProducer));
        }

        private static CalcResultH1ProjectedProducerMaterialTonnage GetProjectedTonnage(MaterialDetail material, List<ProducerReportedMaterial> reportedMaterials, CalcResultH2ProjectedProducer? h2ProjectedProducer)
        {
            if(!reportedMaterials.Any())
            {
                return GetEmptyH1MaterialTonnage(material.Code);
            }

            var hhTonnage = RAMTonnage.GetReportedTonnage(reportedMaterials, PackagingTypes.Household, rm => rm.PackagingTonnage);
            var householdRAMTonnage = RAMTonnage.ToRAMTonnage(PackagingTypes.Household, reportedMaterials);
            var pbTonnage = RAMTonnage.GetReportedTonnage(reportedMaterials, PackagingTypes.PublicBin, rm => rm.PackagingTonnage);
            var publicBinRAMTonnage = RAMTonnage.ToRAMTonnage(PackagingTypes.PublicBin, reportedMaterials);
            decimal? hdcTonnage = (material.Code == MaterialCodes.Glass) ? RAMTonnage.GetReportedTonnage(reportedMaterials, PackagingTypes.HouseholdDrinksContainers, rm => rm.PackagingTonnage) : null;
            var hdcRAMTonnage = (material.Code == MaterialCodes.Glass) ? RAMTonnage.ToRAMTonnage(PackagingTypes.HouseholdDrinksContainers, reportedMaterials) : null;

            var householdTonnageWithoutRAM = CalcResultProjectedProducersBuilder.TonnageWithoutRAM(hhTonnage, householdRAMTonnage);
            var publicBinTonnageWithoutRAM = CalcResultProjectedProducersBuilder.TonnageWithoutRAM(pbTonnage, publicBinRAMTonnage);
            decimal? householdDrinksContainerTonnageWithoutRAM = (hdcTonnage != null && hdcRAMTonnage != null) ? CalcResultProjectedProducersBuilder.TonnageWithoutRAM(hdcTonnage.Value, hdcRAMTonnage) : null;

            var h2ProjectedTonnage = h2ProjectedProducer != null ? h2ProjectedProducer.H2ProjectedTonnageByMaterial[material.Code] : GetEmptyH2MaterialTonnage(material.Code);
            var h2RamProportions = new RAMProportions{
                Red = GetH2RAMProportion(h2ProjectedTonnage.GetTotalProjectedRedTonnage(), h2ProjectedTonnage.TotalTonnage),
                Amber = GetH2RAMProportion(h2ProjectedTonnage.GetTotalProjectedAmberTonnage(), h2ProjectedTonnage.TotalTonnage),
                Green = GetH2RAMProportion(h2ProjectedTonnage.GetTotalProjectedGreenTonnage(), h2ProjectedTonnage.TotalTonnage),
                RedMedical = GetH2RAMProportion(h2ProjectedTonnage.GetTotalProjectedRedMedicalTonnage(), h2ProjectedTonnage.TotalTonnage),
                AmberMedical = GetH2RAMProportion(h2ProjectedTonnage.GetTotalProjectedAmberMedicalTonnage(), h2ProjectedTonnage.TotalTonnage),
                GreenMedical = GetH2RAMProportion(h2ProjectedTonnage.GetTotalProjectedGreenMedicalTonnage(), h2ProjectedTonnage.TotalTonnage)
            };

            var h1ProportionateRAMTonnage = (decimal tonnage, RAMTonnage ramTonnage, decimal tonnageWithoutRAM, decimal h2TotalTonnage)
                => GetProjectedTonnage(tonnage, ramTonnage, tonnageWithoutRAM, h2RamProportions, h2TotalTonnage);

            return new CalcResultH1ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = hhTonnage,
                HouseholdRAMTonnage = householdRAMTonnage,
                PublicBinTonnage = pbTonnage,
                PublicBinRAMTonnage = publicBinRAMTonnage,
                HouseholdDrinksContainerTonnage = hdcTonnage,
                HouseholdDrinksContainerRAMTonnage = hdcRAMTonnage,
                HouseholdTonnageWithoutRAM = householdTonnageWithoutRAM,
                PublicBinTonnageWithoutRAM = publicBinTonnageWithoutRAM,
                HouseholdDrinksContainerTonnageWithoutRAM = householdDrinksContainerTonnageWithoutRAM,
                H2RamProportions = h2RamProportions,
                TotalTonnage = hhTonnage + pbTonnage + (hdcTonnage ?? 0),
                ProjectedHouseholdTonnage = hhTonnage,
                ProjectedHouseholdRAMTonnage = h1ProportionateRAMTonnage(hhTonnage, householdRAMTonnage, householdTonnageWithoutRAM, h2ProjectedTonnage.TotalTonnage),
                ProjectedPublicBinTonnage = pbTonnage,
                ProjectedPublicBinRAMTonnage = h1ProportionateRAMTonnage(pbTonnage, publicBinRAMTonnage, publicBinTonnageWithoutRAM, h2ProjectedTonnage.TotalTonnage),
                ProjectedHouseholdDrinksContainerTonnage = hdcTonnage,
                ProjectedHouseholdDrinksContainerRAMTonnage = (hdcTonnage != null && hdcRAMTonnage != null) ? h1ProportionateRAMTonnage(hdcTonnage.Value, hdcRAMTonnage, householdDrinksContainerTonnageWithoutRAM!.Value, h2ProjectedTonnage.TotalTonnage) : null
            };
        }

        private static decimal GetH2RAMProportion(decimal totalH2MatTonnage, decimal totalH2Tonnage)
        {
            if(totalH2Tonnage <= 0) return 0;

            return Math.Round(totalH2MatTonnage / totalH2Tonnage, 6);
        }

        public static RAMTonnage GetProportionateRam(RAMTonnage h1RAMTonnage, decimal tonnageWithoutRAM, RAMProportions h2RamProportions)
        {
            return h1RAMTonnage with {
                RedTonnage = Math.Round(h1RAMTonnage.RedTonnage + (tonnageWithoutRAM * h2RamProportions.Red), 3),
                AmberTonnage = Math.Round(h1RAMTonnage.AmberTonnage + (tonnageWithoutRAM * h2RamProportions.Amber), 3),
                GreenTonnage = Math.Round(h1RAMTonnage.GreenTonnage + (tonnageWithoutRAM * h2RamProportions.Green), 3),
                RedMedicalTonnage = Math.Round(h1RAMTonnage.RedMedicalTonnage + (tonnageWithoutRAM * h2RamProportions.RedMedical), 3),
                AmberMedicalTonnage = Math.Round(h1RAMTonnage.AmberMedicalTonnage + (tonnageWithoutRAM * h2RamProportions.AmberMedical), 3),
                GreenMedicalTonnage = Math.Round(h1RAMTonnage.GreenMedicalTonnage + (tonnageWithoutRAM * h2RamProportions.GreenMedical), 3)
            };
        }

        private static RAMTonnage GetProjectedTonnage(decimal tonnage, RAMTonnage h1RAMTonnage, decimal tonnageWithoutRAM, RAMProportions h2RamProportions, decimal h2TotalTonnage)
        {
            if (h2TotalTonnage > 0) {
                var projectedTonnage = GetProportionateRam(h1RAMTonnage, tonnageWithoutRAM, h2RamProportions);
                return ReconcileRoundingDifference(tonnage, projectedTonnage);
            }
            else
            {
                return h1RAMTonnage with { RedTonnage = h1RAMTonnage.RedTonnage + tonnageWithoutRAM };
            }
        }

        public static RAMTonnage ReconcileRoundingDifference(decimal tonnage, RAMTonnage projectedTonnage)
        {
            var diffTonnage = tonnage - projectedTonnage.TotalRamTonnage();
            if (diffTonnage != 0)
            {
                var dominantRamTonnage = new[]
                {
                    (Priority: 1, Tonnage: projectedTonnage.AmberTonnage, Apply: (Func<RAMTonnage, RAMTonnage>)(t => t with { AmberTonnage = t.AmberTonnage + diffTonnage })),
                    (Priority: 2, Tonnage: projectedTonnage.AmberMedicalTonnage, Apply: t => t with { AmberMedicalTonnage = t.AmberMedicalTonnage + diffTonnage }),
                    (Priority: 3, Tonnage: projectedTonnage.RedTonnage, Apply: t => t with { RedTonnage = t.RedTonnage + diffTonnage }),
                    (Priority: 4, Tonnage: projectedTonnage.RedMedicalTonnage, Apply: t => t with { RedMedicalTonnage = t.RedMedicalTonnage + diffTonnage }),
                    (Priority: 5, Tonnage: projectedTonnage.GreenTonnage, Apply: t => t with { GreenTonnage = t.GreenTonnage + diffTonnage }),
                    (Priority: 6, Tonnage: projectedTonnage.GreenMedicalTonnage, Apply: t => t with { GreenMedicalTonnage = t.GreenMedicalTonnage + diffTonnage }),
                }
                .OrderByDescending(kv => kv.Tonnage)
                .ThenBy(kv => kv.Priority)
                .First();

                return dominantRamTonnage.Apply(projectedTonnage);
            }

            return projectedTonnage;
        }

        public static CalcResultH1ProjectedProducer CreateParentProducer(CalcResultH1ProjectedProducer p)
        {
            return new CalcResultH1ProjectedProducer
                {
                    ProducerId = p.ProducerId,
                    SubsidiaryId = null,
                    Level = CommonConstants.LevelOne.ToString(),
                    SubmissionPeriodCode = p.SubmissionPeriodCode,
                    IsSubtotal = true,
                    H1ProjectedTonnageByMaterial = p.H1ProjectedTonnageByMaterial
                };
        }

        public static CalcResultH1ProjectedProducer SumProducerGroupTonnages(List<CalcResultH1ProjectedProducer> prodGroup)
        {
            var producer = prodGroup.First();
            var sumRam = (string matKey, Func<CalcResultProjectedProducerMaterialTonnage, RAMTonnage?> tonnageFunc) =>
                CalcResultProjectedProducersBuilder.SumRAMTonnages(prodGroup.Cast<ICalcResultProjectedProducer>().ToList(), matKey, tonnageFunc);

            return new CalcResultH1ProjectedProducer
            {
                ProducerId = producer.ProducerId,
                SubsidiaryId = null,
                Level = CommonConstants.LevelOne.ToString(),
                SubmissionPeriodCode = producer.SubmissionPeriodCode,
                IsSubtotal = true,
                H1ProjectedTonnageByMaterial = producer.H1ProjectedTonnageByMaterial.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new CalcResultH1ProjectedProducerMaterialTonnage {
                        HouseholdTonnage = prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].HouseholdTonnage),
                        HouseholdRAMTonnage = sumRam(kvp.Key, p => p.HouseholdRAMTonnage),
                        PublicBinTonnage = prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].PublicBinTonnage),
                        PublicBinRAMTonnage = sumRam(kvp.Key, p => p.PublicBinRAMTonnage),
                        HouseholdDrinksContainerTonnage = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].HouseholdDrinksContainerTonnage) : null,
                        HouseholdDrinksContainerRAMTonnage = kvp.Key == MaterialCodes.Glass ? sumRam(kvp.Key, p => p.HouseholdDrinksContainerRAMTonnage) : null,
                        HouseholdTonnageWithoutRAM = prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].HouseholdTonnageWithoutRAM),
                        PublicBinTonnageWithoutRAM = prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].PublicBinTonnageWithoutRAM),
                        HouseholdDrinksContainerTonnageWithoutRAM = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].HouseholdDrinksContainerTonnageWithoutRAM ?? 0) : null,
                        H2RamProportions = producer.H1ProjectedTonnageByMaterial[kvp.Key].H2RamProportions ?? new RAMProportions(),
                        TotalTonnage = prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].TotalTonnage),
                        ProjectedHouseholdTonnage = prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].ProjectedHouseholdTonnage),
                        ProjectedHouseholdRAMTonnage = sumRam(kvp.Key, p => p.ProjectedHouseholdRAMTonnage),
                        ProjectedPublicBinTonnage = prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].ProjectedPublicBinTonnage),
                        ProjectedPublicBinRAMTonnage = sumRam(kvp.Key, p => p.ProjectedPublicBinRAMTonnage),
                        ProjectedHouseholdDrinksContainerTonnage = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].ProjectedHouseholdDrinksContainerTonnage) : null,
                        ProjectedHouseholdDrinksContainerRAMTonnage = kvp.Key == MaterialCodes.Glass ? sumRam(kvp.Key, p => p.ProjectedHouseholdDrinksContainerRAMTonnage) : null
                    })
            };
        }

        private static CalcResultH1ProjectedProducerMaterialTonnage GetEmptyH1MaterialTonnage(string materialCode) {
            return new CalcResultH1ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = 0,
                HouseholdRAMTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                PublicBinTonnage = 0,
                PublicBinRAMTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                HouseholdDrinksContainerTonnage = materialCode == MaterialCodes.Glass ? 0 : null,
                HouseholdDrinksContainerRAMTonnage = materialCode == MaterialCodes.Glass ?
                    new RAMTonnage { RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 } : null,
                HouseholdTonnageWithoutRAM = 0,
                PublicBinTonnageWithoutRAM = 0,
                HouseholdDrinksContainerTonnageWithoutRAM = materialCode == MaterialCodes.Glass ? 0 : null,
                H2RamProportions = new RAMProportions {
                    Red = 0, Amber = 0, Green = 0,
                    RedMedical = 0, AmberMedical = 0, GreenMedical = 0
                },
                TotalTonnage = 0,
                ProjectedHouseholdTonnage = 0,
                ProjectedHouseholdRAMTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                ProjectedPublicBinTonnage = 0,
                ProjectedPublicBinRAMTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                ProjectedHouseholdDrinksContainerTonnage = materialCode == MaterialCodes.Glass ? 0 : null,
                ProjectedHouseholdDrinksContainerRAMTonnage = materialCode == MaterialCodes.Glass ?
                    new RAMTonnage { RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 } : null
            };
        }

        private static CalcResultH2ProjectedProducerMaterialTonnage GetEmptyH2MaterialTonnage(string materialCode) {
            return new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = 0,
                HouseholdRAMTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                PublicBinTonnage = 0,
                PublicBinRAMTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                HouseholdDrinksContainerTonnage = materialCode == MaterialCodes.Glass ? 0 : null,
                HouseholdDrinksContainerRAMTonnage = materialCode == MaterialCodes.Glass ?
                    new RAMTonnage { RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 } : null,
                HouseholdTonnageWithoutRAM = 0,
                PublicBinTonnageWithoutRAM = 0,
                HouseholdDrinksContainerTonnageWithoutRAM = materialCode == MaterialCodes.Glass ? 0 : null,
                ProjectedHouseholdTonnage = 0,
                ProjectedHouseholdRAMTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                ProjectedPublicBinTonnage = 0,
                ProjectedPublicBinRAMTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                ProjectedHouseholdDrinksContainerTonnage = materialCode == MaterialCodes.Glass ? 0 : null,
                ProjectedHouseholdDrinksContainerRAMTonnage = materialCode == MaterialCodes.Glass ?
                    new RAMTonnage { RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 } : null,
                TotalTonnage = 0
            };
        }
    }
}
