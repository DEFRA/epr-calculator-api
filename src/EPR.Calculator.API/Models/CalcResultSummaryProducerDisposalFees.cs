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

        //Section-(1) & (2a) Start
        public decimal TotalProducerFeeforLADisposalCostswoBadDebtprovision { get; set; }

        public decimal BadDebtProvisionFor1 { get; set; }

        public decimal TotalProducerFeeforLADisposalCostswithBadDebtprovision { get; set; }

        public decimal EnglandTotalwithBadDebtprovision { get; set; }

        public decimal WalesTotalwithBadDebtprovision { get; set; }

        public decimal ScotlandTotalwithBadDebtprovision { get; set; }

        public decimal NorthernIrelandTotalwithBadDebtprovision { get; set; }

        public decimal TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision { get; set; }

        public decimal BadDebtProvisionFor2A { get; set; }

        public decimal TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision { get; set; }

        public decimal EnglandTotalwithBadDebtprovision2A { get; set; }

        public decimal WalesTotalwithBadDebtprovision2A { get; set; }

        public decimal ScotlandTotalwithBadDebtprovision2A { get; set; }

        public decimal NorthernIrelandTotalwithBadDebtprovision2A { get; set; }
        //Section-(1) & (2a) End

        public decimal TwoCTotalProducerFeeForCommsCostsWithoutBadDebt { get; set; }
        public decimal TwoCBadDebtProvisionHeader { get; set; }
        public decimal TwoCBadDebtCommCostByCountry { get; set; }
        public decimal TwoCBadDebtProvision { get; set; }
        public decimal TwoCTotalProducerFeeForCommsCostsWithBadDebt { get; set; }
        public decimal TwoCEnglandTotalWithBadDebt { get; set; }
        public decimal TwoCWalesTotalWithBadDebt { get; set; }
        public decimal TwoCScotlandTotalWithBadDebt { get; set; }
        public decimal TwoCNorthernIrelandTotalWithBadDebt { get; set; }

        public decimal PercentageofProducerReportedHHTonnagevsAllProducers { get; set; }

        //Section-3
        public decimal Total3SAOperatingCostwoBadDebtprovision { get; set; }

        public decimal BadDebtProvisionFor3 { get; set; }

        public decimal Total3SAOperatingCostswithBadDebtprovision { get; set; }

        public decimal EnglandTotalwithBadDebtprovision3 { get; set; }

        public decimal WalesTotalwithBadDebtprovision3 { get; set; }

        public decimal ScotlandTotalwithBadDebtprovision3 { get; set; }

        public decimal NorthernIrelandTotalwithBadDebtprovision3 { get; set; }
        //End Section-3

        // Section-4 LA data prep costs
        public decimal LaDataPrepCostsTotalWithoutBadDebtProvisionSection4 { get; set; }

        public decimal LaDataPrepCostsBadDebtProvisionSection4 { get; set; }

        public decimal LaDataPrepCostsTotalWithBadDebtProvisionSection4 { get; set; }

        public decimal LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4 { get; set; }

        public decimal LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4 { get; set; }

        public decimal LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4 { get; set; }

        public decimal LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4 { get; set; }
        // End Section-4 LA data prep costs

        public required Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> ProducerDisposalFeesByMaterial { get; set; }
        public required Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> ProducerCommsFeesByMaterial { get; set; }
        public decimal TotalOnePlus2AFeeWithBadDebtProvision { get; set; }
        public decimal ProducerPercentageOfCosts { get; set; }

        public decimal TotalProducerFeeWithoutBadDebtFor2bComms { get; set; }

        public decimal BadDebtProvisionFor2bComms { get; set; }

        public decimal TotalProducerFeeWithBadDebtFor2bComms { get; set; }

        public decimal EnglandTotalWithBadDebtFor2bComms { get; set; }

        public decimal WalesTotalWithBadDebtFor2bComms { get; set; }

        public decimal ScotlandTotalWithBadDebtFor2bComms { get; set; }

        public decimal NorthernIrelandTotalWithBadDebtFor2bComms { get; set; }

    }
}