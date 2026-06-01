namespace EPR.Calculator.Service.Function.Constants
{
    public static class CalcResultPartialObligationHeaders
    {
        public static readonly string PartialObligations = "Partial Obligation Calculation";
        public static readonly string ProducerId = "Producer ID";
        public static readonly string SubsidiaryId = "Subsidiary ID";
        public static readonly string ProducerOrSubsidiaryName = "Producer / Subsidiary Name";
        public static readonly string TradingName = "Trading Name";
        public static readonly string Level = "Level";
        public static readonly string SubmissionYear = "Submission year";
        public static readonly string DaysInSubmissionYear = "Days in submission year";
        public static readonly string JoiningDate = "Joining date";
        public static readonly string ObligatedDays = "Obligated days";
        public static readonly string ObligatedPercentage = "Obligated %";
        public static readonly string HouseholdPackagingWasteTonnage = "Household Packaging Tonnage";
        public static readonly string HouseholdRedTonnage = MaterialTonnageHeader("Household", "Red");
        public static readonly string HouseholdAmberTonnage = MaterialTonnageHeader("Household", "Amber");
        public static readonly string HouseholdGreenTonnage = MaterialTonnageHeader("Household", "Green");
        public static readonly string HouseholdRedMedicalTonnage = MaterialTonnageHeader("Household", "Red Medical");
        public static readonly string HouseholdAmberMedicalTonnage = MaterialTonnageHeader("Household", "Amber Medical");
        public static readonly string HouseholdGreenMedicalTonnage = MaterialTonnageHeader("Household", "Green Medical");
        public static readonly string PublicBinTonnage = "Public Bin Tonnage";
        public static readonly string PublicBinRedTonnage = MaterialTonnageHeader("Public Bin", "Red");
        public static readonly string PublicBinAmberTonnage = MaterialTonnageHeader("Public Bin", "Amber");
        public static readonly string PublicBinGreenTonnage = MaterialTonnageHeader("Public Bin", "Green");
        public static readonly string PublicBinRedMedicalTonnage = MaterialTonnageHeader("Public Bin", "Red Medical");
        public static readonly string PublicBinAmberMedicalTonnage = MaterialTonnageHeader("Public Bin", "Amber Medical");
        public static readonly string PublicBinGreenMedicalTonnage = MaterialTonnageHeader("Public Bin", "Green Medical");
        public static readonly string HouseholdDrinksContainersTonnage = "Household Drinks Containers Tonnage";
        public static readonly string HouseholdDrinksContainersRedTonnage = MaterialTonnageHeader("Household Drinks Containers", "Red");
        public static readonly string HouseholdDrinksContainersAmberTonnage = MaterialTonnageHeader("Household Drinks Containers", "Amber");
        public static readonly string HouseholdDrinksContainersGreenTonnage = MaterialTonnageHeader("Household Drinks Containers", "Green");
        public static readonly string HouseholdDrinksContainersRedMedicalTonnage = MaterialTonnageHeader("Household Drinks Containers", "Red Medical");
        public static readonly string HouseholdDrinksContainersAmberMedicalTonnage = MaterialTonnageHeader("Household Drinks Containers", "Amber Medical");
        public static readonly string HouseholdDrinksContainersGreenMedicalTonnage = MaterialTonnageHeader("Household Drinks Containers", "Green Medical");
        public static readonly string TotalTonnage = "Total Tonnage";
        public static readonly string SelfManagedConsumerWasteTonnage = "Self Managed Consumer Waste Tonnage";
        public static readonly string PartialHouseholdPackagingWasteTonnage = "Partial Household Packaging Tonnage";
        public static readonly string PartialHouseholdRedTonnage = MaterialTonnageHeader("Partial Household", "Red");
        public static readonly string PartialHouseholdAmberTonnage = MaterialTonnageHeader("Partial Household", "Amber");
        public static readonly string PartialHouseholdGreenTonnage = MaterialTonnageHeader("Partial Household", "Green");
        public static readonly string PartialHouseholdRedMedicalTonnage = MaterialTonnageHeader("Partial Household", "Red Medical");
        public static readonly string PartialHouseholdAmberMedicalTonnage = MaterialTonnageHeader("Partial Household", "Amber Medical");
        public static readonly string PartialHouseholdGreenMedicalTonnage = MaterialTonnageHeader("Partial Household", "Green Medical");
        public static readonly string PartialPublicBinTonnage = "Partial Public Bin Tonnage";
        public static readonly string PartialPublicBinRedTonnage = MaterialTonnageHeader("Partial Public Bin", "Red");
        public static readonly string PartialPublicBinAmberTonnage = MaterialTonnageHeader("Partial Public Bin", "Amber");
        public static readonly string PartialPublicBinGreenTonnage = MaterialTonnageHeader("Partial Public Bin", "Green");
        public static readonly string PartialPublicBinRedMedicalTonnage = MaterialTonnageHeader("Partial Public Bin", "Red Medical");
        public static readonly string PartialPublicBinAmberMedicalTonnage = MaterialTonnageHeader("Partial Public Bin", "Amber Medical");
        public static readonly string PartialPublicBinGreenMedicalTonnage = MaterialTonnageHeader("Partial Public Bin", "Green Medical");
        public static readonly string PartialHouseholdDrinksContainersTonnage = "Partial Household Drinks Containers Tonnage";
        public static readonly string PartialHouseholdDrinksContainersRedTonnage = MaterialTonnageHeader("Partial Household Drinks Containers", "Red");
        public static readonly string PartialHouseholdDrinksContainersAmberTonnage = MaterialTonnageHeader("Partial Household Drinks Containers", "Amber");
        public static readonly string PartialHouseholdDrinksContainersGreenTonnage = MaterialTonnageHeader("Partial Household Drinks Containers", "Green");
        public static readonly string PartialHouseholdDrinksContainersRedMedicalTonnage = MaterialTonnageHeader("Partial Household Drinks Containers", "Red Medical");
        public static readonly string PartialHouseholdDrinksContainersAmberMedicalTonnage = MaterialTonnageHeader("Partial Household Drinks Containers", "Amber Medical");
        public static readonly string PartialHouseholdDrinksContainersGreenMedicalTonnage = MaterialTonnageHeader("Partial Household Drinks Containers", "Green Medical");
        public static readonly string PartialTotalTonnage = "Partial Total Tonnage";
        public static readonly string PartialSelfManagedConsumerWasteTonnage = "Partial Self Managed Consumer Waste Tonnage";
        public static readonly string NoPartialObligations = "None";
        private static string MaterialTonnageHeader(string type, string RAM) { return $"{type} {RAM} Material Tonnage"; }
    }
}

