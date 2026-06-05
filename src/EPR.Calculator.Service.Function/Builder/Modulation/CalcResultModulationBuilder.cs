using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Builder.Modulation
{
    public record MaterialModulation
    {
        public required decimal RedMaterialDisposalCost { get; init; }
        public required decimal AmberMaterialDisposalCost { get; init; }
        public required decimal GreenMaterialDisposalCost { get; init; }
        public required decimal RedMaterialTonnages { get; init; }
        public required decimal AmberMaterialTonnages { get; init; }
        public required decimal GreenMaterialTonnages { get; init; }
        public required decimal TotalRedMaterialAtAmberDisposalCost { get; init; }
        public required decimal TotalGreenMaterialAtAmberDisposalCost { get; init; }
    }

    public record ModulationResult
    {
        public required decimal GreenFactor { get; init; }
        public required decimal RedFactor { get; init; }
        public required IReadOnlyDictionary<MaterialDetail, MaterialModulation> MaterialModulation { get; init; }
    }

    public interface ICalcResultModulationBuilder
    {
        Task<ModulationResult> ConstructAsync(
            IReadOnlyDictionary<string, decimal> defaultParams,
            IImmutableList<MaterialDetail> materials,
            CalcResultLaDisposalCostData laDisposalCostData,
            SelfManagedConsumerWaste smcw
        );
    }

    public class CalcResultModulationBuilder : ICalcResultModulationBuilder
    {
        public Task<ModulationResult> ConstructAsync(
            IReadOnlyDictionary<string, decimal> defaultParams,
            IImmutableList<MaterialDetail> materials,
            CalcResultLaDisposalCostData laDisposalCostData,
            SelfManagedConsumerWaste smcw
        )
        {
            var redFactor = defaultParams["REDM-RF"];

            decimal pricePerTonne(MaterialDetail material)
            {
                return laDisposalCostData.ByMaterial[material.Code].DisposalCostPricePerTonne ?? 0m;
            }

            var materialCosts =
                materials.Select(material =>
                {
                    var materialDisposalCost = pricePerTonne(material);
                    var netReportedTonnage = smcw.OverallTotalPerMaterials[material.Code].NetReportedTonnage;
                    var lateReportingTonnageR = GetLateReportingTonnage(defaultParams, material, RagRating.Red);
                    var lateReportingTonnageA = GetLateReportingTonnage(defaultParams, material, RagRating.Amber);
                    var lateReportingTonnageG = GetLateReportingTonnage(defaultParams, material, RagRating.Green);
                    var redMaterialTonnages   = lateReportingTonnageR + netReportedTonnage.red   ?? 0m;
                    var amberMaterialTonnages = lateReportingTonnageA + netReportedTonnage.amber ?? 0m;
                    var greenMaterialTonnages = lateReportingTonnageG + netReportedTonnage.green ?? 0m;
                    return new
                    {
                        material = material,
                        amberMaterialDisposalCost = materialDisposalCost,
                        redMaterialTonnages   = redMaterialTonnages,
                        amberMaterialTonnages = amberMaterialTonnages,
                        greenMaterialTonnages = greenMaterialTonnages,
                        redAtAmberDisposalCost   = Math.Round(redMaterialTonnages   * materialDisposalCost, 2),
                        greenAtAmberDisposalCost = Math.Round(greenMaterialTonnages * materialDisposalCost, 2)
                    };
                }).ToImmutableList();

            var totalRedAtAmberDisposalCost  = materialCosts.Sum(c => c.redAtAmberDisposalCost);
            var totalGreenAtAmberDispoalCost = materialCosts.Sum(c => c.greenAtAmberDisposalCost);
            var greenDiscount =
                totalGreenAtAmberDispoalCost == 0
                    ? 0m // this is unlikely, but if happens then the green discount is moot
                    : (redFactor - 1) * totalRedAtAmberDisposalCost / totalGreenAtAmberDispoalCost;
            var greenFactor = Math.Round(1 - greenDiscount, 6);
            var materialModulations =
                materials.ToDictionary(
                    material => material,
                    material =>
                {
                    var cost = materialCosts.First(c => c.material == material);
                    return new MaterialModulation
                    {
                        RedMaterialDisposalCost   = Math.Round(cost.amberMaterialDisposalCost * redFactor  , 4),
                        AmberMaterialDisposalCost = Math.Round(cost.amberMaterialDisposalCost              , 4),
                        GreenMaterialDisposalCost = Math.Round(cost.amberMaterialDisposalCost * greenFactor, 4),
                        RedMaterialTonnages       = cost.redMaterialTonnages,
                        AmberMaterialTonnages     = cost.amberMaterialTonnages,
                        GreenMaterialTonnages     = cost.greenMaterialTonnages,
                        TotalRedMaterialAtAmberDisposalCost   = cost.redAtAmberDisposalCost,
                        TotalGreenMaterialAtAmberDisposalCost = cost.greenAtAmberDisposalCost
                    };
                });

            return Task.FromResult(new ModulationResult
            {
                GreenFactor = greenFactor,
                RedFactor   = redFactor,
                MaterialModulation = materialModulations
            });
        }

        private decimal GetLateReportingTonnage(IReadOnlyDictionary<string, decimal> defaultParams, MaterialDetail material, RagRating ragRating)
        {
            var rag = ragRating switch
            {
                RagRating.Red   => "-R",
                RagRating.Amber => "",
                RagRating.Green => "-G",
                _ => throw new ArgumentException("Invalid RagRating")
            };
            return defaultParams[$"LRET-{material.Code}{rag}"];
        }
    }
}
