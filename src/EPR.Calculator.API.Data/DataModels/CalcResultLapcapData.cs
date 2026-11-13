using System.ComponentModel.DataAnnotations.Schema;

namespace EPR.Calculator.API.Data.DataModels;

public class CalcResultLapcapDataEntry
{
    public int Id { get; set; }
    public required int CalculatorRunId { get; set; }
    public required CalcResultLapcapData LapcapData { get; set; }
}

public class CalcResultLapcapData
{
    internal ICollection<MaterialLapcapData> MaterialCosts { get; set; } = [];

    [NotMapped]
    public required IReadOnlyDictionary<string, ByCountryCost> ByMaterial
    {
        get => MaterialCosts.ToDictionary(x => x.MaterialCode, x => x.Cost);
        init => MaterialCosts = [..value.Select(kvp =>
            new MaterialLapcapData { MaterialCode = kvp.Key, Cost = kvp.Value })];
    }

    public required ByCountryCost Total { get; init; }

    public required ByCountryApportionment CountryApportionment { get; init; }
}

internal record MaterialLapcapData
{
    public required string MaterialCode { get; set; }
    public required ByCountryCost Cost { get; set; }
}