using System.ComponentModel.DataAnnotations.Schema;

namespace EPR.Calculator.API.Data.DataModels;

public class CalcResultCommsCostEntry
{
    public int Id { get; set; }
    public required int CalculatorRunId { get; set; }
    public required CalcResultCommsCost CommsCost { get; set; }
}

public class CalcResultCommsCost
{
    public required ByCountryApportionment OnePlusFourApportionment { get; set; }

    internal ICollection<MaterialCommsCostData> MaterialCosts { get; set; } = [];

    [NotMapped]
    public required IReadOnlyDictionary<string, CalcResultCommsCostCommsCostByMaterial> ByMaterial
    {
        get => MaterialCosts.ToDictionary(x => x.MaterialCode, x => x.CommsCost);
        init => MaterialCosts = [..value.Select(kvp =>
            new MaterialCommsCostData { MaterialCode = kvp.Key, CommsCost = kvp.Value })];
    }

    public required CalcResultCommsCostCommsCostByMaterial Total { get; init; }

    public required ByCountryCost CommsCostUkWide { get; set; }

    public required ByCountryCost CommsCostByCountry { get; set; }
}

public record CalcResultCommsCostCommsCostByMaterial
{
    public required ByCountryCost Cost {get; set;}

    /// <summary>
    /// The pre-apportionment total cost for this material. Used for PricePerTonne so that
    /// floating-point rounding across the four country splits does not alter the result.
    /// </summary>
    public required decimal TotalCost { get; set; }

    public required decimal HouseholdPackagingWasteTonnage { get; set; }
    public required decimal PublicBinTonnage { get; set; }
    public required decimal HouseholdDrinksContainersTonnage { get; set; }
    public required decimal LateReportingTonnage { get; set; }

    public decimal TotalTonnage { get; init; }

    public decimal PricePerTonne { get; init; }
}

internal record MaterialCommsCostData
{
    public required string MaterialCode { get; set; }
    public required CalcResultCommsCostCommsCostByMaterial CommsCost { get; set; }
}
