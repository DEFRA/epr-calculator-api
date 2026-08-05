namespace EPR.Calculator.Service.Function.Constants
{
    public static class CalcResultCancelledProducersHeader
    {
        public static readonly string CancelledProducers = "Cancelled Producers";
        public static readonly string ProducerId = "Producer ID";
        public static readonly string ProducerOrSubsidiaryName = "Producer Name";
        public static readonly string TradingName = "Trading Name";

        public static readonly string LastTonnage = "Last Tonnage";
        public const int LastTonnageSubHeaderIndex = 3;

        public static readonly string Aluminium = "Aluminium";
        public static readonly string FibreComposite = "Fibre composite";
        public static readonly string Glass = "Glass";
        public static readonly string PaperOrCard = "Paper or Card";
        public static readonly string Plastic = "Plastic";
        public static readonly string Steel = "Steel";
        public static readonly string Wood = "Wood";
        public static readonly string OtherMaterials = "Other materials";

        public static readonly string LatestInvoice = "Latest Invoice";
        public const int LatestInvoiceSubHeaderIndex = 11;

        public static readonly string RunNumber = "Run Number";
        public static readonly string RunName = "Run Name";
        public static readonly string BillingInstructionId = "Billing Instruction ID";
        public static readonly string CurrentYearInvoicedTotalToDate = "Current Year Invoiced Total To Date";
    }
}
