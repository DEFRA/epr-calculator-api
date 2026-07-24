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

    private ByCountryCost? total;
    public ByCountryCost Total =>
        total ??=
            new ByCountryCost
            {
                England         = ByMaterial.Values.Sum(x => x.England),
                NorthernIreland = ByMaterial.Values.Sum(x => x.NorthernIreland),
                Scotland        = ByMaterial.Values.Sum(x => x.Scotland),
                Wales           = ByMaterial.Values.Sum(x => x.Wales)
            };

    private ByCountryApportionment? countryApportionment;
    public ByCountryApportionment CountryApportionment =>
        countryApportionment ??=
            Total.Total == 0
            ? ByCountryApportionment.Empty
            : new ByCountryApportionment
            {
                England         = Total.England         / Total.Total * 100,
                Wales           = Total.Wales           / Total.Total * 100,
                Scotland        = Total.Scotland        / Total.Total * 100,
                NorthernIreland = Total.NorthernIreland / Total.Total * 100
            };
}

internal record MaterialLapcapData
{
    public required string MaterialCode { get; set; }
    public required ByCountryCost Cost { get; set; }
}