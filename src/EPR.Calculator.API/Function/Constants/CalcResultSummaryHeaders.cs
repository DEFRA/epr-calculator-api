namespace EPR.Calculator.Service.Function.Constants
{
    public static class CalcResultSummaryHeaders
    {
        public static readonly string CalculationResult = "Calculation Result";
        public static readonly string Notes = "NOTE: Rows with 'Scaled-up tonnages?' = " +
            "Yes include reported tonnages for a period that have been scaled-up to a full 6 month equivalent period. " +
            "See 'Scaled-up Producers' table for details.";

        public static readonly string OneProducerDisposalFeesWithBadDebtProvision =
            "1 Producer Disposal Fees with Bad Debt Provision";

        public static readonly string CommsCostHeader = "2a Fees for Comms Costs - by Material with Bad Debt provision";
        public static readonly string CommsCostSummaryHeader = "Summary of Fee for Comms Costs - by Material";


        public static readonly string DisposalFeeSummary = "Disposal Fee Summary";
        public static readonly string ProducerId = "Producer ID";
        public static readonly string SubsidiaryId = "Subsidiary ID";
        public static readonly string ProducerOrSubsidiaryName = "Producer / Subsidiary Name";
        public static readonly string TradingName = "Trading Name";
        public static readonly string Level = "Level";
        public static readonly string ScaledupTonnages = "Scaled-up tonnages?";
        public static readonly string PartialCalculation = "Partial Calculation?";
        public static readonly string StatusCode = "Registration Status Code";
        public static readonly string JoinersDate = "Joiners Date";
        public static readonly string LeaversDate = "Leavers Date";

        public static readonly string PreviousInvoicedTonnage = "Previous Invoiced Tonnage";
        public static readonly string TonnageChange = "Tonnage Change";

        public static readonly string HouseholdPackagingWasteTonnage = "Household Packaging Tonnage";
        public static readonly string HouseholdPackagingWasteTonnageRed = "Household Red Material Tonnage";
        public static readonly string HouseholdPackagingWasteTonnageAmber = "Household Amber Material Tonnage";
        public static readonly string HouseholdPackagingWasteTonnageGreen = "Household Green Material Tonnage";
        public static readonly string HouseholdPackagingWasteTonnageRedMedical = "Household Red Medical Material Tonnage";
        public static readonly string HouseholdPackagingWasteTonnageAmberMedical = "Household Amber Medical Material Tonnage";
        public static readonly string HouseholdPackagingWasteTonnageGreenMedical = "Household Green Medical Material Tonnage";

        public static readonly string PublicBinTonnage = "Public Bin Tonnage";
        public static readonly string PublicBinTonnageRed = "Public Bin Red Material Tonnage";
        public static readonly string PublicBinTonnageAmber = "Public Bin Amber Material Tonnage";
        public static readonly string PublicBinTonnageGreen = "Public Bin Green Material Tonnage";
        public static readonly string PublicBinTonnageRedMedical = "Public Bin Red Medical Material Tonnage";
        public static readonly string PublicBinTonnageAmberMedical = "Public Bin Amber Medical Material Tonnage";
        public static readonly string PublicBinTonnageGreenMedical = "Public Bin Green Medical Material Tonnage";

        public static readonly string HouseholdDrinksContainersTonnage = "Household Drinks Containers Tonnage - Glass";
        public static readonly string HouseholdDrinksContainersTonnageRed = "Household Drinks Containers Red Material Tonnage";
        public static readonly string HouseholdDrinksContainersTonnageAmber = "Household Drinks Containers Amber Material Tonnage";
        public static readonly string HouseholdDrinksContainersTonnageGreen = "Household Drinks Containers Green Material Tonnage";
        public static readonly string HouseholdDrinksContainersTonnageRedMedical = "Household Drinks Containers Red Medical Material Tonnage";
        public static readonly string HouseholdDrinksContainersTonnageAmberMedical = "Household Drinks Containers Amber Medical Material Tonnage";
        public static readonly string HouseholdDrinksContainersTonnageGreenMedical = "Household Drinks Containers Green Medical Material Tonnage";

        public static readonly string TotalTonnage = "Total Tonnage";
        public static readonly string RedTotalTonnage = "Red Total Tonnage";
        public static readonly string AmberTotalTonnage = "Amber Total Tonnage";
        public static readonly string GreenTotalTonnage = "Green Total Tonnage";
        public static readonly string RedMedicalTotalTonnage = "Red Medical Total Tonnage";
        public static readonly string AmberMedicalTotalTonnage = "Amber Medical Total Tonnage";
        public static readonly string GreenMedicalTotalTonnage = "Green Medical Total Tonnage";

        public static readonly string RedPlusRedMedicalTotalTonnage = "Red + Red Medical Total Tonnage";
        public static readonly string AmberPlusAmberMedicalTotalTonnage = "Amber + Amber Medical Total Tonnage";
        public static readonly string GreenPlusGreenMedicalTotalTonnage = "Green + Green Medical Total Tonnage";
        public static readonly string SelfManagedConsumerWasteTonnage = "Self Managed Consumer Waste Tonnage";

        public static readonly string ActionedSelfManagedConsumerWasteTonnage = "Actioned Self Managed Consumer Waste Tonnage";
        public static readonly string RedActionedSelfManagedConsumerWasteTonnage = "Red + Red Medical Actioned Self Managed Consumer Waste Tonnage";
        public static readonly string AmberActionedSelfManagedConsumerWasteTonnage = "Amber + Amber Medical Actioned Self Managed Consumer Waste Tonnage";
        public static readonly string GreenActionedSelfManagedConsumerWasteTonnage = "Green + Green Medical Actioned Self Managed Consumer Waste Tonnage";

        public static readonly string ResidualSelfManagedConsumerWasteTonnage = "Residual SMCW";

        public static readonly string NetTonnage = "Net Tonnage";
        public static readonly string RedPlusRedMedicalNetTonnage = "Red + Red Medical Net Tonnage";
        public static readonly string AmberPlusAmberMedicalNetTonnage = "Amber + Amber Medical Net Tonnage";
        public static readonly string GreenPlusGreenMedicalNetTonnage = "Green + Green Medical Net Tonnage";

        public static readonly string RedPlusRedMedicalMaterialPricePerTonne = "Red + Red Medical Material Price per Tonne";
        public static readonly string AmberPlusAmberMedicalMaterialPricePerTonne = "Amber + Amber Medical Material Price per Tonne";
        public static readonly string GreenPlusGreenMedicalMaterialPricePerTonne = "Green + Green Medical Material Price per Tonne";
        public static readonly string ProducerRedPlusRedMedicalMaterialDisposalCost = "Producer Red + Red Medical Material Disposal Cost";
        public static readonly string ProducerAmberPlusAmberMedicalMaterialDisposalCost = "Producer Amber + Amber Medical Material Disposal Cost";
        public static readonly string ProducerGreenPlusGreenMedicalMaterialDisposalCost = "Producer Green + Green Medical Material Disposal Cost";

        public static readonly string PricePerTonne = "Price per Tonne";
        public static readonly string ProducerDisposalFee = "Producer Disposal Fee w/o Bad Debt Provision";
        public static readonly string BadDebtProvision = "Bad Debt Provision";

        public static readonly string ProducerDisposalFeeWithBadDebtProvision =
            "Producer Disposal Fee with Bad Debt Provision";

        public static readonly string EnglandWithBadDebtProvision = "England with Bad Debt Provision";
        public static readonly string WalesWithBadDebtProvision = "Wales with Bad Debt Provision";
        public static readonly string ScotlandWithBadDebtProvision = "Scotland with Bad Debt Provision";
        public static readonly string NorthernIrelandWithBadDebtProvision = "Northern Ireland with Bad Debt Provision";

        public static readonly string ProducerTotalCostWithoutBadDebtProvision =
            "Producer Total Cost w/o Bad Debt Provision";

        public static readonly string ProducerTotalCostwithBadDebtProvision =
            "Producer Total Cost with Bad Debt Provision";

        public static readonly string TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision =
            "2a Total Producer Fee for Comms Costs - by Material w/o Bad Debt provision";

        public static readonly string TotalBadDebtProvision = "Total Bad Debt Provision";

        public static readonly string TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision =
            "2a Total Producer Fee for Comms Costs - by Material with Bad Debt provision";

        public static readonly string EnglandTotalwithBadDebtprovision = "England Total with Bad Debt provision";
        public static readonly string WalesTotalwithBadDebtprovision = "Wales Total with Bad Debt provision";
        public static readonly string ScotlandTotalwithBadDebtprovision = "Scotland Total with Bad Debt provision";

        public static readonly string NorthernIrelandTotalwithBadDebtprovision =
            "Northern Ireland Total with Bad Debt provision";

        public static readonly string TotalProducerDisposalFee = "1 Total Producer Disposal Fee w/o Bad Debt Provision";
        public static readonly string TotalProducerDisposalFeeWithBadDebtProvision = "1 Total Producer Disposal Fee with Bad Debt Provision";
        public static readonly string EnglandTotal = "England Total";
        public static readonly string WalesTotal = "Wales Total";
        public static readonly string ScotlandTotal = "Scotland Total";
        public static readonly string NorthernIrelandTotal = "Northern Ireland Total";
        public static readonly string OneCountryApportionment = "1 Country Apportionment %s";

        public static readonly string TonnageChangeCount = "Tonnage Change Count";
        public static readonly string TonnageChangeAdvice = "Tonnage Change Advice";

        //section 7
        public static readonly string TotalProducerFeeforLADisposalCostswoBadDebtprovision =
            "1 Total Producer Fee for LA Disposal Costs w/o Bad Debt provision";

        public static readonly string BadDebtProvisionFor1 = "Bad Debt Provision for 1";

        public static readonly string TotalProducerFeeforLADisposalCostswithBadDebtprovision =
            "1 Total Producer Fee for LA Disposal Costs with Bad Debt provision";

        public static readonly string TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision2A =
            "2a Total Producer Fee for Comms Costs - by Material w/o Bad Debt provision";

        public static readonly string BadDebtProvisionfor2A = "Bad Debt Provision for 2a";

        public static readonly string TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision2A =
            "2a Total Producer Fee for Comms Costs - by Material with Bad Debt provision";

        public static readonly string FeeforLADisposalCostswoBadDebtprovision1 =
            "1 Fee for LA Disposal Costs w/o Bad Debt provision";

        public static readonly string FeeforLADisposalCostswithBadDebtprovision1 =
            "1 Fee for LA Disposal Costs with Bad Debt provision";

        public static readonly string FeeforCommsCostsbyMaterialwoBadDebtprovision2A =
            "2a Fee for Comms Costs - by Material w/o Bad Debt provision";

        public static readonly string FeeforCommsCostsbyMaterialwithBadDebtprovision2A =
            "2a Fee for Comms Costs - by Material with Bad Debt provision";

        // Percentage of Producer Tonnage vs All Producers
        public static readonly string PercentageofProducerTonnagevsAllProducers = "Percentage of Producer Tonnage vs All Producers";

        // 2b comms total
        public static readonly string CommsCostHeaderWithoutBadDebtFor2bTitle = "2b Comms Costs - UK wide w/o Bad Debt provision";
        public static readonly string CommsCostHeaderWithBadDebtFor2bTitle = "2b Comms Costs - UK wide with Bad Debt provision";
        public static readonly string CommsCostHeaderBadDebtProvisionFor2bTitle = "Bad Debt provision";
        public static readonly string ProducerFeeWithoutBadDebtForComms2b = "2b Total Producer Fee for Comms Costs - UK wide In proportion to Producer Tonnage w/o Bad Debt provision";
        public static readonly string BadDebtProvisionForComms2b = "Bad Debt Provision for 2b";
        public static readonly string ProducerFeeForCommsCostsWithBadDebtForComms2b = "2b Total Producer Fee for Comms Costs - UK wide In proportion to Producer Tonnage with Bad Debt provision";
        public static readonly string EnglandTotalWithBadDebtProvisionForComms2b = "England Total with Bad Debt provision";
        public static readonly string WalesTotalWithBadDebtProvisionForComms2b = "Wales Total with Bad Debt provision";
        public static readonly string ScotlandTotalWithBadDebtProvisionForComms2b = "Scotland Total with Bad Debt provision";
        public static readonly string NorthernIrelandTotalWithBadDebtProvisionForComms2b = "Northern Ireland Total with Bad Debt provision";
    }
}