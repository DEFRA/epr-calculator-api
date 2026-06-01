namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummaryProducerDisposalFees
    {
        public required string ProducerId { get; set; }

        public required string SubsidiaryId { get; set; }

        public required string ProducerName { get; set; }

        public string? TradingName { get; set; }

        public string? Level { get; set; }

        public string? StatusCode { get; set; }

        public string IsProducerScaledup { get; set; } = "No";

        public string IsPartialObligation { get; set; } = "No";

        public string? JoinerDate { get; set; }

        public string? LeaverDate { get; set; }

        public bool isTotalRow { get; set; }

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

        public CalcResultSummaryBadDebtProvision? LocalAuthorityDisposalCostsSectionOne { get; set; }

        public CalcResultSummaryBadDebtProvision? CommunicationCostsSectionTwoA { get; set; }

        public CalcResultSummaryBadDebtProvision? CommunicationCostsSectionTwoB { get; set; }

        public CalcResultSummaryBadDebtProvision? CommunicationCostsSectionTwoC { get; set; }

        public CalcResultSummaryBadDebtProvision? SchemeAdministratorOperatingCosts { get; set; }

        public CalcResultSummaryBadDebtProvision? LocalAuthorityDataPreparationCosts { get; set; }

        public CalcResultSummaryBadDebtProvision? OneOffSchemeAdministrationSetupCosts { get; set; }

        public CalcResultSummaryBadDebtProvision? TotalProducerBillBreakdownCosts { get; set; }

        public decimal TwoCTotalProducerFeeForCommsCostsWithoutBadDebt { get; set; }
        public decimal TwoCBadDebtProvision { get; set; }
        public decimal TwoCTotalProducerFeeForCommsCostsWithBadDebt { get; set; }
        public decimal TwoCEnglandTotalWithBadDebt { get; set; }
        public decimal TwoCWalesTotalWithBadDebt { get; set; }
        public decimal TwoCScotlandTotalWithBadDebt { get; set; }
        public decimal TwoCNorthernIrelandTotalWithBadDebt { get; set; }

        public decimal PercentageofProducerReportedTonnagevsAllProducers { get; set; }

        // Section Total bill (1 + 2a + 2b + 2c)
        public decimal ProducerTotalOnePlus2A2B2CWithBadDeptProvision { get; set; }

        public decimal ProducerOverallPercentageOfCostsForOnePlus2A2B2C { get; set; }
        // End Section Total bill (1 + 2a + 2b + 2c)

        public Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> ProducerDisposalFeesByMaterial { get; set; } = new();

        public Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> ProducerCommsFeesByMaterial { get; set; } = new();

        public string? TonnageChangeCount { get; set; }

        public string? TonnageChangeAdvice { get; set; }

        public CalcResultSummaryBillingInstruction? BillingInstructionSection { get; set; }

        public bool isOverallTotalRow { get; set; }
        public int ProducerIdInt { get; set; }
    }
}