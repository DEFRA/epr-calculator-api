using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.ProjectedProducers
{
    public static class H2ProjectedProducersBuilderUtils
    {
        public static List<CalcResultH2ProjectedProducer> GetProjectedProducers(List<ProducerDetail> producerDetails, IImmutableList<MaterialDetail> materials, string submissionPeriod) =>
            producerDetails.Select(pd => new CalcResultH2ProjectedProducer
            {
                ProducerId                   = pd.ProducerId,
                SubsidiaryId                 = pd.SubsidiaryId,
                Level                        = string.Empty,  // Level will be set later when subtotals are added
                SubmissionPeriodCode         = submissionPeriod,
                H2ProjectedTonnageByMaterial = GetProjectedTonnages(
                    materials,
                    pd.ProducerReportedMaterials.Where(rm => rm.SubmissionPeriod == submissionPeriod).ToList()
                )
            }).ToList();

        private static Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage> GetProjectedTonnages(IImmutableList<MaterialDetail> materials, List<ProducerReportedMaterial> reportedMaterials) =>
            materials.ToDictionary(m => m.Code, m => GetProjectedTonnage(m, reportedMaterials.Where(rm => rm.MaterialId == m.Id).ToList()));

        private static CalcResultH2ProjectedProducerMaterialTonnage GetProjectedTonnage(MaterialDetail material, List<ProducerReportedMaterial> reportedMaterials)
        {
            var hhTonnage = RamTonnage.GetReportedTonnage(reportedMaterials, PackagingTypes.Household, rm => rm.PackagingTonnage);
            var hhRAMTonnage = RamTonnage.ToRamTonnage(reportedMaterials, PackagingTypes.Household);
            var pbTonnage = RamTonnage.GetReportedTonnage(reportedMaterials, PackagingTypes.PublicBin, rm => rm.PackagingTonnage);
            var pbRAMTonnage = RamTonnage.ToRamTonnage(reportedMaterials, PackagingTypes.PublicBin);
            var hdcTonnage = (material.Code == MaterialCodes.Glass) ? RamTonnage.GetReportedTonnage(reportedMaterials, PackagingTypes.HouseholdDrinksContainers, rm => rm.PackagingTonnage) : (decimal?) null;
            var hdcRAMTonnage = (material.Code == MaterialCodes.Glass) ? RamTonnage.ToRamTonnage(reportedMaterials, PackagingTypes.HouseholdDrinksContainers) : null;
            var hhWithoutRam = CalcResultProjectedProducersBuilder.TonnageWithoutRAM(hhTonnage, hhRAMTonnage);
            var pbWithoutRam = CalcResultProjectedProducersBuilder.TonnageWithoutRAM(pbTonnage, pbRAMTonnage);
            decimal? hdcWithoutRam = (hdcTonnage != null && hdcRAMTonnage != null) ? CalcResultProjectedProducersBuilder.TonnageWithoutRAM(hdcTonnage.Value, hdcRAMTonnage) : null;

            return new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage                            = hhTonnage,
                HouseholdRAMTonnage                         = hhRAMTonnage,
                PublicBinTonnage                            = pbTonnage,
                PublicBinRAMTonnage                         = pbRAMTonnage,
                HouseholdDrinksContainerTonnage             = hdcTonnage,
                HouseholdDrinksContainerRAMTonnage          = hdcRAMTonnage,
                HouseholdTonnageWithoutRAM                  = hhWithoutRam,
                PublicBinTonnageWithoutRAM                  = pbWithoutRam,
                HouseholdDrinksContainerTonnageWithoutRAM   = hdcWithoutRam,
                ProjectedHouseholdTonnage                   = hhTonnage,
                ProjectedHouseholdRAMTonnage                = hhRAMTonnage with { Red = hhRAMTonnage.Red + hhWithoutRam },
                ProjectedPublicBinTonnage                   = pbTonnage,
                ProjectedPublicBinRAMTonnage                = pbRAMTonnage with { Red = pbRAMTonnage.Red + pbWithoutRam },
                ProjectedHouseholdDrinksContainerTonnage    = hdcTonnage,
                ProjectedHouseholdDrinksContainerRAMTonnage = hdcRAMTonnage != null ? hdcRAMTonnage with { Red = hdcRAMTonnage.Red + hdcWithoutRam!.Value } : null
            };
        }

        public static CalcResultH2ProjectedProducer CreateParentProducer(CalcResultH2ProjectedProducer p) =>
            new CalcResultH2ProjectedProducer()
            {
                ProducerId                   = p.ProducerId,
                SubsidiaryId                 = null,
                Level                        = CommonConstants.LevelOne.ToString(),
                SubmissionPeriodCode         = p.SubmissionPeriodCode,
                IsSubtotal                   = true,
                H2ProjectedTonnageByMaterial = p.H2ProjectedTonnageByMaterial
            };

        public static CalcResultH2ProjectedProducer SumProducerGroupTonnages(List<CalcResultH2ProjectedProducer> prodGroup)
        {
            var producer = prodGroup.First();
            var sumRam = (string matKey, Func<CalcResultProjectedProducerMaterialTonnage, RamTonnage?> tonnageFunc) =>
                CalcResultProjectedProducersBuilder.SumRAMTonnages(prodGroup.Cast<ICalcResultProjectedProducer>().ToList(), matKey, tonnageFunc);

            return new CalcResultH2ProjectedProducer
            {
                ProducerId                   = producer.ProducerId,
                SubsidiaryId                 = null,
                Level                        = CommonConstants.LevelOne.ToString(),
                SubmissionPeriodCode         = producer.SubmissionPeriodCode,
                IsSubtotal                   = true,
                H2ProjectedTonnageByMaterial = producer.H2ProjectedTonnageByMaterial.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new CalcResultH2ProjectedProducerMaterialTonnage {
                        HouseholdTonnage                            = prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].HouseholdTonnage),
                        HouseholdRAMTonnage                         = sumRam(kvp.Key, p => p.HouseholdRAMTonnage),
                        PublicBinTonnage                            = prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].PublicBinTonnage),
                        PublicBinRAMTonnage                         = sumRam(kvp.Key, p => p.PublicBinRAMTonnage),
                        HouseholdDrinksContainerTonnage             = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].HouseholdDrinksContainerTonnage ?? 0) : null,
                        HouseholdDrinksContainerRAMTonnage          = kvp.Key == MaterialCodes.Glass ? sumRam(kvp.Key, p => p.HouseholdDrinksContainerRAMTonnage) : null,
                        HouseholdTonnageWithoutRAM                  = prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].HouseholdTonnageWithoutRAM),
                        PublicBinTonnageWithoutRAM                  = prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].PublicBinTonnageWithoutRAM),
                        HouseholdDrinksContainerTonnageWithoutRAM   = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].HouseholdDrinksContainerTonnageWithoutRAM ?? 0) : null,
                        ProjectedHouseholdTonnage                   = prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].ProjectedHouseholdTonnage),
                        ProjectedHouseholdRAMTonnage                = sumRam(kvp.Key, p => p.ProjectedHouseholdRAMTonnage),
                        ProjectedPublicBinTonnage                   = prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].ProjectedPublicBinTonnage),
                        ProjectedPublicBinRAMTonnage                = sumRam(kvp.Key, p => p.ProjectedPublicBinRAMTonnage),
                        ProjectedHouseholdDrinksContainerTonnage    = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].ProjectedHouseholdDrinksContainerTonnage ?? 0) : null,
                        ProjectedHouseholdDrinksContainerRAMTonnage = kvp.Key == MaterialCodes.Glass ? sumRam(kvp.Key, p => p.ProjectedHouseholdDrinksContainerRAMTonnage) : null
                    })
            };
        }
    }
}
