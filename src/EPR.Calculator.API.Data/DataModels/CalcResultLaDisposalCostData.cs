using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using EPR.Calculator.API.Data.Utils;

namespace EPR.Calculator.API.Data.DataModels;

public class CalcResultLaDisposalCostDataEntry
{
    public int Id { get; set; }
    public required int CalculatorRunId { get; set; }
    public required CalcResultLaDisposalCostData LaDisposalCost { get; set; }

}

public class CalcResultLaDisposalCostData
{
    internal ICollection<MaterialLaDisposalCostData> MaterialCosts { get; set; } = [];

    [NotMapped]
    public required IReadOnlyDictionary<string, CalcResultLaDisposalCostDataDetail> ByMaterial
    {
        get => MaterialCosts.ToDictionary(x => x.MaterialCode, x => x.Detail);
        init => MaterialCosts = [..value.Select(kvp =>
            new MaterialLaDisposalCostData { MaterialCode = kvp.Key, Detail = kvp.Value })];
    }

    private CalcResultLaDisposalCostDataDetail? total;
    public CalcResultLaDisposalCostDataDetail Total =>
        total ??=
            new CalcResultLaDisposalCostDataDetail
            {
                Cost                                    = ByCountryCost.Sum(ByMaterial.Values.Select(v => v.Cost).ToImmutableList()),
                // TODO why do we sum up tonnage for different materials?
                HouseholdPackagingWasteTonnage          = ByMaterial.Values.Sum(v => v.HouseholdPackagingWasteTonnage),
                PublicBinTonnage                        = ByMaterial.Values.Sum(v => v.PublicBinTonnage),
                HouseholdDrinkContainersTonnage         = ByMaterial.Values.Sum(v => v.HouseholdDrinkContainersTonnage),
                LateReportingTonnage                    = ByMaterial.Values.Sum(v => v.LateReportingTonnage),
                ActionedSelfManagedConsumerWasteTonnage = ByMaterial.Values.Sum(v => v.ActionedSelfManagedConsumerWasteTonnage ?? 0)
            };
}

internal record MaterialLaDisposalCostData
{
    public required string MaterialCode { get; set; }
    public required CalcResultLaDisposalCostDataDetail Detail { get; set; }
}

public class CalcResultLaDisposalCostDataDetail
{
    public required ByCountryCost Cost { get; init; }

    public required decimal HouseholdPackagingWasteTonnage { get; init; }

    public required decimal PublicBinTonnage { get; init; }

    public required decimal HouseholdDrinkContainersTonnage { get; init; }

    public decimal LateReportingTonnage { get; init; }

    // This will be null for Pre-Modulation - i.e. isn't part of the calculation
    public decimal? ActionedSelfManagedConsumerWasteTonnage { get; init; }

    private decimal? totalTonnage;
    public decimal TotalTonnage =>
        totalTonnage ??=
            LateReportingTonnage
                + HouseholdPackagingWasteTonnage
                + PublicBinTonnage
                + HouseholdDrinkContainersTonnage
                - (ActionedSelfManagedConsumerWasteTonnage ?? 0);

    private decimal? disposalCostPricePerTonne;
    public decimal? DisposalCostPricePerTonne =>
        disposalCostPricePerTonne ??=
            TotalTonnage == 0 ? (decimal?)null : MathUtils.RoundAwayFromZero(Cost.Total / TotalTonnage, 4);
}
