﻿using static Azure.Core.HttpHeader;

namespace EPR.Calculator.API.Models
{
    public class CalcResultSummary
    {
        public CalcResultSummaryHeader? ResultSummaryHeader { get; set; }

        public IEnumerable<CalcResultSummaryHeader>? ProducerDisposalFeesHeaders { get; set; }

        public CalcResultSummaryHeader? CommsCostHeader { get; set; }

        public IEnumerable<CalcResultSummaryHeader>? MaterialBreakdownHeaders { get; set; }

        public IEnumerable<CalcResultSummaryHeader>? ColumnHeaders { get; set; }

        //Section-(1) & (2a)
        public decimal TotalFeeforLADisposalCostswoBadDebtprovision1 { get; set; }

        public decimal BadDebtProvisionFor1 { get; set; }

        public decimal TotalFeeforLADisposalCostswithBadDebtprovision1 { get; set; }

        public decimal TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A { get; set; }

        public decimal BadDebtProvisionFor2A { get; set; }

        public decimal TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A { get; set; }

        public decimal TotalOnePlus2AFeeWithBadDebtProvision { get; set; }

        // Section-3
        public decimal SAOperatingCostsWoTitleSection3 { get; set; }

        public decimal SAOperatingCostsWithTitleSection3 { get; set; }

        public decimal BadDebtProvisionTitleSection3 { get; set; }
        //Ends Section-3

        // Section-4 LA data prep costs
        public decimal LaDataPrepCostsTitleSection4 { get; set; }

        public decimal LaDataPrepCostsBadDebtProvisionTitleSection4 { get; set; }

        public decimal LaDataPrepCostsWithBadDebtProvisionTitleSection4 { get; set; }
        // End Section-4 LA data prep costs

        // Section-5 SA setup costs
        public decimal SaSetupCostsTitleSection5 { get; set; }

        public decimal SaSetupCostsBadDebtProvisionTitleSection5 { get; set; }

        public decimal SaSetupCostsWithBadDebtProvisionTitleSection5 { get; set; }
        // End Section-5 SA setup costs

        public decimal TwoCCommsCostsByCountryWithoutBadDebtProvision { get; set; }

        public decimal TwoCBadDebtProvision { get; set; }

        public decimal TwoCCommsCostsByCountryWithBadDebtProvision { get; set; }

        public decimal CommsCostHeaderWithoutBadDebtFor2bTitle { get; set; }

        public decimal CommsCostHeaderWithBadDebtFor2bTitle { get; set; }

        public decimal CommsCostHeaderBadDebtProvisionFor2bTitle { get; set; }

        public IEnumerable<CalcResultSummaryProducerDisposalFees> ProducerDisposalFees { get; set; }
    }
}