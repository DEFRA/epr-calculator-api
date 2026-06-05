using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary.TotalBillBreakdown
{
    public static class TotalBillBreakdownProducer
    {
        public static void SetValues(CalcResultSummary result)
        {
            foreach (var fee in result.ProducerDisposalFees)
            {
                fee.TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision
                {
                    TotalProducerFeeWithoutBadDebtProvision = GetTotalProducerBillWithoutBadDebtProvision(fee) ?? 0,
                    BadDebtProvision = GetBadDebtProvisionForTotalProducerBill(fee) ?? 0,
                    TotalProducerFeeWithBadDebtProvision = GetTotalProducerBillWithBadDebtProvision(fee) ?? 0,
                    EnglandTotalWithBadDebtProvision = GetEnglandTotalWithBadDebtProvision(fee) ?? 0,
                    WalesTotalWithBadDebtProvision = GetWalesTotalWithBadDebtProvision(fee) ?? 0,
                    ScotlandTotalWithBadDebtProvision = GetScotlandTotalWithBadDebtProvision(fee) ?? 0,
                    NorthernIrelandTotalWithBadDebtProvision = GetNorthernIrelandTotalWithBadDebtProvision(fee) ?? 0
                };
            }
        }

        private static decimal? GetTotalProducerBillWithoutBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.LocalAuthorityDisposalCostsSectionOne?.TotalProducerFeeWithoutBadDebtProvision +
                   fee.CommunicationCostsSectionTwoA?.TotalProducerFeeWithoutBadDebtProvision +
                   fee.CommunicationCostsSectionTwoB?.TotalProducerFeeWithoutBadDebtProvision +
                   fee.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt +
                   fee.SchemeAdministratorOperatingCosts?.TotalProducerFeeWithoutBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts?.TotalProducerFeeWithoutBadDebtProvision +
                   fee.OneOffSchemeAdministrationSetupCosts?.TotalProducerFeeWithoutBadDebtProvision;
        }

        private static decimal? GetBadDebtProvisionForTotalProducerBill(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.LocalAuthorityDisposalCostsSectionOne?.BadDebtProvision +
                   fee.CommunicationCostsSectionTwoA?.BadDebtProvision +
                   fee.CommunicationCostsSectionTwoB?.BadDebtProvision +
                   fee.TwoCBadDebtProvision +
                   fee.SchemeAdministratorOperatingCosts?.BadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts?.BadDebtProvision +
                   fee.OneOffSchemeAdministrationSetupCosts?.BadDebtProvision;
        }

        private static decimal? GetTotalProducerBillWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.LocalAuthorityDisposalCostsSectionOne?.TotalProducerFeeWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoA?.TotalProducerFeeWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoB?.TotalProducerFeeWithBadDebtProvision +
                   fee.TwoCTotalProducerFeeForCommsCostsWithBadDebt +
                   fee.SchemeAdministratorOperatingCosts?.TotalProducerFeeWithBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts?.TotalProducerFeeWithBadDebtProvision +
                   fee.OneOffSchemeAdministrationSetupCosts?.TotalProducerFeeWithBadDebtProvision;
        }

        private static decimal? GetEnglandTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.LocalAuthorityDisposalCostsSectionOne?.EnglandTotalWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoA?.EnglandTotalWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoB?.EnglandTotalWithBadDebtProvision +
                   fee.TwoCEnglandTotalWithBadDebt +
                   fee.SchemeAdministratorOperatingCosts?.EnglandTotalWithBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts?.EnglandTotalWithBadDebtProvision +
                   fee.OneOffSchemeAdministrationSetupCosts?.EnglandTotalWithBadDebtProvision;
        }

        private static decimal? GetWalesTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.LocalAuthorityDisposalCostsSectionOne?.WalesTotalWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoA?.WalesTotalWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoB?.WalesTotalWithBadDebtProvision +
                   fee.TwoCWalesTotalWithBadDebt +
                   fee.SchemeAdministratorOperatingCosts?.WalesTotalWithBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts?.WalesTotalWithBadDebtProvision +
                   fee.OneOffSchemeAdministrationSetupCosts?.WalesTotalWithBadDebtProvision;
        }

        private static decimal? GetScotlandTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.LocalAuthorityDisposalCostsSectionOne?.ScotlandTotalWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoA?.ScotlandTotalWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoB?.ScotlandTotalWithBadDebtProvision +
                   fee.TwoCScotlandTotalWithBadDebt +
                   fee.SchemeAdministratorOperatingCosts?.ScotlandTotalWithBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts?.ScotlandTotalWithBadDebtProvision +
                   fee.OneOffSchemeAdministrationSetupCosts?.ScotlandTotalWithBadDebtProvision;
        }

        private static decimal? GetNorthernIrelandTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.LocalAuthorityDisposalCostsSectionOne?.NorthernIrelandTotalWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoA?.NorthernIrelandTotalWithBadDebtProvision +
                   fee.CommunicationCostsSectionTwoB?.NorthernIrelandTotalWithBadDebtProvision +
                   fee.TwoCNorthernIrelandTotalWithBadDebt +
                   fee.SchemeAdministratorOperatingCosts?.NorthernIrelandTotalWithBadDebtProvision +
                   fee.LocalAuthorityDataPreparationCosts?.NorthernIrelandTotalWithBadDebtProvision +
                   fee.OneOffSchemeAdministrationSetupCosts?.NorthernIrelandTotalWithBadDebtProvision;
        }
    }
}
