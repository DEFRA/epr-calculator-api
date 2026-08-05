using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public interface ICalcResultReader
    {
        Task<IReadOnlyList<CalcResultH1ProjectedProducer>> ReadH1ProjectedData(int runId, CancellationToken cancellationToken);
        Task<IReadOnlyList<CalcResultH2ProjectedProducer>> ReadH2ProjectedData(int runId, CancellationToken cancellationToken);
        Task<IReadOnlyList<CalcResultScaledupProducer>> ReadScaledData(int runId, CancellationToken cancellationToken);
        Task<IReadOnlyList<CalcResultPartialObligation>> ReadPartialData(int runId, CancellationToken cancellationToken);
        Task<ProducerFees> ReadProducerFees(int runId, CancellationToken cancellationToken);
        Task<SelfManagedConsumerWaste> ReadSmcw(int runId, CancellationToken cancellationToken);
        Task<ModulationResult> ReadModulationResult(int runId, CancellationToken cancellationToken);
        Task<CalcResultLapcapData> ReadLapcapData(int runId, CancellationToken cancellationToken);
        Task<CalcResultCommsCost> ReadCommsCost(int runId, CancellationToken cancellationToken);
        Task<CalcResultLateReportingTonnage> ReadLateReportingTonnage(int runId, CancellationToken cancellationToken);
        Task<CalcResultParameterOtherCost> ReadParameterOtherCost(int runId, CancellationToken cancellationToken);
        Task<CalcResultOnePlusFourApportionment> ReadOnePlusFourApportionment(int runId, CancellationToken cancellationToken);
        Task<CalcResultLaDisposalCostData> ReadLaDisposalCostData(int runId, CancellationToken cancellationToken);
        Task<IReadOnlyList<CalcResultCancelledProducer>> ReadCancelledProducers(int runId, CancellationToken cancellationToken);
    }

    public class CalcResultReader(ApplicationDBContext dbContext) : ICalcResultReader
    {
        public async Task<IReadOnlyList<CalcResultH1ProjectedProducer>> ReadH1ProjectedData(int runId, CancellationToken cancellationToken)
        {
            return await dbContext.TransformProjectedH1
                        .Where(p => p.CalculatorRunId == runId)
                        .GroupBy(p => new { p.ProducerId, p.SubsidiaryId, p.SubmissionPeriodCode, p.Level })
                        .Select(g => new CalcResultH1ProjectedProducer
                        {
                            ProducerId = g.Key.ProducerId,
                            SubsidiaryId = g.Key.SubsidiaryId,
                            Level = g.Key.Level,
                            SubmissionPeriodCode = g.Key.SubmissionPeriodCode,
                            H1ProjectedTonnageByMaterial = MapToH1MaterialTonnages(g.ToList())
                        })
                        .OrderBy(p => p.ProducerId)
                        .ThenBy(p => p.Level)
                        .ThenBy(p => p.SubsidiaryId)
                        .ToImmutableListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<CalcResultH2ProjectedProducer>> ReadH2ProjectedData(int runId, CancellationToken cancellationToken)
        {
            return await dbContext.TransformProjectedH2
                        .Where(p => p.CalculatorRunId == runId)
                        .GroupBy(p => new { p.ProducerId, p.SubsidiaryId, p.SubmissionPeriodCode, p.Level })
                        .Select(g => new CalcResultH2ProjectedProducer
                        {
                            ProducerId = g.Key.ProducerId,
                            SubsidiaryId = g.Key.SubsidiaryId,
                            Level = g.Key.Level,
                            SubmissionPeriodCode = g.Key.SubmissionPeriodCode,
                            H2ProjectedTonnageByMaterial = MapToH2MaterialTonnages(g.ToList())
                        })
                        .OrderBy(p => p.ProducerId)
                        .ThenBy(p => p.Level)
                        .ThenBy(p => p.SubsidiaryId)
                        .ToImmutableListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<CalcResultScaledupProducer>> ReadScaledData(int runId, CancellationToken cancellationToken)
        {
            return await dbContext.TransformScaled
                        .Where(p => p.CalculatorRunId == runId)
                        .GroupBy(p => new { p.ProducerId, p.SubsidiaryId, p.ProducerName, p.TradingName, p.SubmissionPeriodCode, p.Level, p.IsSubTotal, p.DaysInSubmissionPeriod, p.DaysInWholePeriod, p.ScaleupFactor })
                        .Select(g =>
                            new CalcResultScaledupProducer
                            {
                                ProducerId = g.Key.ProducerId,
                                SubsidiaryId = g.Key.SubsidiaryId,
                                ProducerName = g.Key.ProducerName,
                                TradingName = g.Key.TradingName,
                                Level = g.Key.Level,
                                IsSubtotalRow = g.Key.IsSubTotal,
                                SubmissionPeriodCode = g.Key.SubmissionPeriodCode,
                                DaysInSubmissionPeriod = g.Key.DaysInSubmissionPeriod,
                                DaysInWholePeriod = g.Key.DaysInWholePeriod,
                                ScaleupFactor = g.Key.ScaleupFactor,
                                PomData = MapToScaled(g.ToList())
                            }
                        )
                        .OrderBy(p => p.ProducerId)
                        .ThenBy(p => p.SubmissionPeriodCode)
                        .ThenBy(p => p.Level)
                        .ThenBy(p => p.SubsidiaryId)
                        .ThenBy(p => p.SubmissionPeriodCode)
                        .ToImmutableListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<CalcResultPartialObligation>> ReadPartialData(int runId, CancellationToken cancellationToken){
            return await dbContext.TransformPartial
                        .Where(p => p.CalculatorRunId == runId)
                        .GroupBy(p => new { p.ProducerId, p.SubsidiaryId, p.ProducerName, p.TradingName, p.SubmissionYear, p.Level, p.DaysInSubmissionYear, p.JoiningDate, p.DaysObligated, p.ObligatedFactor })
                        .Select(g =>
                            new CalcResultPartialObligation
                            {
                                ProducerId = g.Key.ProducerId,
                                SubsidiaryId = g.Key.SubsidiaryId,
                                ProducerName = g.Key.ProducerName,
                                TradingName = g.Key.TradingName,
                                Level = g.Key.Level,
                                SubmissionYear = g.Key.SubmissionYear,
                                DaysInSubmissionYear = g.Key.DaysInSubmissionYear,
                                JoiningDate = g.Key.JoiningDate,
                                DaysObligated = g.Key.DaysObligated,
                                ObligatedFactor = g.Key.ObligatedFactor,
                                PartialObligationTonnageByMaterial = MapToPartial(g.ToList())
                            }
                        )
                        .OrderBy(p => p.ProducerId)
                        .ThenBy(p => p.Level)
                        .ThenBy(p => p.SubsidiaryId)
                        .ToImmutableListAsync(cancellationToken);
        }

        public async Task<ProducerFees> ReadProducerFees(int runId, CancellationToken cancellationToken)
        {
            return await dbContext.ProducerDisposalFee
                        .Include(p => p.Details)
                        .Where(p => p.CalculatorRunId == runId)
                        .SingleAsync(cancellationToken);
        }

        public async Task<SelfManagedConsumerWaste> ReadSmcw(int runId, CancellationToken cancellationToken) =>
            await dbContext.SelfManagedConsumerWaste
                    .Include(s => s.ProducerTotals)
                    .Where(p => p.CalculatorRunId == runId)
                    .SingleAsync(cancellationToken);
        

        public async Task<ModulationResult> ReadModulationResult(int runId, CancellationToken cancellationToken) =>
            await dbContext.ModulationResult
                    .Where(p => p.CalculatorRunId == runId)
                    .SingleAsync(cancellationToken);

        public async Task<CalcResultLapcapData> ReadLapcapData(int runId, CancellationToken cancellationToken) =>
            await dbContext.LapcapData
                    .AsNoTracking()
                    .Where(p => p.CalculatorRunId == runId)
                    .Select(x => x.LapcapData)
                    .SingleAsync(cancellationToken);

        public async Task<CalcResultCommsCost> ReadCommsCost(int runId, CancellationToken cancellationToken) =>
            await dbContext.CommCost
                    .AsNoTracking()
                    .Where(p => p.CalculatorRunId == runId)
                    .Select(x => x.CommsCost)
                    .SingleAsync(cancellationToken);

        public async Task<CalcResultLateReportingTonnage> ReadLateReportingTonnage(int runId, CancellationToken cancellationToken) =>
            await dbContext.LateReportingTonnage
                    .AsNoTracking()
                    .Where(p => p.CalculatorRunId == runId)
                    .Select(x => x.LateReportingTonnage)
                    .SingleAsync(cancellationToken);

        public async Task<CalcResultParameterOtherCost> ReadParameterOtherCost(int runId, CancellationToken cancellationToken) =>
            await dbContext.ParameterOtherCost
                    .AsNoTracking()
                    .Where(p => p.CalculatorRunId == runId)
                    .Select(x => x.ParameterOtherCost)
                    .SingleAsync(cancellationToken);

        public async Task<CalcResultOnePlusFourApportionment> ReadOnePlusFourApportionment(int runId, CancellationToken cancellationToken) =>
            await dbContext.OnePlusFourApportionment
                    .AsNoTracking()
                    .Where(p => p.CalculatorRunId == runId)
                    .Select(x => x.OnePlusFourApportionment)
                    .SingleAsync(cancellationToken);

        public async Task<CalcResultLaDisposalCostData> ReadLaDisposalCostData(int runId, CancellationToken cancellationToken) =>
            await dbContext.LaDisposalCostData
                    .AsNoTracking()
                    .Where(p => p.CalculatorRunId == runId)
                    .Select(x => x.LaDisposalCost)
                    .SingleAsync(cancellationToken);

        public async Task<IReadOnlyList<CalcResultCancelledProducer>> ReadCancelledProducers(int runId, CancellationToken cancellationToken) =>
            await dbContext.CancelledProducers
                    .AsNoTracking()
                    .Where(p => p.CalculatorRunId == runId)
                    .Select(x => x.CancelledProducer)
                    .ToImmutableListAsync(cancellationToken);

        private static Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage> MapToH1MaterialTonnages(List<TransformProjectedH1> transformProjectedH1s)
        {
            return transformProjectedH1s.ToDictionary(
                t => t.MaterialCode,
                t => new CalcResultH1ProjectedProducerMaterialTonnage
                {
                    HouseholdTonnage = t.HouseholdTonnage,
                    HouseholdRAMTonnage = new RamTonnage
                    {
                        Red = t.HouseholdTonnageRed,
                        Amber = t.HouseholdTonnageAmber,
                        Green = t.HouseholdTonnageGreen,
                        RedMedical = t.HouseholdTonnageRedMedical,
                        AmberMedical = t.HouseholdTonnageAmberMedical,
                        GreenMedical = t.HouseholdTonnageGreenMedical
                    },
                    PublicBinTonnage = t.PublicBinTonnage,
                    PublicBinRAMTonnage = new RamTonnage
                    {
                        Red = t.PublicBinTonnageRed,
                        Amber = t.PublicBinTonnageAmber,
                        Green = t.PublicBinTonnageGreen,
                        RedMedical = t.PublicBinTonnageRedMedical,
                        AmberMedical = t.PublicBinTonnageAmberMedical,
                        GreenMedical = t.PublicBinTonnageGreenMedical
                    },
                    HouseholdDrinksContainerTonnage = t.HDCTonnage,
                    HouseholdDrinksContainerRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? new RamTonnage
                    {
                        Red = t.HDCTonnageRed!.Value,
                        Amber = t.HDCTonnageAmber!.Value,
                        Green = t.HDCTonnageGreen!.Value,
                        RedMedical = t.HDCTonnageRedMedical!.Value,
                        AmberMedical = t.HDCTonnageAmberMedical!.Value,
                        GreenMedical = t.HDCTonnageGreenMedical!.Value
                    } : null,
                    HouseholdTonnageWithoutRAM = t.HouseholdTonnageWithoutRAM,
                    PublicBinTonnageWithoutRAM = t.PublicBinTonnageWithoutRAM,
                    HouseholdDrinksContainerTonnageWithoutRAM = t.HDCTonnageWithoutRAM,
                    ProjectedHouseholdTonnage = t.ProjectedHouseholdTonnage,
                    ProjectedHouseholdRAMTonnage = new RamTonnage
                    {
                        Red = t.ProjectedHouseholdTonnageRed,
                        Amber = t.ProjectedHouseholdTonnageAmber,
                        Green = t.ProjectedHouseholdTonnageGreen,
                        RedMedical = t.ProjectedHouseholdTonnageRedMedical,
                        AmberMedical = t.ProjectedHouseholdTonnageAmberMedical,
                        GreenMedical = t.ProjectedHouseholdTonnageGreenMedical
                    },
                    ProjectedPublicBinTonnage = t.ProjectedPublicBinTonnage,
                    ProjectedPublicBinRAMTonnage = new RamTonnage
                    {
                        Red = t.ProjectedPublicBinTonnageRed,
                        Amber = t.ProjectedPublicBinTonnageAmber,
                        Green = t.ProjectedPublicBinTonnageGreen,
                        RedMedical = t.ProjectedPublicBinTonnageRedMedical,
                        AmberMedical = t.ProjectedPublicBinTonnageAmberMedical,
                        GreenMedical = t.ProjectedPublicBinTonnageGreenMedical
                    },
                    ProjectedHouseholdDrinksContainerTonnage = t.ProjectedHDCTonnage,
                    ProjectedHouseholdDrinksContainerRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? new RamTonnage
                    {
                        Red = t.ProjectedHDCTonnageRed!.Value,
                        Amber = t.ProjectedHDCTonnageAmber!.Value,
                        Green = t.ProjectedHDCTonnageGreen!.Value,
                        RedMedical = t.ProjectedHDCTonnageRedMedical!.Value,
                        AmberMedical = t.ProjectedHDCTonnageAmberMedical!.Value,
                        GreenMedical = t.ProjectedHDCTonnageGreenMedical!.Value
                    } : null,
                    H2RamProportions = new RAMProportions
                    {
                        Red = t.H2RamProportionsRed,
                        Amber = t.H2RamProportionsAmber,
                        Green = t.H2RamProportionsGreen,
                        RedMedical = t.H2RamProportionsRedMedical,
                        AmberMedical = t.H2RamProportionsAmberMedical,
                        GreenMedical = t.H2RamProportionsGreenMedical
                    }
                }
            );
        }

        private static Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage> MapToH2MaterialTonnages(List<TransformProjectedH2> transformProjectedH2s)
        {
            return transformProjectedH2s.ToDictionary(
                t => t.MaterialCode,
                t => new CalcResultH2ProjectedProducerMaterialTonnage
                {
                    HouseholdTonnage = t.HouseholdTonnage,
                    HouseholdRAMTonnage = new RamTonnage
                    {
                        Red = t.HouseholdTonnageRed,
                        Amber = t.HouseholdTonnageAmber,
                        Green = t.HouseholdTonnageGreen,
                        RedMedical = t.HouseholdTonnageRedMedical,
                        AmberMedical = t.HouseholdTonnageAmberMedical,
                        GreenMedical = t.HouseholdTonnageGreenMedical
                    },
                    PublicBinTonnage = t.PublicBinTonnage,
                    PublicBinRAMTonnage = new RamTonnage
                    {
                        Red = t.PublicBinTonnageRed,
                        Amber = t.PublicBinTonnageAmber,
                        Green = t.PublicBinTonnageGreen,
                        RedMedical = t.PublicBinTonnageRedMedical,
                        AmberMedical = t.PublicBinTonnageAmberMedical,
                        GreenMedical = t.PublicBinTonnageGreenMedical
                    },
                    HouseholdDrinksContainerTonnage = t.HDCTonnage,
                    HouseholdDrinksContainerRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? new RamTonnage
                    {
                        Red = t.HDCTonnageRed!.Value,
                        Amber = t.HDCTonnageAmber!.Value,
                        Green = t.HDCTonnageGreen!.Value,
                        RedMedical = t.HDCTonnageRedMedical!.Value,
                        AmberMedical = t.HDCTonnageAmberMedical!.Value,
                        GreenMedical = t.HDCTonnageGreenMedical!.Value
                    } : null,
                    HouseholdTonnageWithoutRAM = t.HouseholdTonnageWithoutRAM,
                    PublicBinTonnageWithoutRAM = t.PublicBinTonnageWithoutRAM,
                    HouseholdDrinksContainerTonnageWithoutRAM = t.HDCTonnageWithoutRAM,
                    ProjectedHouseholdTonnage = t.ProjectedHouseholdTonnage,
                    ProjectedHouseholdRAMTonnage = new RamTonnage
                    {
                        Red = t.ProjectedHouseholdTonnageRed,
                        Amber = t.ProjectedHouseholdTonnageAmber,
                        Green = t.ProjectedHouseholdTonnageGreen,
                        RedMedical = t.ProjectedHouseholdTonnageRedMedical,
                        AmberMedical = t.ProjectedHouseholdTonnageAmberMedical,
                        GreenMedical = t.ProjectedHouseholdTonnageGreenMedical
                    },
                    ProjectedPublicBinTonnage = t.ProjectedPublicBinTonnage,
                    ProjectedPublicBinRAMTonnage = new RamTonnage
                    {
                        Red = t.ProjectedPublicBinTonnageRed,
                        Amber = t.ProjectedPublicBinTonnageAmber,
                        Green = t.ProjectedPublicBinTonnageGreen,
                        RedMedical = t.ProjectedPublicBinTonnageRedMedical,
                        AmberMedical = t.ProjectedPublicBinTonnageAmberMedical,
                        GreenMedical = t.ProjectedPublicBinTonnageGreenMedical
                    },
                    ProjectedHouseholdDrinksContainerTonnage = t.ProjectedHDCTonnage,
                    ProjectedHouseholdDrinksContainerRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? new RamTonnage
                    {
                        Red = t.ProjectedHDCTonnageRed!.Value,
                        Amber = t.ProjectedHDCTonnageAmber!.Value,
                        Green = t.ProjectedHDCTonnageGreen!.Value,
                        RedMedical = t.ProjectedHDCTonnageRedMedical!.Value,
                        AmberMedical = t.ProjectedHDCTonnageAmberMedical!.Value,
                        GreenMedical = t.ProjectedHDCTonnageGreenMedical!.Value
                    } : null
                }
            );
        }

        private static ImmutableList<ScaledupPomEntry> MapToScaled(List<TransformScaled> scaled)
        {
            return scaled.Select(s =>
                new ScaledupPomEntry(
                    MaterialId: s.MaterialId,
                    PackagingType: s.PackagingType,
                    Tonnage: s.Tonnage,
                    ScaledTonnage: s.ScaledTonnage
                )
            ).ToImmutableList();
        }

        private static Dictionary<string, CalcResultPartialObligationTonnage> MapToPartial(List<TransformPartial> partial)
        {
            RamTonnage? ToMaybeRamTonnage(
                decimal? red,
                decimal? amber,
                decimal? green,
                decimal? redMedical,
                decimal? amberMedical,
                decimal? greenMedical)
            {
                return red is null && amber is null && green is null && redMedical is null && amberMedical is null && greenMedical is null
                    ? null
                    : new RamTonnage
                    {
                        Red = red!.Value,
                        Amber = amber!.Value,
                        Green = green!.Value,
                        RedMedical = redMedical!.Value,
                        AmberMedical = amberMedical!.Value,
                        GreenMedical = greenMedical!.Value
                    };
            }

            return partial.ToDictionary(
                t => t.MaterialCode,
                t => new CalcResultPartialObligationTonnage
                {
                    ObligatedFactor = t.ObligatedFactor,
                    HouseholdTonnage = t.HouseholdTonnage,
                    HouseholdRAMTonnage = ToMaybeRamTonnage(
                        t.HouseholdTonnageRed,
                        t.HouseholdTonnageAmber,
                        t.HouseholdTonnageGreen,
                        t.HouseholdTonnageRedMedical,
                        t.HouseholdTonnageAmberMedical,
                        t.HouseholdTonnageGreenMedical
                    ),
                    PublicBinTonnage = t.PublicBinTonnage,
                    PublicBinRAMTonnage = ToMaybeRamTonnage(
                        t.PublicBinTonnageRed,
                        t.PublicBinTonnageAmber,
                        t.PublicBinTonnageGreen,
                        t.PublicBinTonnageRedMedical,
                        t.PublicBinTonnageAmberMedical,
                        t.PublicBinTonnageGreenMedical
                    ),
                    HouseholdDrinksContainersTonnage = t.HDCTonnage,
                    HouseholdDrinksContainersRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? ToMaybeRamTonnage(
                        t.HDCTonnageRed,
                        t.HDCTonnageAmber,
                        t.HDCTonnageGreen,
                        t.HDCTonnageRedMedical,
                        t.HDCTonnageAmberMedical,
                        t.HDCTonnageGreenMedical
                    ) : null,
                    SelfManagedConsumerWasteTonnage = t.SMCWTonnage
                }
            );
        }
    }
}
