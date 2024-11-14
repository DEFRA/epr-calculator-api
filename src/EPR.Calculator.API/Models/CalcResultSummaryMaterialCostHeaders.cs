namespace EPR.Calculator.API.Models
{
    public class CalcResultSummaryMaterialCostHeaders
    {
        public string HouseholdPackagingWasteTonnage { get; set; }

        public string ManagedConsumerWasteTonnage { get; set; }

        public string NetReportedTonnage { get; set; }

        public string PricePerTonnage { get; set; }

        public string ProducerDisposalFee { get; set; }

        public string BadDebtProvision { get; set; }

        public string ProducerDisposalFeeWithBadDebtProvision { get; set; }

        public string EnglandWithBadDebtProvision { get; set; }

        public string WalesWithBadDebtProvision { get; set; }

        public string ScotlandWithBadDebtProvision { get; set; }

        public string NorthernIrelandWithBadDebtProvision { get; set; }
    }
}
