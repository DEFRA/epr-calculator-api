namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultPartialObligationTonnage
    {
        public decimal HouseholdTonnage { get; set; }
        public RAMTonnage? HouseholdRAMTonnage { get; set; }
        public decimal PublicBinTonnage { get; set; }
        public RAMTonnage? PublicBinRAMTonnage { get; set; }
        public decimal? HouseholdDrinksContainersTonnage { get; set; }
        public RAMTonnage? HouseholdDrinksContainersRAMTonnage { get; set; }
        public decimal TotalTonnage { get; set; }
        public decimal SelfManagedConsumerWasteTonnage { get; set; }
        public decimal PartialHouseholdTonnage { get; set; }
        public RAMTonnage? PartialHouseholdRAMTonnage { get; set; }
        public decimal PartialPublicBinTonnage { get; set; }
        public RAMTonnage? PartialPublicBinRAMTonnage { get; set; }
        public decimal? PartialHouseholdDrinksContainersTonnage { get; set; }
        public RAMTonnage? PartialHouseholdDrinksContainersRAMTonnage { get; set; }
        public decimal PartialTotalTonnage { get; set; }
        public decimal PartialSelfManagedConsumerWasteTonnage { get; set; }
    }
}

