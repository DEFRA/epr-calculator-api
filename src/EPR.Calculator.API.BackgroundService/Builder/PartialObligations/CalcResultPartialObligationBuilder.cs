using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.BackgroundService.Builder.PartialObligations
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

        private static ProducerReportedMaterial Scale(bool applyModulation, ProducerReportedMaterial reportedMaterial, CalcResultPartialObligation partialObligation)
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

        [ActivityTrace]
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

                    var reportedMaterials = pd.ProducerReportedMaterials.ToList();
                    var scaledMaterials   = reportedMaterials.Select(rm => Scale(runContext.RequiresModulation, rm, obligation)).ToList();

                    obligation.PartialObligationTonnageByMaterial =
                        ComputeTonnageByMaterial(reportedMaterials, scaledMaterials, materialDetails, obligation.ObligatedFactor, runContext.RequiresModulation);

                    return UpdateReportedMaterials(pd, _ => scaledMaterials);
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
                join pd in dbContext.ProducerDetail.AsNoTracking() on run.Id equals pd.CalculatorRunId
                // ProducerDetail rows only ever exist for obligated organisations - a ProducerDetail row
                // is only ever created for a record with reported materials, which only ever comes from
                // an obligated ("O"-status) aligner-produced ProducerRecord.
                where run.Id == runId && pd.DaysObligated != null
                let daysInYear = DateTime.IsLeapYear(run.RelativeYear) ? 366 : 365
                let partialAmount = pd.DaysObligated != null ? (decimal)pd.DaysObligated! / daysInYear : 1
                select new CalcResultPartialObligation
                {
                    ProducerId           = pd.ProducerId,
                    SubsidiaryId         = pd.SubsidiaryId,
                    ProducerName         = pd.ProducerName,
                    TradingName          = pd.TradingName,
                    Level                = pd.SubsidiaryId != null ? CommonConstants.LevelTwo.ToString() : CommonConstants.LevelOne.ToString(),
                    SubmissionYear       = run.RelativeYear,
                    DaysInSubmissionYear = daysInYear,
                    JoiningDate          = pd.JoinerDate,
                    DaysObligated        = pd.DaysObligated,
                    ObligatedFactor      = partialAmount
                }
            ).ToListAsync();
        }

        // The partial figures are summed from the same scaled rows the fees use, so the section shows what the fees were worked out on.
        private static Dictionary<string, CalcResultPartialObligationTonnage> ComputeTonnageByMaterial(
            IReadOnlyCollection<ProducerReportedMaterial> reportedMaterials,
            IReadOnlyCollection<ProducerReportedMaterial> scaledMaterials,
            IReadOnlyCollection<MaterialDetail> materials,
            decimal partialAmount,
            bool applyModulation)
        {
            Dictionary<string, MaterialTotals> TotalsByMaterialCode(IEnumerable<ProducerReportedMaterial> rows) =>
                rows.GroupBy(rm => rm.MaterialId)
                    .ToDictionary(
                        g => materials.First(m => m.Id == g.Key).Code,
                        g => MaterialTotals.From(g.ToList(), materials.First(m => m.Id == g.Key).Code, applyModulation));

            var reported = TotalsByMaterialCode(reportedMaterials);
            var partial  = TotalsByMaterialCode(scaledMaterials);

            return materials.ToDictionary(
                m => m.Code,
                m =>
                {
                    var r = reported.GetValueOrDefault(m.Code) ?? MaterialTotals.Empty(m.Code, applyModulation);
                    var p = partial.GetValueOrDefault(m.Code) ?? MaterialTotals.Empty(m.Code, applyModulation);
                    return new CalcResultPartialObligationTonnage
                    {
                        ObligatedFactor                            = partialAmount,
                        HouseholdTonnage                           = r.Household,
                        HouseholdRAMTonnage                        = r.HouseholdRam,
                        PublicBinTonnage                           = r.PublicBin,
                        PublicBinRAMTonnage                        = r.PublicBinRam,
                        HouseholdDrinksContainersTonnage           = r.DrinksContainers,
                        HouseholdDrinksContainersRAMTonnage        = r.DrinksContainersRam,
                        SelfManagedConsumerWasteTonnage            = r.SelfManagedConsumerWaste,
                        PartialHouseholdTonnage                    = p.Household,
                        PartialHouseholdRAMTonnage                 = p.HouseholdRam,
                        PartialPublicBinTonnage                    = p.PublicBin,
                        PartialPublicBinRAMTonnage                 = p.PublicBinRam,
                        PartialHouseholdDrinksContainersTonnage    = p.DrinksContainers,
                        PartialHouseholdDrinksContainersRAMTonnage = p.DrinksContainersRam,
                        PartialSelfManagedConsumerWasteTonnage     = p.SelfManagedConsumerWaste
                    };
                });
        }

        private sealed record MaterialTotals(
            decimal Household,
            RamTonnage? HouseholdRam,
            decimal PublicBin,
            RamTonnage? PublicBinRam,
            decimal? DrinksContainers,
            RamTonnage? DrinksContainersRam,
            decimal SelfManagedConsumerWaste)
        {
            public static MaterialTotals From(List<ProducerReportedMaterial> rows, string materialCode, bool applyModulation)
            {
                var isGlass = materialCode == MaterialCodes.Glass;
                var smcw    = RamTonnage.GetReportedTonnage(rows, PackagingTypes.ConsumerWaste, rm => rm.PackagingTonnage);

                if (applyModulation)
                {
                    var hhRam  = RamTonnage.ToRamTonnage(rows, PackagingTypes.Household);
                    var pbRam  = RamTonnage.ToRamTonnage(rows, PackagingTypes.PublicBin);
                    var hdcRam = isGlass ? RamTonnage.ToRamTonnage(rows, PackagingTypes.HouseholdDrinksContainers) : null;
                    return new MaterialTotals(hhRam.TotalRamTonnage(), hhRam, pbRam.TotalRamTonnage(), pbRam, hdcRam?.TotalRamTonnage(), hdcRam, smcw);
                }

                return new MaterialTotals(
                    RamTonnage.GetReportedTonnage(rows, PackagingTypes.Household, rm => rm.PackagingTonnage), null,
                    RamTonnage.GetReportedTonnage(rows, PackagingTypes.PublicBin, rm => rm.PackagingTonnage), null,
                    isGlass ? RamTonnage.GetReportedTonnage(rows, PackagingTypes.HouseholdDrinksContainers, rm => rm.PackagingTonnage) : null, null,
                    smcw);
            }

            public static MaterialTotals Empty(string materialCode, bool applyModulation)
            {
                decimal? drinksContainers = materialCode == MaterialCodes.Glass ? 0m : null;
                if (!applyModulation)
                    return new MaterialTotals(0m, null, 0m, null, drinksContainers, null, 0m);

                var drinksContainersRam = drinksContainers is null ? null : RamTonnage.Empty;
                return new MaterialTotals(0m, RamTonnage.Empty, 0m, RamTonnage.Empty, drinksContainers, drinksContainersRam, 0m);
            }
        }
    }
}
