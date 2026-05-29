namespace EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions
{
    public static class BillingInstructionsHeader
    {
        public static readonly string Title = "Calculation of Suggested Billing Instructions and Invoice Amounts";

        public static readonly string CurrentYearInvoicedTotalToDate = "Current Year Invoiced Total To Date";
        public static readonly string TonnageChangeSinceLastInvoice = "Tonnage Change Since Last Invoice";
        public static readonly string LiabilityDifference = "Liability Difference (Calc vs Prev)";
        public static readonly string MaterialThresholdBreached = "Material £ Threshold Breached";
        public static readonly string TonnageThresholdBreached = "Tonnage £ Threshold Breached (if tonnage changed)";
        public static readonly string PercentageLiabilityDifference = "% Liability Difference (Calc vs Prev)";
        public static readonly string MaterialPercentageThresholdBreached = "Material % Threshold Breached";
        public static readonly string TonnagePercentagThresholdBreached = "Tonnage % Threshold Breached (if tonnage changed)";
        public static readonly string SuggestedBillingInstruction = "Suggested Billing Instruction";
        public static readonly string SuggestedInvoiceAmount = "Suggested Invoice Amount";
    }
}
