namespace EPR.Calculator.API.Models
{
    public class CalcResultSummaryProducerDisposalFees
    {
        public required string ProducerId { get; set; }

        public required string SubsidiaryId { get; set; }

        public required string ProducerName { get; set; }

        public string Level { get; set; }

        public bool isTotalRow { get; set; } = false;

        public decimal TotalProducerDisposalFee { get; set; }

        public decimal BadDebtProvision { get; set; }

        public decimal TotalProducerDisposalFeeWithBadDebtProvision { get; set; }

        public decimal EnglandTotal { get; set; }

        public decimal WalesTotal { get; set; }

        public decimal ScotlandTotal { get; set; }

        public decimal NorthernIrelandTotal { get; set; }

        public decimal TotalProducerCommsFee { get; set; }

        public decimal BadDebtProvisionComms { get; set; }

        public decimal TotalProducerCommsFeeWithBadDebtProvision { get; set; }

        public decimal EnglandTotalComms { get; set; }

        public decimal WalesTotalComms { get; set; }

        public decimal ScotlandTotalComms { get; set; }

        public decimal NorthernIrelandTotalComms { get; set; }

        public decimal LaDataPrepCostsTotalWithoutBadDebtProvisionSection4 { get; set; }

        public decimal LaDataPrepCostsBadDebtProvisionSection4 { get; set; }

        public decimal LaDataPrepCostsTotalWithBadDebtProvisionSection4 { get; set; }

        public decimal LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4 { get; set; }

        public decimal LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4 { get; set; }

        public decimal LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4 { get; set; }

        public decimal LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4 { get; set; }

        public required Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> ProducerDisposalFeesByMaterial { get; set; }
        public required Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> ProducerCommsFeesByMaterial { get; set; }

    }
}