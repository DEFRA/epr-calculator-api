namespace EPR.Calculator.Service.Function.Models;

/// <summary>
/// The CommsCost report.
/// </summary>
public class CalcResultCommsCost
{
    public required ByCountryApportionment OnePlusFourApportionment { get; init; }

    public required Dictionary<string, CalcResultCommsCostCommsCostByMaterial> ByMaterial { get; init; }
        = [];

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

    public required ByCountryCost CommsCostUkWide { get; init; }

    public required ByCountryCost CommsCostByCountry { get; init; }
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
            TotalTonnage != 0 ? Math.Round(TotalCost / TotalTonnage, 4, MidpointRounding.AwayFromZero) : 0;
}
