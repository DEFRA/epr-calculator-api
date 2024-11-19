using Microsoft.Extensions.Hosting;

namespace EPR.Calculator.API.Constants
{
    public static class CalcResultSummaryHeaders
    {
        public static readonly string CalculationResult = "Calculation Result";
        public static readonly string OneProducerDisposalFeesWithBadDebtProvision = "1 Producer Disposal Fees with Bad Debt Provision";
        public static readonly string DisposalFeeSummary = "Disposal Fee Summary";
        public static readonly string OneFeeForLADisposalCosts = "1 Fee for LA Disposal Costs w/o Bad Debt provision";
        public static readonly string OneFeeForLADisposalCostsWithBadDebtProvision = "1 Fee for LA Disposal Costs with Bad Debt provision";
        public static readonly string TwoAFeeForCommsCosts = "2a Fee for Comms Costs - by Material w/o Bad Debt provision";
        public static readonly string TwoAFeeForCommsCostsWithBadDebtProvision = "2a Fee for Comms Costs - by Material with Bad Debt provision";
        public static readonly string ProducerId = "Producer ID";
        public static readonly string SubsidiaryId = "Subsidiary ID";
        public static readonly string ProducerOrSubsidiaryName = "Producer / Subsidiary Name";
        public static readonly string Level = "Level";
        public static readonly string ReportedHouseholdPackagingWasteTonnage = "Reported Household Packaging Waste Tonnage";
        public static readonly string ReportedSelfManagedConsumerWasteTonnage = "Reported Self Managed Consumer Waste Tonnage";
        public static readonly string NetReportedTonnage = "Net Reported Tonnage";
        public static readonly string PricePerTonne = "Price per Tonne";
        public static readonly string ProducerDisposalFee = "Producer Disposal Fee w/o Bad Debt Provision";
        public static readonly string BadDebtProvision = "Bad Debt Provision";
        public static readonly string ProducerDisposalFeeWithBadDebtProvision = "Producer Disposal Fee with Bad Debt Provision";
        public static readonly string EnglandWithBadDebtProvision = "England with Bad Debt Provision";
        public static readonly string WalesWithBadDebtProvision = "Wales with Bad Debt Provision";
        public static readonly string ScotlandWithBadDebtProvision = "Scotland with Bad Debt Provision";
        public static readonly string NorthernIrelandWithBadDebtProvision = "Northern Ireland with Bad Debt Provision";
        public static readonly string TotalProducerDisposalFee = "1 Total Producer Disposal Fee w/o Bad Debt Provision";
        public static readonly string TotalProducerDisposalFeeWithBadDebtProvision = "1 Total Producer Disposal Fee with Bad Debt Provision";
        public static readonly string EnglandTotal = "England Total";
        public static readonly string WalesTotal = "Wales Total";
        public static readonly string ScotlandTotal = "Scotland Total";
        public static readonly string NorthernIrelandTotal = "Northern Ireland Total";
        public static readonly string OneCountryApportionment = "1 Country Apportionment";

        public static readonly string TotalProducerFeeLADisposalFee = "1 Total Producer Fee for LA Disposal Costs w/o Bad Debt provision";
        public static readonly string BadDebtProvisionForOne = "Bad Debt Provision for 1";
        public static readonly string TotalProducerFeeLADisposalFeeWithBadDebtProvision = "1 Total Producer Fee for LA Disposal Costs with Bad Debt provision";
        public static readonly string EnglandTotalWithBadDebtProvision = "England Total with Bad Debt provision";
        public static readonly string WalesTotalWithBadDebtProvision = "Wales Total with Bad Debt provision";
        public static readonly string ScotlandTotalWithBadDebtProvision = "Scotland Total with Bad Debt provision";
        public static readonly string NorthernIrelandTotalWithBadDebtProvision = "Northern Ireland Total with Bad Debt provision";
    }
}
