using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Builder.Modulation
{
    public interface ICalcResultModulationBuilder
    {
        Task<ModulationResult> ConstructAsync(
            RunContext runContext,
            IImmutableList<MaterialDetail> materials,
            CalcResultLaDisposalCostData laDisposalCostData,
            SelfManagedConsumerWaste smcw
        );
    }

    public class CalcResultModulationBuilder : ICalcResultModulationBuilder
    {
        public Task<ModulationResult> ConstructAsync(
            RunContext runContext,
            IImmutableList<MaterialDetail> materials,
            CalcResultLaDisposalCostData laDisposalCostData,
            SelfManagedConsumerWaste smcw
        )
        {
            var redFactor = runContext.DefaultParameters.RedModulationFactor;

            decimal pricePerTonne(MaterialDetail material)
            {
                return laDisposalCostData.ByMaterial[material.Code].DisposalCostPricePerTonne ?? 0m;
            }

            var materialCosts =
                materials.Select(material =>
                {
                    var materialDisposalCost  = pricePerTonne(material);
                    var netReportedTonnage    = smcw.TotalByMaterial[material.Code].NetTonnage;
                    var lateReportingTonnageR = runContext.DefaultParameters.LateReportingTonnageByMaterialCode[material.Code].Red;
                    var lateReportingTonnageA = runContext.DefaultParameters.LateReportingTonnageByMaterialCode[material.Code].Amber;
                    var lateReportingTonnageG = runContext.DefaultParameters.LateReportingTonnageByMaterialCode[material.Code].Green;
                    var redMaterialTonnages   = lateReportingTonnageR + netReportedTonnage.Red   ?? 0m;
                    var amberMaterialTonnages = lateReportingTonnageA + netReportedTonnage.Amber ?? 0m;
                    var greenMaterialTonnages = lateReportingTonnageG + netReportedTonnage.Green ?? 0m;
                    return new
                    {
                        material = material,
                        amberMaterialDisposalCost = materialDisposalCost,
                        redMaterialTonnages       = redMaterialTonnages,
                        amberMaterialTonnages     = amberMaterialTonnages,
                        greenMaterialTonnages     = greenMaterialTonnages,
                        redAtAmberDisposalCost    = MathUtils.RoundAwayFromZero(redMaterialTonnages   * materialDisposalCost, 2),
                        greenAtAmberDisposalCost  = MathUtils.RoundAwayFromZero(greenMaterialTonnages * materialDisposalCost, 2)
                    };
                }).ToImmutableList();

            var totalRedAtAmberDisposalCost  = materialCosts.Sum(c => c.redAtAmberDisposalCost);
            var totalGreenAtAmberDispoalCost = materialCosts.Sum(c => c.greenAtAmberDisposalCost);
            var greenDiscount =
                totalGreenAtAmberDispoalCost == 0
                    ? 0m // this is unlikely, but if happens then the green discount is moot
                    : (redFactor - 1) * totalRedAtAmberDisposalCost / totalGreenAtAmberDispoalCost;
            var greenFactor = MathUtils.RoundAwayFromZero(1 - greenDiscount, 6);
            var materialModulations =
                materials.ToDictionary(
                    material => material,
                    material =>
                {
                    var cost = materialCosts.First(c => c.material == material);
                    return new ModulationDetail
                    {
                        RedMaterialDisposalCost               = MathUtils.RoundAwayFromZero(cost.amberMaterialDisposalCost * redFactor  , 4),
                        AmberMaterialDisposalCost             = MathUtils.RoundAwayFromZero(cost.amberMaterialDisposalCost              , 4),
                        GreenMaterialDisposalCost             = MathUtils.RoundAwayFromZero(cost.amberMaterialDisposalCost * greenFactor, 4),
                        RedMaterialTonnages                   = cost.redMaterialTonnages,
                        AmberMaterialTonnages                 = cost.amberMaterialTonnages,
                        GreenMaterialTonnages                 = cost.greenMaterialTonnages,
                        TotalRedMaterialAtAmberDisposalCost   = cost.redAtAmberDisposalCost,
                        TotalGreenMaterialAtAmberDisposalCost = cost.greenAtAmberDisposalCost
                    };
                });

            return Task.FromResult(new ModulationResult
            {
                CalculatorRunId      = smcw.CalculatorRunId,
                GreenFactor          = greenFactor,
                RedFactor            = redFactor,
                ModulationByMaterial = materialModulations
            });
        }
    }
}
