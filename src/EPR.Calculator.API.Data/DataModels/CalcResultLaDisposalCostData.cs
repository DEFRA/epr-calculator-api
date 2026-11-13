using System.ComponentModel.DataAnnotations.Schema;

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

    public required CalcResultLaDisposalCostDataDetail Total { get; init; }
}

internal record MaterialLaDisposalCostData
{
    public required string MaterialCode { get; set; }
    public required CalcResultLaDisposalCostDataDetail Detail { get; set; }
}

public record CalcResultLaDisposalCostDataDetail
{
    public required ByCountryCost Cost { get; init; }

    public required decimal HouseholdPackagingWasteTonnage { get; init; }

    public required decimal PublicBinTonnage { get; init; }

    public required decimal HouseholdDrinkContainersTonnage { get; init; }

    public decimal LateReportingTonnage { get; init; }

    // This will be null for Pre-Modulation - i.e. isn't part of the calculation
    public decimal? ActionedSelfManagedConsumerWasteTonnage { get; init; }

    public decimal TotalTonnage { get; init; }

    public decimal? DisposalCostPricePerTonne { get; init; }
}
