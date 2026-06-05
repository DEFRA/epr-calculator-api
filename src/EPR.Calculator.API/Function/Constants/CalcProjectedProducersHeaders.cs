namespace EPR.Calculator.Service.Function.Constants
{
    public static class CalcResultProjectedProducersHeaders
    {
        public static readonly string H2ProjectedProducers = "H2 Packaging Data - Submitted & Projected";
        public static readonly string H1ProjectedProducers = "H1 Packaging Data - Submitted & Projected";
        public static readonly string ProducerId = "Producer ID";
        public static readonly string SubsidiaryId = "Subsidiary ID";
        public static readonly string Level = "Level";
        public static readonly string SubmissionPeriodCode = "Submission period code";
        public static readonly string HouseholdPackagingTonnage = "Household Packaging Tonnage";
        public static readonly string HouseholdRedTonnage = MaterialTonnageHeader("Household", "Red");
        public static readonly string HouseholdAmberTonnage = MaterialTonnageHeader("Household", "Amber");
        public static readonly string HouseholdGreenTonnage = MaterialTonnageHeader("Household", "Green");
        public static readonly string HouseholdRedMedicalTonnage = MaterialTonnageHeader("Household", "Red Medical");
        public static readonly string HouseholdAmberMedicalTonnage = MaterialTonnageHeader("Household", "Amber Medical");
        public static readonly string HouseholdGreenMedicalTonnage = MaterialTonnageHeader("Household", "Green Medical");
        public static readonly string PublicBinPackagingTonnage = "Public Bin Packaging Tonnage";
        public static readonly string PublicBinRedTonnage = MaterialTonnageHeader("Public Bin", "Red");
        public static readonly string PublicBinAmberTonnage = MaterialTonnageHeader("Public Bin", "Amber");
        public static readonly string PublicBinGreenTonnage = MaterialTonnageHeader("Public Bin", "Green");
        public static readonly string PublicBinRedMedicalTonnage = MaterialTonnageHeader("Public Bin", "Red Medical");
        public static readonly string PublicBinAmberMedicalTonnage = MaterialTonnageHeader("Public Bin", "Amber Medical");
        public static readonly string PublicBinGreenMedicalTonnage = MaterialTonnageHeader("Public Bin", "Green Medical");
        public static readonly string HouseholdDrinksContainersPackagingTonnage = "Household Drinks Containers Tonnage";
        public static readonly string HouseholdDrinksContainersRedTonnage = MaterialTonnageHeader("Household Drinks Containers", "Red");
        public static readonly string HouseholdDrinksContainersAmberTonnage = MaterialTonnageHeader("Household Drinks Containers", "Amber");
        public static readonly string HouseholdDrinksContainersGreenTonnage = MaterialTonnageHeader("Household Drinks Containers", "Green");
        public static readonly string HouseholdDrinksContainersRedMedicalTonnage = MaterialTonnageHeader("Household Drinks Containers", "Red Medical");
        public static readonly string HouseholdDrinksContainersAmberMedicalTonnage = MaterialTonnageHeader("Household Drinks Containers", "Amber Medical");
        public static readonly string HouseholdDrinksContainersGreenMedicalTonnage = MaterialTonnageHeader("Household Drinks Containers", "Green Medical");
        public static readonly string HouseholdTonnageWithoutRAMDefaultedToRed = "Household Tonnage Without RAM(Defaulted to Red)";
        public static readonly string PublicBinTonnageWithoutRAMDefaultedToRed = "Public Bin Tonnage Without RAM(Defaulted to Red)";
        public static readonly string HouseholdDrinksContainersTonnageWithoutRAMDefaultedToRed = "Household Drinks Container Tonnage Without RAM(Defaulted to Red)";
        public static readonly string HouseholdTonnageWithoutRAM = "Household Tonnage Without RAM";
        public static readonly string PublicBinTonnageWithoutRAM = "Public Bin Tonnage Without RAM";
        public static readonly string HouseholdDrinksContainersTonnageWithoutRAM = "Household Drinks Container Tonnage Without RAM";
        public static readonly string RedH2MaterialTonnageProportion = "H2 Red Material %";
        public static readonly string AmberH2MaterialTonnageProportion = "H2 Amber Material %";
        public static readonly string GreenH2MaterialTonnageProportion = "H2 Green Material %";
        public static readonly string RedMedicalH2MaterialTonnageProportion = "H2 Red Medical Material %";
        public static readonly string AmberMedicalH2MaterialTonnageProportion = "H2 Amber Medical Material %";
        public static readonly string GreenMedicalH2MaterialTonnageProportion = "H2 Green Medical Material %";
        public static readonly string TotalTonnage = "Total Tonnage";
        public static readonly string NoProjectedProducers = "None";

        private static string MaterialTonnageHeader(string type, string RAM) { return $"{type} {RAM} Material Tonnage"; }
    }
}
