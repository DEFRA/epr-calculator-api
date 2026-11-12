using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Models
{
    public class CalcResultPartialObligationTonnage
    {
        public required decimal ObligatedFactor { get; set; }
        public required decimal HouseholdTonnage { get; set; }
        public RamTonnage? HouseholdRAMTonnage { get; set; }
        public required decimal PublicBinTonnage { get; set; }
        public RamTonnage? PublicBinRAMTonnage { get; set; }
        public decimal? HouseholdDrinksContainersTonnage { get; set; }
        public RamTonnage? HouseholdDrinksContainersRAMTonnage { get; set; }
        public required decimal SelfManagedConsumerWasteTonnage { get; set; }
        public required decimal PartialHouseholdTonnage { get; init; }
        public RamTonnage? PartialHouseholdRAMTonnage { get; init; }
        public required decimal PartialPublicBinTonnage { get; init; }
        public RamTonnage? PartialPublicBinRAMTonnage { get; init; }
        public decimal? PartialHouseholdDrinksContainersTonnage { get; init; }
        public RamTonnage? PartialHouseholdDrinksContainersRAMTonnage { get; init; }
        public required decimal PartialSelfManagedConsumerWasteTonnage { get; init; }
        public decimal TotalTonnage()
        {
            return HouseholdTonnage + PublicBinTonnage + (HouseholdDrinksContainersTonnage ?? 0);
        }
        public decimal PartialTotalTonnage()
        {
            return PartialHouseholdTonnage + PartialPublicBinTonnage + (PartialHouseholdDrinksContainersTonnage ?? 0);
        }
    }
}

