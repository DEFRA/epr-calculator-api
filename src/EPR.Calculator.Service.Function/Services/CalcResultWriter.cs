using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Services
{
    public interface ICalcResultWriter
    {
        Task StoreProjectedH1Data(int runId, IReadOnlyList<CalcResultH1ProjectedProducer> projectedProducers, CancellationToken cancellationToken);
        Task StoreProjectedH2Data(int runId, IReadOnlyList<CalcResultH2ProjectedProducer> projectedProducers, CancellationToken cancellationToken);
        Task StoreScaledData(int runId, IReadOnlyList<CalcResultScaledupProducer> scaled, CancellationToken cancellationToken);
        Task StorePartialData(int runId, IReadOnlyList<CalcResultPartialObligation> partial, CancellationToken cancellationToken);
        Task StoreProducerMaterialPackaging(List<L1Producer> producerDetails, CancellationToken cancellationToken);
        Task StoreProducerFees(int runId, ProducerFees producerFees, CancellationToken cancellationToken);
        Task StoreSmcw(int runId, SelfManagedConsumerWaste smcw, CancellationToken cancellationToken);
        Task StoreModulationResult(int runId, ModulationResult modulation, CancellationToken cancellationToken);
        Task StoreLapcapData(int runId, CalcResultLapcapData lapcapData, CancellationToken cancellationToken);
        Task StoreCommsCost(int runId, CalcResultCommsCost commsCost, CancellationToken cancellationToken);
        Task StoreLateReportingTonnage(int runId, CalcResultLateReportingTonnage lateReportingTonnage, CancellationToken cancellationToken);
        Task StoreParameterOtherCost(int runId, CalcResultParameterOtherCost parameterOtherCost, CancellationToken cancellationToken);
        Task StoreOnePlusFourApportionment(int runId, CalcResultOnePlusFourApportionment onePlusFourApportionment, CancellationToken cancellationToken);
        Task StoreLaDisposalCostData(int runId, CalcResultLaDisposalCostData laDisposalCostData, CancellationToken cancellationToken);
        Task StoreCancelledProducers(int runId, IReadOnlyList<CalcResultCancelledProducer> cancelledProducers, CancellationToken cancellationToken);
    }

    public class CalcResultWriter(IBulkOperations bulkOps, ApplicationDBContext dbContext) : ICalcResultWriter
    {
        public async Task StoreProjectedH1Data(int runId, IReadOnlyList<CalcResultH1ProjectedProducer> projectedProducers, CancellationToken cancellationToken)
        {
            await bulkOps.BulkInsertAsync(dbContext, projectedProducers.SelectMany(p => 
                p.H1ProjectedTonnageByMaterial.Select(m => 
                    MapToTransformProjectedH1(runId, p.ProducerId, p.SubsidiaryId, m.Key, p.SubmissionPeriodCode, p.Level, m.Value)
                )
            ), cancellationToken);
        }

        public async Task StoreProjectedH2Data(int runId, IReadOnlyList<CalcResultH2ProjectedProducer> projectedProducers, CancellationToken cancellationToken)
        {
            await bulkOps.BulkInsertAsync(dbContext, projectedProducers.SelectMany(p => 
                p.H2ProjectedTonnageByMaterial.Select(m => 
                    MapToTransformProjectedH2(runId, p.ProducerId, p.SubsidiaryId, m.Key, p.SubmissionPeriodCode, p.Level, m.Value)
                )
            ), cancellationToken);
        }

        public async Task StoreScaledData(int runId, IReadOnlyList<CalcResultScaledupProducer> scaled, CancellationToken cancellationToken)
        {
             await bulkOps.BulkInsertAsync(dbContext, scaled.SelectMany(p => 
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
            ), cancellationToken);
        }
        
        public async Task StorePartialData(int runId, IReadOnlyList<CalcResultPartialObligation> partial, CancellationToken cancellationToken){
            await bulkOps.BulkInsertAsync(dbContext, partial.SelectMany(p => 
                p.PartialObligationTonnageByMaterial.Select(m => 
                    MapToTransformPartial(runId, m.Key, p, m.Value)
                )
            ), cancellationToken);
        }

        public async Task StoreProducerMaterialPackaging(List<L1Producer> producerDetails, CancellationToken cancellationToken)
        {
            await bulkOps.BulkInsertAsync(dbContext, producerDetails
                    .SelectMany(p => p.Producers)
                    .SelectMany(p => p.ProducerReportedMaterials.Select(rm =>
                        new ProducerMaterialPackaging
                        {
                            ProducerDetailId             = rm.ProducerDetailId,
                            MaterialId                   = rm.MaterialId,
                            SubmissionPeriod             = rm.SubmissionPeriod,
                            PackagingType                = rm.PackagingType,
                            PackagingTonnage             = rm.PackagingTonnage,
                            PackagingTonnageRed          = rm.PackagingTonnageRed,
                            PackagingTonnageAmber        = rm.PackagingTonnageAmber,
                            PackagingTonnageGreen        = rm.PackagingTonnageGreen,
                            PackagingTonnageRedMedical   = rm.PackagingTonnageRedMedical,
                            PackagingTonnageAmberMedical = rm.PackagingTonnageAmberMedical,
                            PackagingTonnageGreenMedical = rm.PackagingTonnageGreenMedical
                        }
                    )
                ).ToList(), cancellationToken);
        }

        public async Task StoreProducerFees(int runId, ProducerFees producerFees, CancellationToken cancellationToken)
        {
            dbContext.ProducerDisposalFee.Add(producerFees);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task StoreSmcw(int runId, SelfManagedConsumerWaste smcw, CancellationToken cancellationToken)
        {
            dbContext.SelfManagedConsumerWaste.Add(smcw);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task StoreModulationResult(int runId, ModulationResult modulation, CancellationToken cancellationToken)
        {
            dbContext.ModulationResult.Add(modulation);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task StoreLapcapData(int runId, CalcResultLapcapData lapcapData, CancellationToken cancellationToken)
        {
            dbContext.LapcapData.Add(new CalcResultLapcapDataEntry { CalculatorRunId = runId, LapcapData = lapcapData });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task StoreCommsCost(int runId, CalcResultCommsCost commsCost, CancellationToken cancellationToken)
        {
            dbContext.CommCost.Add(new CalcResultCommsCostEntry { CalculatorRunId = runId, CommsCost = commsCost });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task StoreLateReportingTonnage(int runId, CalcResultLateReportingTonnage lateReportingTonnage, CancellationToken cancellationToken)
        {
            dbContext.LateReportingTonnage.Add(new CalcResultLateReportingTonnageEntry { CalculatorRunId = runId, LateReportingTonnage = lateReportingTonnage });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task StoreParameterOtherCost(int runId, CalcResultParameterOtherCost parameterOtherCost, CancellationToken cancellationToken)
        {
            dbContext.ParameterOtherCost.Add(new CalcResultParameterOtherCostEntry { CalculatorRunId = runId, ParameterOtherCost = parameterOtherCost });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task StoreOnePlusFourApportionment(int runId, CalcResultOnePlusFourApportionment onePlusFourApportionment, CancellationToken cancellationToken)
        {
            dbContext.OnePlusFourApportionment.Add(new CalcResultOnePlusFourApportionmentEntry { CalculatorRunId = runId, OnePlusFourApportionment = onePlusFourApportionment });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task StoreLaDisposalCostData(int runId, CalcResultLaDisposalCostData laDisposalCostData, CancellationToken cancellationToken)
        {
            dbContext.LaDisposalCostData.Add(new CalcResultLaDisposalCostDataEntry { CalculatorRunId = runId, LaDisposalCost = laDisposalCostData });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task StoreCancelledProducers(int runId, IReadOnlyList<CalcResultCancelledProducer> cancelledProducers, CancellationToken cancellationToken) {
            dbContext.CancelledProducers.AddRange(cancelledProducers.Select(c => new CalcResultCancelledProducerEntry { CalculatorRunId = runId, CancelledProducer = c }));
            await dbContext.SaveChangesAsync(cancellationToken);
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
                HouseholdTonnageRed = tonnage.HouseholdRAMTonnage.Red,
                HouseholdTonnageAmber = tonnage.HouseholdRAMTonnage.Amber,
                HouseholdTonnageGreen = tonnage.HouseholdRAMTonnage.Green,
                HouseholdTonnageRedMedical = tonnage.HouseholdRAMTonnage.RedMedical,
                HouseholdTonnageAmberMedical = tonnage.HouseholdRAMTonnage.AmberMedical,
                HouseholdTonnageGreenMedical = tonnage.HouseholdRAMTonnage.GreenMedical,
                PublicBinTonnage = tonnage.PublicBinTonnage,
                PublicBinTonnageRed = tonnage.PublicBinRAMTonnage.Red,
                PublicBinTonnageAmber = tonnage.PublicBinRAMTonnage.Amber,
                PublicBinTonnageGreen = tonnage.PublicBinRAMTonnage.Green,
                PublicBinTonnageRedMedical = tonnage.PublicBinRAMTonnage.RedMedical,
                PublicBinTonnageAmberMedical = tonnage.PublicBinRAMTonnage.AmberMedical,
                PublicBinTonnageGreenMedical = tonnage.PublicBinRAMTonnage.GreenMedical,
                HDCTonnage = tonnage.HouseholdDrinksContainerTonnage,
                HDCTonnageRed = tonnage.HouseholdDrinksContainerRAMTonnage?.Red,
                HDCTonnageAmber = tonnage.HouseholdDrinksContainerRAMTonnage?.Amber,
                HDCTonnageGreen = tonnage.HouseholdDrinksContainerRAMTonnage?.Green,
                HDCTonnageRedMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.RedMedical,
                HDCTonnageAmberMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.AmberMedical,
                HDCTonnageGreenMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.GreenMedical,
                HouseholdTonnageWithoutRAM = tonnage.HouseholdTonnageWithoutRAM,
                PublicBinTonnageWithoutRAM = tonnage.PublicBinTonnageWithoutRAM,
                HDCTonnageWithoutRAM = tonnage.HouseholdDrinksContainerTonnageWithoutRAM,
                ProjectedHouseholdTonnage = tonnage.ProjectedHouseholdTonnage,
                ProjectedHouseholdTonnageRed = tonnage.ProjectedHouseholdRAMTonnage.Red,
                ProjectedHouseholdTonnageAmber = tonnage.ProjectedHouseholdRAMTonnage.Amber,
                ProjectedHouseholdTonnageGreen = tonnage.ProjectedHouseholdRAMTonnage.Green,
                ProjectedHouseholdTonnageRedMedical = tonnage.ProjectedHouseholdRAMTonnage.RedMedical,
                ProjectedHouseholdTonnageAmberMedical = tonnage.ProjectedHouseholdRAMTonnage.AmberMedical,
                ProjectedHouseholdTonnageGreenMedical = tonnage.ProjectedHouseholdRAMTonnage.GreenMedical,
                ProjectedPublicBinTonnage = tonnage.ProjectedPublicBinTonnage,
                ProjectedPublicBinTonnageRed = tonnage.ProjectedPublicBinRAMTonnage.Red,
                ProjectedPublicBinTonnageAmber = tonnage.ProjectedPublicBinRAMTonnage.Amber,
                ProjectedPublicBinTonnageGreen = tonnage.ProjectedPublicBinRAMTonnage.Green,
                ProjectedPublicBinTonnageRedMedical = tonnage.ProjectedPublicBinRAMTonnage.RedMedical,
                ProjectedPublicBinTonnageAmberMedical = tonnage.ProjectedPublicBinRAMTonnage.AmberMedical,
                ProjectedPublicBinTonnageGreenMedical = tonnage.ProjectedPublicBinRAMTonnage.GreenMedical,
                ProjectedHDCTonnage = tonnage.ProjectedHouseholdDrinksContainerTonnage,
                ProjectedHDCTonnageRed = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.Red,
                ProjectedHDCTonnageAmber = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.Amber,
                ProjectedHDCTonnageGreen = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.Green,
                ProjectedHDCTonnageRedMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedical,
                ProjectedHDCTonnageAmberMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedical,
                ProjectedHDCTonnageGreenMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedical,
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
                HouseholdTonnageRed = tonnage.HouseholdRAMTonnage.Red,
                HouseholdTonnageAmber = tonnage.HouseholdRAMTonnage.Amber,
                HouseholdTonnageGreen = tonnage.HouseholdRAMTonnage.Green,
                HouseholdTonnageRedMedical = tonnage.HouseholdRAMTonnage.RedMedical,
                HouseholdTonnageAmberMedical = tonnage.HouseholdRAMTonnage.AmberMedical,
                HouseholdTonnageGreenMedical = tonnage.HouseholdRAMTonnage.GreenMedical,
                PublicBinTonnage = tonnage.PublicBinTonnage,
                PublicBinTonnageRed = tonnage.PublicBinRAMTonnage.Red,
                PublicBinTonnageAmber = tonnage.PublicBinRAMTonnage.Amber,
                PublicBinTonnageGreen = tonnage.PublicBinRAMTonnage.Green,
                PublicBinTonnageRedMedical = tonnage.PublicBinRAMTonnage.RedMedical,
                PublicBinTonnageAmberMedical = tonnage.PublicBinRAMTonnage.AmberMedical,
                PublicBinTonnageGreenMedical = tonnage.PublicBinRAMTonnage.GreenMedical,
                HDCTonnage = tonnage.HouseholdDrinksContainerTonnage,
                HDCTonnageRed = tonnage.HouseholdDrinksContainerRAMTonnage?.Red,
                HDCTonnageAmber = tonnage.HouseholdDrinksContainerRAMTonnage?.Amber,
                HDCTonnageGreen = tonnage.HouseholdDrinksContainerRAMTonnage?.Green,
                HDCTonnageRedMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.RedMedical,
                HDCTonnageAmberMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.AmberMedical,
                HDCTonnageGreenMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.GreenMedical,
                HouseholdTonnageWithoutRAM = tonnage.HouseholdTonnageWithoutRAM,
                PublicBinTonnageWithoutRAM = tonnage.PublicBinTonnageWithoutRAM,
                HDCTonnageWithoutRAM = tonnage.HouseholdDrinksContainerTonnageWithoutRAM,
                ProjectedHouseholdTonnage = tonnage.ProjectedHouseholdTonnage,
                ProjectedHouseholdTonnageRed = tonnage.ProjectedHouseholdRAMTonnage.Red,
                ProjectedHouseholdTonnageAmber = tonnage.ProjectedHouseholdRAMTonnage.Amber,
                ProjectedHouseholdTonnageGreen = tonnage.ProjectedHouseholdRAMTonnage.Green,
                ProjectedHouseholdTonnageRedMedical = tonnage.ProjectedHouseholdRAMTonnage.RedMedical,
                ProjectedHouseholdTonnageAmberMedical = tonnage.ProjectedHouseholdRAMTonnage.AmberMedical,
                ProjectedHouseholdTonnageGreenMedical = tonnage.ProjectedHouseholdRAMTonnage.GreenMedical,
                ProjectedPublicBinTonnage = tonnage.ProjectedPublicBinTonnage,
                ProjectedPublicBinTonnageRed = tonnage.ProjectedPublicBinRAMTonnage.Red,
                ProjectedPublicBinTonnageAmber = tonnage.ProjectedPublicBinRAMTonnage.Amber,
                ProjectedPublicBinTonnageGreen = tonnage.ProjectedPublicBinRAMTonnage.Green,
                ProjectedPublicBinTonnageRedMedical = tonnage.ProjectedPublicBinRAMTonnage.RedMedical,
                ProjectedPublicBinTonnageAmberMedical = tonnage.ProjectedPublicBinRAMTonnage.AmberMedical,
                ProjectedPublicBinTonnageGreenMedical = tonnage.ProjectedPublicBinRAMTonnage.GreenMedical,
                ProjectedHDCTonnage = tonnage.ProjectedHouseholdDrinksContainerTonnage,
                ProjectedHDCTonnageRed = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.Red,
                ProjectedHDCTonnageAmber = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.Amber,
                ProjectedHDCTonnageGreen = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.Green,
                ProjectedHDCTonnageRedMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedical,
                ProjectedHDCTonnageAmberMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedical,
                ProjectedHDCTonnageGreenMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedical
            };
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
                HouseholdTonnageRed = tonnage.HouseholdRAMTonnage?.Red,
                HouseholdTonnageAmber = tonnage.HouseholdRAMTonnage?.Amber,
                HouseholdTonnageGreen = tonnage.HouseholdRAMTonnage?.Green,
                HouseholdTonnageRedMedical = tonnage.HouseholdRAMTonnage?.RedMedical,
                HouseholdTonnageAmberMedical = tonnage.HouseholdRAMTonnage?.AmberMedical,
                HouseholdTonnageGreenMedical = tonnage.HouseholdRAMTonnage?.GreenMedical,
                PublicBinTonnage = tonnage.PublicBinTonnage,
                PublicBinTonnageRed = tonnage.PublicBinRAMTonnage?.Red,
                PublicBinTonnageAmber = tonnage.PublicBinRAMTonnage?.Amber,
                PublicBinTonnageGreen = tonnage.PublicBinRAMTonnage?.Green,
                PublicBinTonnageRedMedical = tonnage.PublicBinRAMTonnage?.RedMedical,
                PublicBinTonnageAmberMedical = tonnage.PublicBinRAMTonnage?.AmberMedical,
                PublicBinTonnageGreenMedical = tonnage.PublicBinRAMTonnage?.GreenMedical,
                HDCTonnage = tonnage.HouseholdDrinksContainersTonnage,
                HDCTonnageRed = tonnage.HouseholdDrinksContainersRAMTonnage?.Red,
                HDCTonnageAmber = tonnage.HouseholdDrinksContainersRAMTonnage?.Amber,
                HDCTonnageGreen = tonnage.HouseholdDrinksContainersRAMTonnage?.Green,
                HDCTonnageRedMedical = tonnage.HouseholdDrinksContainersRAMTonnage?.RedMedical,
                HDCTonnageAmberMedical = tonnage.HouseholdDrinksContainersRAMTonnage?.AmberMedical,
                HDCTonnageGreenMedical = tonnage.HouseholdDrinksContainersRAMTonnage?.GreenMedical,
                SMCWTonnage = tonnage.SelfManagedConsumerWasteTonnage
            };
        }
    }
}
