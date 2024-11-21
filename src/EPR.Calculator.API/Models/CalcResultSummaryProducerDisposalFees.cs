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

        public required Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> ProducerDisposalFeesByMaterial { get; set; }
        public required Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> ProducerCommsFeesByMaterial { get; set; }
        public decimal TotalOnePlus2AFeeWithBadDebtProvision { get; set; }
        public decimal ProducerPercentageOfCosts { get; set; }

    }
}