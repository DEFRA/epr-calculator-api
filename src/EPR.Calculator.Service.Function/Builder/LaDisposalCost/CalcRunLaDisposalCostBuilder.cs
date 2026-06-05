using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.LaDisposalCost
{
    public interface ICalcRunLaDisposalCostBuilder
    {
        Task<CalcResultLaDisposalCostData> ConstructAsync(
            RunContext runContext,
            IEnumerable<MaterialDetail> materialDetails,
            CalcResultLapcapData lapcapData,
            CalcResultLateReportingTonnage lateReportingTonnage,
            SelfManagedConsumerWaste smcw
        );
    }

    public class CalcRunLaDisposalCostBuilder : ICalcRunLaDisposalCostBuilder
    {
        private readonly ApplicationDBContext context;
        private List<ProducerData> producerData;

        internal class ProducerData
        {
            public required string MaterialName { get; set; }

            public required string MaterialCode { get; set; }

            public required string PackagingType { get; set; }

            public decimal Tonnage { get; set; }

            public required ProducerDetail? ProducerDetail { get; set; }
        }

        public CalcRunLaDisposalCostBuilder(ApplicationDBContext context)
        {
            this.context = context;
            this.producerData = new List<ProducerData>();
        }

        public async Task<CalcResultLaDisposalCostData> ConstructAsync(
            RunContext runContext,
            IEnumerable<MaterialDetail> materialDetails,
            CalcResultLapcapData lapcapData,
            CalcResultLateReportingTonnage lateReportingTonnage,
            SelfManagedConsumerWaste smcw
        )
        {
            producerData = await GetProducerData(runContext);

            var lapcapDetailsByMaterial =
                lapcapData.ByMaterial.Select(detail =>
                {
                    var materialCode = detail.Key;
                    var materialName = materialDetails.First(m => m.Code == materialCode).Name;
                    var lateReportingTonnageValue = lateReportingTonnage.ByMaterial[materialCode].Total;
                    var hhTonnage  = producerData.Where(t => t.MaterialName == materialName && t.PackagingType == PackagingTypes.Household).Sum(t => t.Tonnage);
                    var pbTonnage  = producerData.Where(p => p.MaterialName == materialName && p.PackagingType == PackagingTypes.PublicBin).Sum(p => p.Tonnage);
                    var hdcTonnage = producerData.Where(p => p.MaterialName == materialName && p.PackagingType == PackagingTypes.HouseholdDrinksContainers).Sum(p => p.Tonnage);
                    decimal? actionedSelfManagedConsumerWasteTonnage = runContext.RequiresModulation ? smcw.OverallTotalPerMaterials[materialCode].ActionedSelfManagedConsumerWasteTonnage.total ?? 0 : null;
                    var laDisposalDetail = new CalcResultLaDisposalCostDataDetail
                    {
                        Cost                                    = detail.Value,
                        HouseholdPackagingWasteTonnage          = hhTonnage,
                        PublicBinTonnage                        = pbTonnage,
                        HouseholdDrinkContainersTonnage         = hdcTonnage,
                        LateReportingTonnage                    = lateReportingTonnageValue,
                        ActionedSelfManagedConsumerWasteTonnage = actionedSelfManagedConsumerWasteTonnage,
                    };
                    return (detail.Key, laDisposalDetail);
                }).ToDictionary();

            return new CalcResultLaDisposalCostData
            {
                ByMaterial = lapcapDetailsByMaterial
            };
        }

        private async Task<List<ProducerData>> GetProducerData(RunContext runContext)
        {
            // TODO note returns duplicates for SubmissionPeriod - should remove it from ProducerReportedMaterialProjected - it's not needed
            // TODO why filter PackagingType/Material? should already be done
            return await (
                from run in context.CalculatorRuns
                join producerDetail in context.ProducerDetail on run.Id equals producerDetail.CalculatorRunId
                join producerMaterial in context.ProducerReportedMaterialProjected on producerDetail.Id equals producerMaterial.ProducerDetailId
                join material in context.Material on producerMaterial.MaterialId equals material.Id
                where run.Id == runContext.RunId &&
                    producerMaterial.PackagingType != null &&
                    (
                        producerMaterial.PackagingType == PackagingTypes.Household ||
                        producerMaterial.PackagingType == PackagingTypes.PublicBin ||
                        (
                            producerMaterial.PackagingType == PackagingTypes.HouseholdDrinksContainers &&
                            material.Code == MaterialCodes.Glass
                        )
                    )
                select new ProducerData
                {
                    MaterialName = material.Name,
                    MaterialCode = material.Code,
                    PackagingType = producerMaterial.PackagingType,
                    Tonnage = producerMaterial.PackagingTonnage,
                    ProducerDetail = producerMaterial.ProducerDetail,
                }
            ).ToListAsync();
        }
    }
}
