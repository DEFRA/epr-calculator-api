using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.PartialObligations
{
    public interface ICalcResultPartialObligationBuilder
    {
        Task<(List<L1Producer>, CalcResultPartialObligations)> ConstructAsync(
            RunContext runContext,
            IImmutableList<MaterialDetail> materialDetails,
            List<L1Producer> producers
        );
    }

    public class CalcResultPartialObligationBuilder : ICalcResultPartialObligationBuilder
    {
        private readonly ApplicationDBContext dbContext;

        public CalcResultPartialObligationBuilder(ApplicationDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private ProducerReportedMaterial Scale(bool applyModulation, ProducerReportedMaterial reportedMaterial, CalcResultPartialObligation partialObligation)
        {
            var p = partialObligation.ObligatedFactor;
            if (!applyModulation || reportedMaterial.PackagingType == PackagingTypes.ConsumerWaste)
            {
                return new ProducerReportedMaterial
                {
                    Id               = reportedMaterial.Id,
                    MaterialId       = reportedMaterial.MaterialId,
                    ProducerDetailId = reportedMaterial.ProducerDetailId,
                    PackagingType    = reportedMaterial.PackagingType,
                    PackagingTonnage = MathUtils.RoundAwayFromZero(p * reportedMaterial.PackagingTonnage, 3),
                    SubmissionPeriod = reportedMaterial.SubmissionPeriod,
                    ProducerDetail   = reportedMaterial.ProducerDetail,
                    Material         = reportedMaterial.Material
                };
            }
            else
            {
                var r  = MathUtils.RoundAwayFromZero(p * reportedMaterial.PackagingTonnageRed          ?? 0m, 3);
                var a  = MathUtils.RoundAwayFromZero(p * reportedMaterial.PackagingTonnageAmber        ?? 0m, 3);
                var g  = MathUtils.RoundAwayFromZero(p * reportedMaterial.PackagingTonnageGreen        ?? 0m, 3);
                var rm = MathUtils.RoundAwayFromZero(p * reportedMaterial.PackagingTonnageRedMedical   ?? 0m, 3);
                var am = MathUtils.RoundAwayFromZero(p * reportedMaterial.PackagingTonnageAmberMedical ?? 0m, 3);
                var gm = MathUtils.RoundAwayFromZero(p * reportedMaterial.PackagingTonnageGreenMedical ?? 0m, 3);
                return new ProducerReportedMaterial
                {
                    Id                           = reportedMaterial.Id,
                    MaterialId                   = reportedMaterial.MaterialId,
                    ProducerDetailId             = reportedMaterial.ProducerDetailId,
                    PackagingType                = reportedMaterial.PackagingType,
                    PackagingTonnage             = r + a + g + rm + am + gm,
                    PackagingTonnageRed          = r,
                    PackagingTonnageAmber        = a,
                    PackagingTonnageGreen        = g,
                    PackagingTonnageRedMedical   = rm,
                    PackagingTonnageAmberMedical = am,
                    PackagingTonnageGreenMedical = gm,
                    SubmissionPeriod             = reportedMaterial.SubmissionPeriod,
                    ProducerDetail               = reportedMaterial.ProducerDetail,
                    Material                     = reportedMaterial.Material
                };
            }
        }

        // TODO move this to a utility
        public static ProducerDetail UpdateReportedMaterials(
            ProducerDetail pd,
            Func<ICollection<ProducerReportedMaterial>, ICollection<ProducerReportedMaterial>> f
        )
        {
            var updatedReportedMaterials = f(pd.ProducerReportedMaterials);
            pd.ProducerReportedMaterials.Clear();
            foreach (var item in updatedReportedMaterials) pd.ProducerReportedMaterials.Add(item);
            return pd;
        }

        public async Task<(List<L1Producer>, CalcResultPartialObligations)> ConstructAsync(
            RunContext runContext,
            IImmutableList<MaterialDetail> materialDetails,
            List<L1Producer> producers
        )
        {
            var partialObligationsForRun = await GetPartialObligations(runContext.RunId);
            var obligationsLookup = partialObligationsForRun.ToDictionary(p => (p.ProducerId, p.SubsidiaryId));

            var updatedProducers = producers.Select(l1 => new L1Producer(
                l1.OrganisationId,
                l1.Producers.Select(pd =>
                {
                    if (!obligationsLookup.TryGetValue((pd.ProducerId, pd.SubsidiaryId), out var obligation))
                        return pd;

                    obligation.PartialObligationTonnageByMaterial =
                        ComputeTonnageByMaterial(pd.ProducerReportedMaterials, materialDetails, obligation.ObligatedFactor, runContext.RequiresModulation);

                    return UpdateReportedMaterials(
                        pd,
                        reportedMaterials => reportedMaterials.Select(rm => Scale(runContext.RequiresModulation, rm, obligation)).ToList()
                    );
                }).ToList()
            )).ToList();

            var result = new CalcResultPartialObligations
            {
                PartialObligations = partialObligationsForRun
                    .OrderBy(p => p.ProducerId)
                    .ThenBy(p => p.Level)
                    .ThenBy(p => p.SubsidiaryId)
                    .ToImmutableList()
            };

            return (updatedProducers, result);
        }

        private async Task<List<CalcResultPartialObligation>> GetPartialObligations(int runId)
        {
            return await (
                from run in dbContext.CalculatorRuns.AsNoTracking()
                join crodm in dbContext.CalculatorRunOrganisationDataMaster.AsNoTracking() on run.CalculatorRunOrganisationDataMasterId equals crodm.Id
                join crodd in dbContext.CalculatorRunOrganisationDataDetails.AsNoTracking() on crodm.Id equals crodd.CalculatorRunOrganisationDataMasterId
                join pd in dbContext.ProducerDetail.AsNoTracking() on crodd.OrganisationId equals pd.ProducerId
                where run.Id == runId && crodd.ObligationStatus == ObligationStates.Obligated && crodd.DaysObligated != null && crodd.SubsidiaryId == pd.SubsidiaryId && pd.CalculatorRunId == runId
                let daysInYear = DateTime.IsLeapYear(crodm.RelativeYear) ? 366 : 365
                let partialAmount = crodd.DaysObligated != null ? (decimal)crodd.DaysObligated! / daysInYear : 1
                select new CalcResultPartialObligation
                {
                    ProducerId           = pd.ProducerId,
                    SubsidiaryId         = pd.SubsidiaryId,
                    ProducerName         = pd.ProducerName,
                    TradingName          = pd.TradingName,
                    Level                = pd.SubsidiaryId != null ? CommonConstants.LevelTwo.ToString() : CommonConstants.LevelOne.ToString(),
                    SubmissionYear       = crodm.RelativeYear,
                    DaysInSubmissionYear = daysInYear,
                    JoiningDate          = crodd.JoinerDate,
                    DaysObligated        = crodd.DaysObligated,
                    ObligatedFactor      = partialAmount
                }
            ).ToListAsync();
        }

        private static Dictionary<string, CalcResultPartialObligationTonnage> ComputeTonnageByMaterial(
            ICollection<ProducerReportedMaterial> reportedMaterials,
            IEnumerable<MaterialDetail> materials,
            decimal partialAmount,
            bool applyModulation)
        {
            CalcResultPartialObligationTonnage ToTonnage(IEnumerable<ProducerReportedMaterial> mats, string materialCode)
            {
                var isGlass = materialCode == MaterialCodes.Glass;
                var matList = mats.ToList();
                decimal hh, pb;
                decimal? hdc = null;
                RamTonnage? hhRam = null, pbRam = null, hdcRam = null;

                if (applyModulation)
                {
                    hhRam = RamTonnage.ToRamTonnage(matList, PackagingTypes.Household);
                    hh    = hhRam.TotalRamTonnage();
                    pbRam = RamTonnage.ToRamTonnage(matList, PackagingTypes.PublicBin);
                    pb    = pbRam.TotalRamTonnage();

                    if (isGlass)
                    {
                        hdcRam = RamTonnage.ToRamTonnage(matList, PackagingTypes.HouseholdDrinksContainers);
                        hdc = hdcRam.TotalRamTonnage();
                    }
                }
                else
                {
                    hh = RamTonnage.GetReportedTonnage(matList, PackagingTypes.Household, rm => rm.PackagingTonnage);
                    pb = RamTonnage.GetReportedTonnage(matList, PackagingTypes.PublicBin, rm => rm.PackagingTonnage);
                    hdc = isGlass ? RamTonnage.GetReportedTonnage(matList, PackagingTypes.HouseholdDrinksContainers, rm => rm.PackagingTonnage) : null;
                }

                var smcw = RamTonnage.GetReportedTonnage(matList, PackagingTypes.ConsumerWaste, rm => rm.PackagingTonnage);

                return new CalcResultPartialObligationTonnage
                {
                    ObligatedFactor                     = partialAmount,
                    HouseholdTonnage                    = hh,
                    HouseholdRAMTonnage                 = hhRam,
                    PublicBinTonnage                    = pb,
                    PublicBinRAMTonnage                 = pbRam,
                    HouseholdDrinksContainersTonnage    = hdc,
                    HouseholdDrinksContainersRAMTonnage = hdcRam,
                    SelfManagedConsumerWasteTonnage     = smcw
                };
            }

            var byMaterialCode = reportedMaterials
                .GroupBy(rm => rm.MaterialId)
                .ToDictionary(
                    g => materials.First(m => m.Id == g.Key).Code,
                    g => ToTonnage(g, materials.First(m => m.Id == g.Key).Code)
                );

            return materials.ToDictionary(
                m => m.Code,
                m => byMaterialCode.TryGetValue(m.Code, out var val) ? val : EmptyPartialTonnage(applyModulation, m.Code, partialAmount)
            );
        }

        private static CalcResultPartialObligationTonnage EmptyPartialTonnage(bool applyModulation, string materialCode, decimal partialAmount)
        {
            var isGlass = materialCode == MaterialCodes.Glass;
            if (applyModulation)
            {
                return new CalcResultPartialObligationTonnage()
                {
                    ObligatedFactor                     = partialAmount,
                    HouseholdTonnage                    = 0m,
                    HouseholdRAMTonnage                 = RamTonnage.Empty,
                    PublicBinTonnage                    = 0m,
                    PublicBinRAMTonnage                 = RamTonnage.Empty,
                    HouseholdDrinksContainersTonnage    = isGlass ? 0m : null,
                    HouseholdDrinksContainersRAMTonnage = isGlass ? RamTonnage.Empty : null,
                    SelfManagedConsumerWasteTonnage     = 0m

                };
            } else
            {
                return new CalcResultPartialObligationTonnage()
                {
                    ObligatedFactor                  = partialAmount,
                    HouseholdTonnage                 = 0m,
                    PublicBinTonnage                 = 0m,
                    HouseholdDrinksContainersTonnage = isGlass ? 0m : null,
                    SelfManagedConsumerWasteTonnage  = 0m
                };
            }
        }
    }
}
