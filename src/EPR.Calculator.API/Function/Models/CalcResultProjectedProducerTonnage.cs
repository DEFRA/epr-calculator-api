namespace EPR.Calculator.Service.Function.Models
{
    public record RAMProportions
    {
        public decimal Red { get; init; }
        public decimal Amber { get; init; }
        public decimal Green { get; init; }
        public decimal RedMedical { get; init; }
        public decimal AmberMedical { get; init; }
        public decimal GreenMedical { get; init; }

        public bool AnyProportions()
        {
            return Red > 0 || Amber > 0 || Green > 0 || RedMedical > 0 || AmberMedical > 0 || GreenMedical > 0;
        }
    }

    public abstract record CalcResultProjectedProducerMaterialTonnage
    {
        public required decimal HouseholdTonnage { get; init; }
        public required RAMTonnage HouseholdRAMTonnage { get; init; }
        public required decimal PublicBinTonnage { get; init; }
        public required RAMTonnage PublicBinRAMTonnage { get; init; }
        public decimal? HouseholdDrinksContainerTonnage { get; init; }
        public RAMTonnage? HouseholdDrinksContainerRAMTonnage { get; init; }
        public required decimal HouseholdTonnageWithoutRAM { get; init; }
        public required decimal PublicBinTonnageWithoutRAM { get; init; }
        public decimal? HouseholdDrinksContainerTonnageWithoutRAM { get; init; }
        public required decimal ProjectedHouseholdTonnage { get; init; }
        public required RAMTonnage ProjectedHouseholdRAMTonnage { get; init; }
        public required decimal ProjectedPublicBinTonnage { get; init; }
        public required RAMTonnage ProjectedPublicBinRAMTonnage { get; init; }
        public decimal? ProjectedHouseholdDrinksContainerTonnage { get; init; }
        public RAMTonnage? ProjectedHouseholdDrinksContainerRAMTonnage { get; init; }
        public required decimal TotalTonnage { get; init; }

        private decimal GetTotalProjectedRamTonnage(Func<RAMTonnage, decimal> getTonnage)
        {
            var hdcTonnage = ProjectedHouseholdDrinksContainerRAMTonnage != null ? getTonnage(ProjectedHouseholdDrinksContainerRAMTonnage) : 0;
            return getTonnage(ProjectedHouseholdRAMTonnage) + getTonnage(ProjectedPublicBinRAMTonnage) + hdcTonnage;
        }
        public decimal GetTotalProjectedRedTonnage(){ return GetTotalProjectedRamTonnage(t => t.RedTonnage);}
        public decimal GetTotalProjectedAmberTonnage() { return GetTotalProjectedRamTonnage(t => t.AmberTonnage); }
        public decimal GetTotalProjectedGreenTonnage() { return GetTotalProjectedRamTonnage(t => t.GreenTonnage); }
        public decimal GetTotalProjectedRedMedicalTonnage() { return GetTotalProjectedRamTonnage(t => t.RedMedicalTonnage); }
        public decimal GetTotalProjectedAmberMedicalTonnage() { return GetTotalProjectedRamTonnage(t => t.AmberMedicalTonnage); }
        public decimal GetTotalProjectedGreenMedicalTonnage() { return GetTotalProjectedRamTonnage(t => t.GreenMedicalTonnage); }

        public bool IsWithoutRamTonnage()
        {
            return HouseholdTonnageWithoutRAM > 0 || PublicBinTonnageWithoutRAM > 0 || (HouseholdDrinksContainerTonnageWithoutRAM ?? 0) > 0;
        }
    }

    public record CalcResultH2ProjectedProducerMaterialTonnage : CalcResultProjectedProducerMaterialTonnage {}

    public record CalcResultH1ProjectedProducerMaterialTonnage : CalcResultProjectedProducerMaterialTonnage
    {
        public required RAMProportions H2RamProportions { get; init; }
    }
}
