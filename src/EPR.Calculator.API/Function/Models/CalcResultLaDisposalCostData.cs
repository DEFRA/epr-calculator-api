namespace EPR.Calculator.Service.Function.Models;

public class CalcResultLaDisposalCostData
{
    public required Dictionary<string, CalcResultLaDisposalCostDataDetail> ByMaterial { get; init; }

    private CalcResultLaDisposalCostDataDetail? total;
    public CalcResultLaDisposalCostDataDetail Total =>
        total ??=
            new CalcResultLaDisposalCostDataDetail
            {
                Cost                                    = ByCountryCost.Sum(ByMaterial.Values.Select(v => v.Cost)),
                // TODO why do we sum up tonnage for different materials?
                HouseholdPackagingWasteTonnage          = ByMaterial.Values.Sum(v => v.HouseholdPackagingWasteTonnage),
                PublicBinTonnage                        = ByMaterial.Values.Sum(v => v.PublicBinTonnage),
                HouseholdDrinkContainersTonnage         = ByMaterial.Values.Sum(v => v.HouseholdDrinkContainersTonnage),
                LateReportingTonnage                    = ByMaterial.Values.Sum(v => v.LateReportingTonnage),
                ActionedSelfManagedConsumerWasteTonnage = ByMaterial.Values.Sum(v => v.ActionedSelfManagedConsumerWasteTonnage ?? 0)
            };
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
            TotalTonnage == 0 ? (decimal?)null : Math.Round(Cost.Total / TotalTonnage, 4);
}
