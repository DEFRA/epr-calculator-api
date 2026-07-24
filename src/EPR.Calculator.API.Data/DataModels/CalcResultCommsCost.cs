using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using EPR.Calculator.API.Data.Utils;

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

    private CalcResultCommsCostCommsCostByMaterial? total;
    public CalcResultCommsCostCommsCostByMaterial Total =>
        total ??=
            new CalcResultCommsCostCommsCostByMaterial
            {
                Cost                             = ByCountryCost.Sum(ByMaterial.Values.Select(v => v.Cost).ToImmutableList()),
                TotalCost                        = ByMaterial.Values.Sum(v => v.TotalCost),
                // TODO why do we sum up tonnage for different materials?
                HouseholdPackagingWasteTonnage   = ByMaterial.Values.Sum(v => v.HouseholdPackagingWasteTonnage),
                PublicBinTonnage                 = ByMaterial.Values.Sum(v => v.PublicBinTonnage),
                HouseholdDrinksContainersTonnage = ByMaterial.Values.Sum(v => v.HouseholdDrinksContainersTonnage),
                LateReportingTonnage             = ByMaterial.Values.Sum(v => v.LateReportingTonnage)
            };

    public required ByCountryCost CommsCostUkWide { get; set; }

    public required ByCountryCost CommsCostByCountry { get; set; }
}

public class CalcResultCommsCostCommsCostByMaterial
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

    private decimal? totalTonnage;
    public decimal TotalTonnage =>
        totalTonnage ??=
            HouseholdPackagingWasteTonnage
            + LateReportingTonnage
            + PublicBinTonnage
            + HouseholdDrinksContainersTonnage;

    private decimal? pricePerTonne;
    public decimal PricePerTonne =>
        pricePerTonne ??=
            TotalTonnage != 0 ? MathUtils.RoundAwayFromZero(TotalCost / TotalTonnage, 4) : 0;
}

internal record MaterialCommsCostData
{
    public required string MaterialCode { get; set; }
    public required CalcResultCommsCostCommsCostByMaterial CommsCost { get; set; }
}
