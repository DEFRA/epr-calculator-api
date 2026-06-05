using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public interface ICalcResultService
    {
        Task StoreProjectedH1Data(int runId, IReadOnlyList<CalcResultH1ProjectedProducer> projectedProducers);
        Task StoreProjectedH2Data(int runId, IReadOnlyList<CalcResultH2ProjectedProducer> projectedProducers);
        Task StoreScaledData(int runId, IReadOnlyList<CalcResultScaledupProducer> scaled);
        Task StorePartialData(int runId, IReadOnlyList<CalcResultPartialObligation> partial);
        Task<IReadOnlyList<CalcResultH1ProjectedProducer>> ReadH1ProjectedData(int runId);
        Task<IReadOnlyList<CalcResultH2ProjectedProducer>> ReadH2ProjectedData(int runId);
        Task<IReadOnlyList<CalcResultScaledupProducer>> ReadScaledData(int runId);
        Task<IReadOnlyList<CalcResultPartialObligation>> ReadPartialData(int runId);
    }

    public class CalcResultService(ApplicationDBContext dbContext) : ICalcResultService
    {
        public async Task StoreProjectedH1Data(int runId, IReadOnlyList<CalcResultH1ProjectedProducer> projectedProducers)
        {
            await StoreData(dbContext.TransformProjectedH1, projectedProducers, p => 
                p.H1ProjectedTonnageByMaterial.Select(m => 
                    MapToTransformProjectedH1(runId, p.ProducerId, p.SubsidiaryId, m.Key, p.SubmissionPeriodCode, p.Level, m.Value)
                )
            );
        }

        public async Task StoreProjectedH2Data(int runId, IReadOnlyList<CalcResultH2ProjectedProducer> projectedProducers)
        {
            await StoreData(dbContext.TransformProjectedH2, projectedProducers, p => 
                p.H2ProjectedTonnageByMaterial.Select(m => 
                    MapToTransformProjectedH2(runId, p.ProducerId, p.SubsidiaryId, m.Key, p.SubmissionPeriodCode, p.Level, m.Value)
                )
            );
        }

        public async Task<IReadOnlyList<CalcResultH1ProjectedProducer>> ReadH1ProjectedData(int runId)
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
                        .ToImmutableListAsync();
        }

        public async Task<IReadOnlyList<CalcResultH2ProjectedProducer>> ReadH2ProjectedData(int runId)
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
                        .ToImmutableListAsync();
        }

        public async Task StoreScaledData(int runId, IReadOnlyList<CalcResultScaledupProducer> scaled)
        {
             await StoreData(dbContext.TransformScaled, scaled, p => 
                p.PomData.Select(m => 
                    new TransformScaled
                    {
                        CalculatorRunId = runId,
                        ProducerId = p.ProducerId, 
                        SubsidiaryId = p.SubsidiaryId,
                        ProducerName = p.ProducerName,
                        TradingName = p.TradingName,
                        SubmissionPeriodCode = p.SubmissionPeriodCode,
                        Level = p.Level,
                        IsSubTotal = p.IsSubtotalRow,
                        DaysInSubmissionPeriod = p.DaysInSubmissionPeriod,
                        DaysInWholePeriod = p.DaysInWholePeriod,
                        ScaleupFactor = p.ScaleupFactor,
                        MaterialId = m.MaterialId,
                        PackagingType = m.PackagingType,
                        Tonnage = m.Tonnage,
                        ScaledTonnage = m.ScaledTonnage
                    }
                )
            );
        }
        
        public async Task<IReadOnlyList<CalcResultScaledupProducer>> ReadScaledData(int runId)
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
                        .ThenBy(p => p.Level)
                        .ThenBy(p => p.SubsidiaryId)
                        .ThenBy(p => p.SubmissionPeriodCode)
                        .ToImmutableListAsync();
        }

        public async Task StorePartialData(int runId, IReadOnlyList<CalcResultPartialObligation> partial){
            await StoreData(dbContext.TransformPartial, partial, p => 
                p.PartialObligationTonnageByMaterial.Select(m => 
                    MapToTransformPartial(runId, m.Key, p, m.Value)
                )
            );
        }

        public async Task<IReadOnlyList<CalcResultPartialObligation>> ReadPartialData(int runId){
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
                        .ToImmutableListAsync();
        }

        private async Task StoreData<TSource, TEntity>(DbSet<TEntity> dbSet, IReadOnlyList<TSource> data, Func<TSource, IEnumerable<TEntity>> mapper) where TEntity : class
        {
            await dbSet.AddRangeAsync(data.SelectMany(mapper));
            await dbContext.SaveChangesAsync();
        }

        private static Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage> MapToH1MaterialTonnages(List<TransformProjectedH1> transformProjectedH1s)
        {
            return transformProjectedH1s.ToDictionary(
                t => t.MaterialCode, 
                t => new CalcResultH1ProjectedProducerMaterialTonnage
                {
                    HouseholdTonnage = t.HouseholdTonnage,
                    HouseholdRAMTonnage = new RAMTonnage
                    {
                        RedTonnage = t.HouseholdTonnageRed,
                        AmberTonnage = t.HouseholdTonnageAmber,
                        GreenTonnage = t.HouseholdTonnageGreen,
                        RedMedicalTonnage = t.HouseholdTonnageRedMedical,
                        AmberMedicalTonnage = t.HouseholdTonnageAmberMedical,
                        GreenMedicalTonnage = t.HouseholdTonnageGreenMedical
                    },
                    PublicBinTonnage = t.PublicBinTonnage,
                    PublicBinRAMTonnage = new RAMTonnage
                    {
                        RedTonnage = t.PublicBinTonnageRed,
                        AmberTonnage = t.PublicBinTonnageAmber,
                        GreenTonnage = t.PublicBinTonnageGreen,
                        RedMedicalTonnage = t.PublicBinTonnageRedMedical,
                        AmberMedicalTonnage = t.PublicBinTonnageAmberMedical,
                        GreenMedicalTonnage = t.PublicBinTonnageGreenMedical
                    },
                    HouseholdDrinksContainerTonnage = t.HDCTonnage,
                    HouseholdDrinksContainerRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? new RAMTonnage
                    {
                        RedTonnage = t.HDCTonnageRed!.Value,
                        AmberTonnage = t.HDCTonnageAmber!.Value,
                        GreenTonnage = t.HDCTonnageGreen!.Value,
                        RedMedicalTonnage = t.HDCTonnageRedMedical!.Value,
                        AmberMedicalTonnage = t.HDCTonnageAmberMedical!.Value,
                        GreenMedicalTonnage = t.HDCTonnageGreenMedical!.Value
                    } : null,
                    HouseholdTonnageWithoutRAM = t.HouseholdTonnageWithoutRAM,
                    PublicBinTonnageWithoutRAM = t.PublicBinTonnageWithoutRAM,
                    HouseholdDrinksContainerTonnageWithoutRAM = t.HDCTonnageWithoutRAM,
                    ProjectedHouseholdTonnage = t.ProjectedHouseholdTonnage,
                    ProjectedHouseholdRAMTonnage = new RAMTonnage
                    {
                        RedTonnage = t.ProjectedHouseholdTonnageRed,
                        AmberTonnage = t.ProjectedHouseholdTonnageAmber,
                        GreenTonnage = t.ProjectedHouseholdTonnageGreen,
                        RedMedicalTonnage = t.ProjectedHouseholdTonnageRedMedical,
                        AmberMedicalTonnage = t.ProjectedHouseholdTonnageAmberMedical,
                        GreenMedicalTonnage = t.ProjectedHouseholdTonnageGreenMedical
                    },
                    ProjectedPublicBinTonnage = t.ProjectedPublicBinTonnage,
                    ProjectedPublicBinRAMTonnage = new RAMTonnage
                    {
                        RedTonnage = t.ProjectedPublicBinTonnageRed,
                        AmberTonnage = t.ProjectedPublicBinTonnageAmber,
                        GreenTonnage = t.ProjectedPublicBinTonnageGreen,
                        RedMedicalTonnage = t.ProjectedPublicBinTonnageRedMedical,
                        AmberMedicalTonnage = t.ProjectedPublicBinTonnageAmberMedical,
                        GreenMedicalTonnage = t.ProjectedPublicBinTonnageGreenMedical
                    },
                    ProjectedHouseholdDrinksContainerTonnage = t.ProjectedHDCTonnage,
                    ProjectedHouseholdDrinksContainerRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? new RAMTonnage
                    {
                        RedTonnage = t.ProjectedHDCTonnageRed!.Value,
                        AmberTonnage = t.ProjectedHDCTonnageAmber!.Value,
                        GreenTonnage = t.ProjectedHDCTonnageGreen!.Value,
                        RedMedicalTonnage = t.ProjectedHDCTonnageRedMedical!.Value,
                        AmberMedicalTonnage = t.ProjectedHDCTonnageAmberMedical!.Value,
                        GreenMedicalTonnage = t.ProjectedHDCTonnageGreenMedical!.Value
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
                    HouseholdRAMTonnage = new RAMTonnage
                    {
                        RedTonnage = t.HouseholdTonnageRed,
                        AmberTonnage = t.HouseholdTonnageAmber,
                        GreenTonnage = t.HouseholdTonnageGreen,
                        RedMedicalTonnage = t.HouseholdTonnageRedMedical,
                        AmberMedicalTonnage = t.HouseholdTonnageAmberMedical,
                        GreenMedicalTonnage = t.HouseholdTonnageGreenMedical
                    },
                    PublicBinTonnage = t.PublicBinTonnage,
                    PublicBinRAMTonnage = new RAMTonnage
                    {
                        RedTonnage = t.PublicBinTonnageRed,
                        AmberTonnage = t.PublicBinTonnageAmber,
                        GreenTonnage = t.PublicBinTonnageGreen,
                        RedMedicalTonnage = t.PublicBinTonnageRedMedical,
                        AmberMedicalTonnage = t.PublicBinTonnageAmberMedical,
                        GreenMedicalTonnage = t.PublicBinTonnageGreenMedical
                    },
                    HouseholdDrinksContainerTonnage = t.HDCTonnage,
                    HouseholdDrinksContainerRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? new RAMTonnage
                    {
                        RedTonnage = t.HDCTonnageRed!.Value,
                        AmberTonnage = t.HDCTonnageAmber!.Value,
                        GreenTonnage = t.HDCTonnageGreen!.Value,
                        RedMedicalTonnage = t.HDCTonnageRedMedical!.Value,
                        AmberMedicalTonnage = t.HDCTonnageAmberMedical!.Value,
                        GreenMedicalTonnage = t.HDCTonnageGreenMedical!.Value
                    } : null,
                    HouseholdTonnageWithoutRAM = t.HouseholdTonnageWithoutRAM,
                    PublicBinTonnageWithoutRAM = t.PublicBinTonnageWithoutRAM,
                    HouseholdDrinksContainerTonnageWithoutRAM = t.HDCTonnageWithoutRAM,
                    ProjectedHouseholdTonnage = t.ProjectedHouseholdTonnage,
                    ProjectedHouseholdRAMTonnage = new RAMTonnage
                    {
                        RedTonnage = t.ProjectedHouseholdTonnageRed,
                        AmberTonnage = t.ProjectedHouseholdTonnageAmber,
                        GreenTonnage = t.ProjectedHouseholdTonnageGreen,
                        RedMedicalTonnage = t.ProjectedHouseholdTonnageRedMedical,
                        AmberMedicalTonnage = t.ProjectedHouseholdTonnageAmberMedical,
                        GreenMedicalTonnage = t.ProjectedHouseholdTonnageGreenMedical
                    },
                    ProjectedPublicBinTonnage = t.ProjectedPublicBinTonnage,
                    ProjectedPublicBinRAMTonnage = new RAMTonnage
                    {
                        RedTonnage = t.ProjectedPublicBinTonnageRed,
                        AmberTonnage = t.ProjectedPublicBinTonnageAmber,
                        GreenTonnage = t.ProjectedPublicBinTonnageGreen,
                        RedMedicalTonnage = t.ProjectedPublicBinTonnageRedMedical,
                        AmberMedicalTonnage = t.ProjectedPublicBinTonnageAmberMedical,
                        GreenMedicalTonnage = t.ProjectedPublicBinTonnageGreenMedical
                    },
                    ProjectedHouseholdDrinksContainerTonnage = t.ProjectedHDCTonnage,
                    ProjectedHouseholdDrinksContainerRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? new RAMTonnage
                    {
                        RedTonnage = t.ProjectedHDCTonnageRed!.Value,
                        AmberTonnage = t.ProjectedHDCTonnageAmber!.Value,
                        GreenTonnage = t.ProjectedHDCTonnageGreen!.Value,
                        RedMedicalTonnage = t.ProjectedHDCTonnageRedMedical!.Value,
                        AmberMedicalTonnage = t.ProjectedHDCTonnageAmberMedical!.Value,
                        GreenMedicalTonnage = t.ProjectedHDCTonnageGreenMedical!.Value
                    } : null
                }
            );
        }

        private static TransformProjectedH1 MapToTransformProjectedH1(int runId, int producerId, string? subsidiaryId, string materialCode, string submissionPeriod, string level, CalcResultH1ProjectedProducerMaterialTonnage tonnage)
        {
            return new TransformProjectedH1
            {
                CalculatorRunId = runId,
                ProducerId = producerId,
                SubsidiaryId = subsidiaryId,
                MaterialCode = materialCode,
                SubmissionPeriodCode = submissionPeriod,
                Level = level,
                HouseholdTonnage = tonnage.HouseholdTonnage,
                HouseholdTonnageRed = tonnage.HouseholdRAMTonnage.RedTonnage,
                HouseholdTonnageAmber = tonnage.HouseholdRAMTonnage.AmberTonnage,
                HouseholdTonnageGreen = tonnage.HouseholdRAMTonnage.GreenTonnage,
                HouseholdTonnageRedMedical = tonnage.HouseholdRAMTonnage.RedMedicalTonnage,
                HouseholdTonnageAmberMedical = tonnage.HouseholdRAMTonnage.AmberMedicalTonnage,
                HouseholdTonnageGreenMedical = tonnage.HouseholdRAMTonnage.GreenMedicalTonnage,
                PublicBinTonnage = tonnage.PublicBinTonnage,
                PublicBinTonnageRed = tonnage.PublicBinRAMTonnage.RedTonnage,
                PublicBinTonnageAmber = tonnage.PublicBinRAMTonnage.AmberTonnage,
                PublicBinTonnageGreen = tonnage.PublicBinRAMTonnage.GreenTonnage,
                PublicBinTonnageRedMedical = tonnage.PublicBinRAMTonnage.RedMedicalTonnage,
                PublicBinTonnageAmberMedical = tonnage.PublicBinRAMTonnage.AmberMedicalTonnage,
                PublicBinTonnageGreenMedical = tonnage.PublicBinRAMTonnage.GreenMedicalTonnage,
                HDCTonnage = tonnage.HouseholdDrinksContainerTonnage,
                HDCTonnageRed = tonnage.HouseholdDrinksContainerRAMTonnage?.RedTonnage,
                HDCTonnageAmber = tonnage.HouseholdDrinksContainerRAMTonnage?.AmberTonnage,
                HDCTonnageGreen = tonnage.HouseholdDrinksContainerRAMTonnage?.GreenTonnage,
                HDCTonnageRedMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage,
                HDCTonnageAmberMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage,
                HDCTonnageGreenMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage,
                HouseholdTonnageWithoutRAM = tonnage.HouseholdTonnageWithoutRAM,
                PublicBinTonnageWithoutRAM = tonnage.PublicBinTonnageWithoutRAM,
                HDCTonnageWithoutRAM = tonnage.HouseholdDrinksContainerTonnageWithoutRAM,
                ProjectedHouseholdTonnage = tonnage.ProjectedHouseholdTonnage,
                ProjectedHouseholdTonnageRed = tonnage.ProjectedHouseholdRAMTonnage.RedTonnage,
                ProjectedHouseholdTonnageAmber = tonnage.ProjectedHouseholdRAMTonnage.AmberTonnage,
                ProjectedHouseholdTonnageGreen = tonnage.ProjectedHouseholdRAMTonnage.GreenTonnage,
                ProjectedHouseholdTonnageRedMedical = tonnage.ProjectedHouseholdRAMTonnage.RedMedicalTonnage,
                ProjectedHouseholdTonnageAmberMedical = tonnage.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage,
                ProjectedHouseholdTonnageGreenMedical = tonnage.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage,
                ProjectedPublicBinTonnage = tonnage.ProjectedPublicBinTonnage,
                ProjectedPublicBinTonnageRed = tonnage.ProjectedPublicBinRAMTonnage.RedTonnage,
                ProjectedPublicBinTonnageAmber = tonnage.ProjectedPublicBinRAMTonnage.AmberTonnage,
                ProjectedPublicBinTonnageGreen = tonnage.ProjectedPublicBinRAMTonnage.GreenTonnage,
                ProjectedPublicBinTonnageRedMedical = tonnage.ProjectedPublicBinRAMTonnage.RedMedicalTonnage,
                ProjectedPublicBinTonnageAmberMedical = tonnage.ProjectedPublicBinRAMTonnage.AmberMedicalTonnage,
                ProjectedPublicBinTonnageGreenMedical = tonnage.ProjectedPublicBinRAMTonnage.GreenMedicalTonnage,
                ProjectedHDCTonnage = tonnage.ProjectedHouseholdDrinksContainerTonnage,
                ProjectedHDCTonnageRed = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.RedTonnage,
                ProjectedHDCTonnageAmber = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberTonnage,
                ProjectedHDCTonnageGreen = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenTonnage,
                ProjectedHDCTonnageRedMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage,
                ProjectedHDCTonnageAmberMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage,
                ProjectedHDCTonnageGreenMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage,
                H2RamProportionsRed = tonnage.H2RamProportions.Red,
                H2RamProportionsAmber = tonnage.H2RamProportions.Amber,
                H2RamProportionsGreen = tonnage.H2RamProportions.Green,
                H2RamProportionsRedMedical = tonnage.H2RamProportions.RedMedical,
                H2RamProportionsAmberMedical = tonnage.H2RamProportions.AmberMedical,
                H2RamProportionsGreenMedical = tonnage.H2RamProportions.GreenMedical
            };
        }

        private static TransformProjectedH2 MapToTransformProjectedH2(int runId, int producerId, string? subsidiaryId, string materialCode, string submissionPeriod, string level,CalcResultH2ProjectedProducerMaterialTonnage tonnage)
        {
            return new TransformProjectedH2
            {
                CalculatorRunId = runId,
                ProducerId = producerId,
                SubsidiaryId = subsidiaryId,
                MaterialCode = materialCode,
                Level = level,
                SubmissionPeriodCode = submissionPeriod,
                HouseholdTonnage = tonnage.HouseholdTonnage,
                HouseholdTonnageRed = tonnage.HouseholdRAMTonnage.RedTonnage,
                HouseholdTonnageAmber = tonnage.HouseholdRAMTonnage.AmberTonnage,
                HouseholdTonnageGreen = tonnage.HouseholdRAMTonnage.GreenTonnage,
                HouseholdTonnageRedMedical = tonnage.HouseholdRAMTonnage.RedMedicalTonnage,
                HouseholdTonnageAmberMedical = tonnage.HouseholdRAMTonnage.AmberMedicalTonnage,
                HouseholdTonnageGreenMedical = tonnage.HouseholdRAMTonnage.GreenMedicalTonnage,
                PublicBinTonnage = tonnage.PublicBinTonnage,
                PublicBinTonnageRed = tonnage.PublicBinRAMTonnage.RedTonnage,
                PublicBinTonnageAmber = tonnage.PublicBinRAMTonnage.AmberTonnage,
                PublicBinTonnageGreen = tonnage.PublicBinRAMTonnage.GreenTonnage,
                PublicBinTonnageRedMedical = tonnage.PublicBinRAMTonnage.RedMedicalTonnage,
                PublicBinTonnageAmberMedical = tonnage.PublicBinRAMTonnage.AmberMedicalTonnage,
                PublicBinTonnageGreenMedical = tonnage.PublicBinRAMTonnage.GreenMedicalTonnage,
                HDCTonnage = tonnage.HouseholdDrinksContainerTonnage,
                HDCTonnageRed = tonnage.HouseholdDrinksContainerRAMTonnage?.RedTonnage,
                HDCTonnageAmber = tonnage.HouseholdDrinksContainerRAMTonnage?.AmberTonnage,
                HDCTonnageGreen = tonnage.HouseholdDrinksContainerRAMTonnage?.GreenTonnage,
                HDCTonnageRedMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage,
                HDCTonnageAmberMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage,
                HDCTonnageGreenMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage,
                HouseholdTonnageWithoutRAM = tonnage.HouseholdTonnageWithoutRAM,
                PublicBinTonnageWithoutRAM = tonnage.PublicBinTonnageWithoutRAM,
                HDCTonnageWithoutRAM = tonnage.HouseholdDrinksContainerTonnageWithoutRAM,
                ProjectedHouseholdTonnage = tonnage.ProjectedHouseholdTonnage,
                ProjectedHouseholdTonnageRed = tonnage.ProjectedHouseholdRAMTonnage.RedTonnage,
                ProjectedHouseholdTonnageAmber = tonnage.ProjectedHouseholdRAMTonnage.AmberTonnage,
                ProjectedHouseholdTonnageGreen = tonnage.ProjectedHouseholdRAMTonnage.GreenTonnage,
                ProjectedHouseholdTonnageRedMedical = tonnage.ProjectedHouseholdRAMTonnage.RedMedicalTonnage,
                ProjectedHouseholdTonnageAmberMedical = tonnage.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage,
                ProjectedHouseholdTonnageGreenMedical = tonnage.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage,
                ProjectedPublicBinTonnage = tonnage.ProjectedPublicBinTonnage,
                ProjectedPublicBinTonnageRed = tonnage.ProjectedPublicBinRAMTonnage.RedTonnage,
                ProjectedPublicBinTonnageAmber = tonnage.ProjectedPublicBinRAMTonnage.AmberTonnage,
                ProjectedPublicBinTonnageGreen = tonnage.ProjectedPublicBinRAMTonnage.GreenTonnage,
                ProjectedPublicBinTonnageRedMedical = tonnage.ProjectedPublicBinRAMTonnage.RedMedicalTonnage,
                ProjectedPublicBinTonnageAmberMedical = tonnage.ProjectedPublicBinRAMTonnage.AmberMedicalTonnage,
                ProjectedPublicBinTonnageGreenMedical = tonnage.ProjectedPublicBinRAMTonnage.GreenMedicalTonnage,
                ProjectedHDCTonnage = tonnage.ProjectedHouseholdDrinksContainerTonnage,
                ProjectedHDCTonnageRed = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.RedTonnage,
                ProjectedHDCTonnageAmber = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberTonnage,
                ProjectedHDCTonnageGreen = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenTonnage,
                ProjectedHDCTonnageRedMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage,
                ProjectedHDCTonnageAmberMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage,
                ProjectedHDCTonnageGreenMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage
            };
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

        private static TransformPartial MapToTransformPartial(int runId, string materialCode, CalcResultPartialObligation producer, CalcResultPartialObligationTonnage tonnage)
        {
            return new TransformPartial
            {
                CalculatorRunId = runId,
                ProducerId = producer.ProducerId,
                SubsidiaryId = producer.SubsidiaryId,
                ProducerName = producer.ProducerName,
                TradingName = producer.TradingName,
                Level = producer.Level,
                SubmissionYear = producer.SubmissionYear,
                DaysInSubmissionYear = producer.DaysInSubmissionYear,
                JoiningDate = producer.JoiningDate,
                DaysObligated = producer.DaysObligated,
                ObligatedFactor = producer.ObligatedFactor,
                MaterialCode = materialCode,
                HouseholdTonnage = tonnage.HouseholdTonnage,
                HouseholdTonnageRed = tonnage.HouseholdRAMTonnage?.RedTonnage,
                HouseholdTonnageAmber = tonnage.HouseholdRAMTonnage?.AmberTonnage,
                HouseholdTonnageGreen = tonnage.HouseholdRAMTonnage?.GreenTonnage,
                HouseholdTonnageRedMedical = tonnage.HouseholdRAMTonnage?.RedMedicalTonnage,
                HouseholdTonnageAmberMedical = tonnage.HouseholdRAMTonnage?.AmberMedicalTonnage,
                HouseholdTonnageGreenMedical = tonnage.HouseholdRAMTonnage?.GreenMedicalTonnage,
                PublicBinTonnage = tonnage.PublicBinTonnage,
                PublicBinTonnageRed = tonnage.PublicBinRAMTonnage?.RedTonnage,
                PublicBinTonnageAmber = tonnage.PublicBinRAMTonnage?.AmberTonnage,
                PublicBinTonnageGreen = tonnage.PublicBinRAMTonnage?.GreenTonnage,
                PublicBinTonnageRedMedical = tonnage.PublicBinRAMTonnage?.RedMedicalTonnage,
                PublicBinTonnageAmberMedical = tonnage.PublicBinRAMTonnage?.AmberMedicalTonnage,
                PublicBinTonnageGreenMedical = tonnage.PublicBinRAMTonnage?.GreenMedicalTonnage,
                HDCTonnage = tonnage.HouseholdDrinksContainersTonnage,
                HDCTonnageRed = tonnage.HouseholdDrinksContainersRAMTonnage?.RedTonnage,
                HDCTonnageAmber = tonnage.HouseholdDrinksContainersRAMTonnage?.AmberTonnage,
                HDCTonnageGreen = tonnage.HouseholdDrinksContainersRAMTonnage?.GreenTonnage,
                HDCTonnageRedMedical = tonnage.HouseholdDrinksContainersRAMTonnage?.RedMedicalTonnage,
                HDCTonnageAmberMedical = tonnage.HouseholdDrinksContainersRAMTonnage?.AmberMedicalTonnage,
                HDCTonnageGreenMedical = tonnage.HouseholdDrinksContainersRAMTonnage?.GreenMedicalTonnage,
                SMCWTonnage = tonnage.SelfManagedConsumerWasteTonnage
            };
        }

        private static Dictionary<string, CalcResultPartialObligationTonnage> MapToPartial(List<TransformPartial> partial)
        {
            RAMTonnage? ToMaybeRamTonnage(
                decimal? red,
                decimal? amber,
                decimal? green,
                decimal? redMedical,
                decimal? amberMedical,
                decimal? greenMedical)
            {
                return red is null && amber is null && green is null && redMedical is null && amberMedical is null && greenMedical is null
                    ? null
                    : new RAMTonnage
                    {
                        RedTonnage = red!.Value,
                        AmberTonnage = amber!.Value,
                        GreenTonnage = green!.Value,
                        RedMedicalTonnage = redMedical!.Value,
                        AmberMedicalTonnage = amberMedical!.Value,
                        GreenMedicalTonnage = greenMedical!.Value
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
