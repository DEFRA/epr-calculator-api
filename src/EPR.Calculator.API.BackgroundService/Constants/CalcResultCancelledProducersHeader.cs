namespace EPR.Calculator.API.BackgroundService.Constants
{
    public static class CalcResultCancelledProducersHeader
    {
        public static readonly string CancelledProducers = "Cancelled Producers";
        public static readonly string ProducerId = "Producer ID";
        public static readonly string ProducerOrSubsidiaryName = "Producer Name";
        public static readonly string TradingName = "Trading Name";

        public static readonly string LastTonnage = "Last Tonnage";
        public const int LastTonnageSubHeaderIndex = 3;

        public static readonly string LatestInvoice = "Latest Invoice";
        public const int LatestInvoiceSubHeaderIndex = 11;

        public static readonly string RunNumber = "Run Number";
        public static readonly string RunName = "Run Name";
        public static readonly string BillingInstructionId = "Billing Instruction ID";
        public static readonly string CurrentYearInvoicedTotalToDate = "Current Year Invoiced Total To Date";
    }
}
