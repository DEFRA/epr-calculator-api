﻿using EPR.Calculator.API.Builder.Summary.Common;
using EPR.Calculator.API.Builder.Summary.CommsCostTwoA;
using EPR.Calculator.API.Builder.Summary.CommsCostTwoBTotalBill;
using EPR.Calculator.API.Builder.Summary.HHTonnageVsAllProducer;
using EPR.Calculator.API.Builder.Summary.LaDataPrepCosts;
using EPR.Calculator.API.Builder.Summary.OneAndTwoA;
using EPR.Calculator.API.Builder.Summary.OnePlus2A2B2C;
using EPR.Calculator.API.Builder.Summary.SaSetupCosts;
using EPR.Calculator.API.Builder.Summary.ThreeSA;
using EPR.Calculator.API.Builder.Summary.TotalBillBreakdown;
using EPR.Calculator.API.Builder.Summary.TwoCCommsCost;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Builder.Summary
{
    public class CalcResultSummaryBuilder : ICalcResultSummaryBuilder
    {
        private readonly ApplicationDBContext context;

        public CalcResultSummaryBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<CalcResultSummary> Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult)
        {
            // Get and map materials from DB
            var materialsFromDb = await context.Material.ToListAsync();
            var materials = Mappers.MaterialMapper.Map(materialsFromDb);
            var producerDetails = await context.ProducerDetail.ToListAsync();
            var producerReportedMaterials = await context.ProducerReportedMaterial.ToListAsync();
            var runId = resultsRequestDto.RunId;

            var runProducerMaterialDetails = GetProducerRunMaterialDetails(
                producerDetails,
                producerReportedMaterials,
                runId);

            var orderedProducerDetails = GetOrderedListOfProducersAssociatedRunId(
                runId, producerDetails);

            var result = GetCalcResultSummary(
                orderedProducerDetails,
                materials,
                runProducerMaterialDetails,
                calcResult);

            return result;
        }

        public static CalcResultSummary GetCalcResultSummary(
            IEnumerable<ProducerDetail> orderedProducerDetails,
            IEnumerable<MaterialDetail> materials,
            IEnumerable<CalcResultsProducerAndReportMaterialDetail> runProducerMaterialDetails,
            CalcResult calcResult)
        {
            var result = new CalcResultSummary();
            if (orderedProducerDetails.Any())
            {
                var producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>();

                foreach (var producer in orderedProducerDetails)
                {
                    // We have to write an additional row if a producer have at least one subsidiary
                    // This additional row will be the total of this producer and its subsidiaries
                    var producersAndSubsidiaries = orderedProducerDetails.Where(pd => pd.ProducerId == producer.ProducerId);
                    // Make sure the total row is written only once
                    if (producersAndSubsidiaries.Count() > 1 &&
                        producerDisposalFees.Find(pdf => pdf.ProducerId == producer.ProducerId.ToString()) == null)
                    {
                        var totalRow = GetProducerTotalRow(producersAndSubsidiaries.ToList(), materials, calcResult,
                            runProducerMaterialDetails, producerDisposalFees, false);
                        producerDisposalFees.Add(totalRow);
                    }

                    // Calculate the values for the producer
                    producerDisposalFees.Add(GetProducerRow(producerDisposalFees, producer, materials, calcResult,
                        runProducerMaterialDetails));
                }

                // Calculate the total for all the producers
                var allTotalRow = GetProducerTotalRow(orderedProducerDetails.ToList(), materials, calcResult,
                    runProducerMaterialDetails, producerDisposalFees, true);
                producerDisposalFees.Add(allTotalRow);

                result.ProducerDisposalFees = producerDisposalFees;

                //Section-(1) & (2a)
                result.TotalFeeforLADisposalCostswoBadDebtprovision1 = CalcResultOneAndTwoAUtil.GetTotalDisposalCostswoBadDebtprovision1(producerDisposalFees);
                result.BadDebtProvisionFor1 = CalcResultOneAndTwoAUtil.GetTotalBadDebtprovision1(producerDisposalFees);
                result.TotalFeeforLADisposalCostswithBadDebtprovision1 = CalcResultOneAndTwoAUtil.GetTotalDisposalCostswithBadDebtprovision1(producerDisposalFees);

                result.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A = CalcResultOneAndTwoAUtil.GetTotalCommsCostswoBadDebtprovision2A(producerDisposalFees);
                result.BadDebtProvisionFor2A = CalcResultOneAndTwoAUtil.GetTotalBadDebtprovision2A(producerDisposalFees);
                result.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = CalcResultOneAndTwoAUtil.GetTotalCommsCostswithBadDebtprovision2A(producerDisposalFees);

                // 2b comms total
                result.CommsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);
                result.CommsCostHeaderBadDebtProvisionFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderBadDebtProvisionFor2bTitle(calcResult);
                result.CommsCostHeaderWithBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithBadDebtFor2bTitle(calcResult);

                TwoCCommsCostUtil.UpdateHeaderTotal(calcResult, result);

                // Section Total bill (1 + 2a + 2b + 2c)
                OnePlus2A2B2CProducer.SetValues(result);

                // SA Operating cost Section 3 -
                ThreeSaCostsProducer.GetProducerSetUpCostsSection3(calcResult, result);

                // Section-4 LA data prep costs
                LaDataPrepCostsProducer.SetValues(calcResult, result);

                // Section-5 SA setup costs
                SaSetupCostsProducer.GetProducerSetUpCosts(calcResult, result);

                // Total bill section
                TotalBillBreakdownProducer.SetValues(result);
            }

            // Set headers with calculated column index
            CalcResultSummaryUtil.SetHeaders(result, materials);

            return result;
        }

        public static IEnumerable<ProducerDetail> GetOrderedListOfProducersAssociatedRunId(
            int runId,
            IEnumerable<ProducerDetail> producerDetails)
        {
            return producerDetails.Where(pd => pd.CalculatorRunId == runId).OrderBy(pd => pd.ProducerId).ToList();
        }

        public static IEnumerable<CalcResultsProducerAndReportMaterialDetail> GetProducerRunMaterialDetails(
            IEnumerable<ProducerDetail> producerDetails,
            IEnumerable<ProducerReportedMaterial> producerReportedmaterials,
            int runId)
        {
            return (from p in producerDetails
                    join m in producerReportedmaterials
                 on p.Id equals m.ProducerDetailId
                    where p.CalculatorRunId == runId
                    select new CalcResultsProducerAndReportMaterialDetail
                    {
                        ProducerDetail = p,
                        ProducerReportedMaterial = m
                    }).ToList();
        }

        public static CalcResultSummaryProducerDisposalFees GetProducerTotalRow(List<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            IEnumerable<CalcResultsProducerAndReportMaterialDetail> runProducerMaterialDetails,
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            bool isOverAllTotalRow)
        {
            var materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();
            var commsCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>();

            foreach (var material in materials)
            {
                materialCostSummary.Add(material, new CalcResultSummaryProducerDisposalFeesByMaterial
                {
                    HouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetHouseholdPackagingWasteTonnageProducerTotal(producersAndSubsidiaries, material),
                    ManagedConsumerWasteTonnage = CalcResultSummaryUtil.GetManagedConsumerWasteTonnageProducerTotal(producersAndSubsidiaries, material),
                    NetReportedTonnage = CalcResultSummaryUtil.GetNetReportedTonnageProducerTotal(producersAndSubsidiaries, material),
                    PricePerTonne = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult),
                    ProducerDisposalFee = CalcResultSummaryUtil.GetProducerDisposalFeeProducerTotal(producersAndSubsidiaries, material, calcResult),
                    BadDebtProvision = CalcResultSummaryUtil.GetBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult),
                    ProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material,
                            calcResult),
                    EnglandWithBadDebtProvision = CalcResultSummaryUtil.GetEnglandWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult),
                    WalesWithBadDebtProvision = CalcResultSummaryUtil.GetWalesWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult),
                    ScotlandWithBadDebtProvision = CalcResultSummaryUtil.GetScotlandWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult),
                    NorthernIrelandWithBadDebtProvision = CalcResultSummaryUtil.GetNorthernIrelandWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material,
                            calcResult)
                });

                commsCostSummary.Add(material, new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetHouseholdPackagingWasteTonnageProducerTotal(producersAndSubsidiaries, material),
                    PriceperTonne = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult),
                    ProducerTotalCostWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvisionTotal(producersAndSubsidiaries, material,
                            calcResult),
                    BadDebtProvision = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCostTotal(producersAndSubsidiaries, material, calcResult),
                    ProducerTotalCostwithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvisionTotal(producersAndSubsidiaries, material, calcResult),
                    EnglandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material, calcResult),
                    WalesWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material, calcResult),
                    ScotlandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material, calcResult),
                    NorthernIrelandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material,
                            calcResult)
                });

            }

            var totalRow = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = isOverAllTotalRow ? string.Empty : producersAndSubsidiaries[0].ProducerId.ToString(),
                ProducerName = isOverAllTotalRow
                    ? string.Empty
                    : producersAndSubsidiaries[0].ProducerName ?? string.Empty,
                SubsidiaryId = string.Empty,
                Level = isOverAllTotalRow ? "Totals" : "1",
                ProducerDisposalFeesByMaterial = materialCostSummary,

                // Disposal fee summary
                TotalProducerDisposalFee = CalcResultSummaryUtil.GetTotalProducerDisposalFee(materialCostSummary),
                BadDebtProvision = CalcResultSummaryUtil.GetTotalBadDebtProvision(materialCostSummary),
                TotalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary),
                EnglandTotal = CalcResultSummaryUtil.GetEnglandTotal(materialCostSummary),
                WalesTotal = CalcResultSummaryUtil.GetWalesTotal(materialCostSummary),
                ScotlandTotal = CalcResultSummaryUtil.GetScotlandTotal(materialCostSummary),
                NorthernIrelandTotal = CalcResultSummaryUtil.GetNorthernIrelandTotal(materialCostSummary),

                //For Comms Start
                TotalProducerCommsFee = CalcResultSummaryUtil.GetTotalProducerCommsFee(commsCostSummary),
                BadDebtProvisionComms = CalcResultSummaryUtil.GetCommsTotalBadDebtProvision(commsCostSummary),
                TotalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(commsCostSummary),
                EnglandTotalComms = CalcResultSummaryUtil.GetEnglandCommsTotal(commsCostSummary),
                WalesTotalComms = CalcResultSummaryUtil.GetWalesCommsTotal(commsCostSummary),
                ScotlandTotalComms = CalcResultSummaryUtil.GetScotlandCommsTotal(commsCostSummary),
                NorthernIrelandTotalComms = CalcResultSummaryUtil.GetNorthernIrelandCommsTotal(commsCostSummary),
                ProducerCommsFeesByMaterial = commsCostSummary,

                //Section-(1) & (2a)
                TotalProducerFeeforLADisposalCostswoBadDebtprovision = CalcResultSummaryUtil.GetTotalProducerDisposalFee(materialCostSummary),
                BadDebtProvisionFor1 = CalcResultSummaryUtil.GetTotalBadDebtProvision(materialCostSummary),
                TotalProducerFeeforLADisposalCostswithBadDebtprovision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary),
                EnglandTotalWithBadDebtProvision = CalcResultSummaryUtil.GetEnglandTotal(materialCostSummary),
                WalesTotalWithBadDebtProvision = CalcResultSummaryUtil.GetWalesTotal(materialCostSummary),
                ScotlandTotalWithBadDebtProvision = CalcResultSummaryUtil.GetScotlandTotal(materialCostSummary),
                NorthernIrelandTotalWithBadDebtProvision = CalcResultSummaryUtil.GetNorthernIrelandTotal(materialCostSummary),

                TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision = CalcResultSummaryUtil.GetTotalProducerCommsFee(commsCostSummary),
                BadDebtProvisionFor2A = CalcResultSummaryUtil.GetCommsTotalBadDebtProvision(commsCostSummary),
                TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(commsCostSummary),
                EnglandTotalWithBadDebtProvision2A = CalcResultSummaryUtil.GetEnglandCommsTotal(commsCostSummary),
                WalesTotalWithBadDebtProvision2A = CalcResultSummaryUtil.GetWalesCommsTotal(commsCostSummary),
                ScotlandTotalWithBadDebtProvision2A = CalcResultSummaryUtil.GetScotlandCommsTotal(commsCostSummary),
                NorthernIrelandTotalWithBadDebtProvision2A = CalcResultSummaryUtil.GetNorthernIrelandCommsTotal(commsCostSummary),

                //Total Bill for 2b
                TotalProducerFeeWithoutBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2bTotalsRow(calcResult, producersAndSubsidiaries, runProducerMaterialDetails),
                BadDebtProvisionFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2bTotalsRow(calcResult, producersAndSubsidiaries, runProducerMaterialDetails),
                TotalProducerFeeWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2bTotalsRow(calcResult, producersAndSubsidiaries, runProducerMaterialDetails),
                EnglandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, runProducerMaterialDetails),
                WalesTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, runProducerMaterialDetails),
                ScotlandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, runProducerMaterialDetails),
                NorthernIrelandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, runProducerMaterialDetails),

                //Section-3
                // Percentage of Producer Reported Household Tonnage vs All Producers
                PercentageofProducerReportedHHTonnagevsAllProducers = HHTonnageVsAllProducerUtil.GetPercentageofProducerReportedHHTonnagevsAllProducersTotal(producersAndSubsidiaries, runProducerMaterialDetails),

                isTotalRow = true
            };

            TwoCCommsCostUtil.UpdateTwoCTotals(calcResult, producerDisposalFees, isOverAllTotalRow, totalRow,
                producersAndSubsidiaries, runProducerMaterialDetails);

            return totalRow;
        }

        public static CalcResultSummaryProducerDisposalFees GetProducerRow(
            List<CalcResultSummaryProducerDisposalFees> producerDisposalFeesLookup,
            ProducerDetail producer,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            IEnumerable<CalcResultsProducerAndReportMaterialDetail> runProducerMaterialDetails)
        {
            var materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();
            var commsCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>();

            foreach (var material in materials)
            {
                materialCostSummary.Add(material, new CalcResultSummaryProducerDisposalFeesByMaterial
                {
                    HouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetHouseholdPackagingWasteTonnage(producer, material),
                    ManagedConsumerWasteTonnage = CalcResultSummaryUtil.GetManagedConsumerWasteTonnage(producer, material),
                    NetReportedTonnage = CalcResultSummaryUtil.GetNetReportedTonnage(producer, material),
                    PricePerTonne = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult),
                    ProducerDisposalFee = CalcResultSummaryUtil.GetProducerDisposalFee(producer, material, calcResult),
                    BadDebtProvision = CalcResultSummaryUtil.GetBadDebtProvision(producer, material, calcResult),
                    ProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult),
                    EnglandWithBadDebtProvision = CalcResultSummaryUtil.GetEnglandWithBadDebtProvision(producer, material, calcResult),
                    WalesWithBadDebtProvision = CalcResultSummaryUtil.GetWalesWithBadDebtProvision(producer, material, calcResult),
                    ScotlandWithBadDebtProvision = CalcResultSummaryUtil.GetScotlandWithBadDebtProvision(producer, material, calcResult),
                    NorthernIrelandWithBadDebtProvision = CalcResultSummaryUtil.GetNorthernIrelandWithBadDebtProvision(producer, material, calcResult)
                });

                commsCostSummary.Add(material, new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetHouseholdPackagingWasteTonnage(producer, material),
                    PriceperTonne = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult),
                    ProducerTotalCostWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvision(producer, material, calcResult),
                    BadDebtProvision = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCost(producer, material, calcResult),
                    ProducerTotalCostwithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult),
                    EnglandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForComms(producer, material, calcResult),
                    WalesWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForComms(producer, material, calcResult),
                    ScotlandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForComms(producer, material, calcResult),
                    NorthernIrelandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForComms(producer, material, calcResult)
                });

            }

            var result = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = producer.ProducerId.ToString(),
                ProducerName = producer.ProducerName ?? string.Empty,
                SubsidiaryId = producer.SubsidiaryId ?? string.Empty,
                Level = CalcResultSummaryUtil.GetLevelIndex(producerDisposalFeesLookup, producer).ToString(),
                ProducerDisposalFeesByMaterial = materialCostSummary,

                // Disposal fee summary
                TotalProducerDisposalFee = CalcResultSummaryUtil.GetTotalProducerDisposalFee(materialCostSummary),
                BadDebtProvision = CalcResultSummaryUtil.GetTotalBadDebtProvision(materialCostSummary),
                TotalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary),
                EnglandTotal = CalcResultSummaryUtil.GetEnglandTotal(materialCostSummary),
                WalesTotal = CalcResultSummaryUtil.GetWalesTotal(materialCostSummary),
                ScotlandTotal = CalcResultSummaryUtil.GetScotlandTotal(materialCostSummary),
                NorthernIrelandTotal = CalcResultSummaryUtil.GetNorthernIrelandTotal(materialCostSummary),

                //For comms
                TotalProducerCommsFee = CalcResultSummaryUtil.GetTotalProducerCommsFee(commsCostSummary),
                BadDebtProvisionComms = CalcResultSummaryUtil.GetCommsTotalBadDebtProvision(commsCostSummary),
                TotalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(commsCostSummary),
                EnglandTotalComms = CalcResultSummaryUtil.GetEnglandCommsTotal(commsCostSummary),
                WalesTotalComms = CalcResultSummaryUtil.GetWalesCommsTotal(commsCostSummary),
                ScotlandTotalComms = CalcResultSummaryUtil.GetScotlandCommsTotal(commsCostSummary),
                NorthernIrelandTotalComms = CalcResultSummaryUtil.GetNorthernIrelandCommsTotal(commsCostSummary),
                ProducerCommsFeesByMaterial = commsCostSummary,

                //Section-(1) & (2a)
                TotalProducerFeeforLADisposalCostswoBadDebtprovision = CalcResultSummaryUtil.GetTotalProducerDisposalFee(materialCostSummary),
                BadDebtProvisionFor1 = CalcResultSummaryUtil.GetTotalBadDebtProvision(materialCostSummary),
                TotalProducerFeeforLADisposalCostswithBadDebtprovision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary),
                EnglandTotalWithBadDebtProvision = CalcResultSummaryUtil.GetEnglandTotal(materialCostSummary),
                WalesTotalWithBadDebtProvision = CalcResultSummaryUtil.GetWalesTotal(materialCostSummary),
                ScotlandTotalWithBadDebtProvision = CalcResultSummaryUtil.GetScotlandTotal(materialCostSummary),
                NorthernIrelandTotalWithBadDebtProvision = CalcResultSummaryUtil.GetNorthernIrelandTotal(materialCostSummary),

                TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision = CalcResultSummaryUtil.GetTotalProducerCommsFee(commsCostSummary),
                BadDebtProvisionFor2A = CalcResultSummaryUtil.GetCommsTotalBadDebtProvision(commsCostSummary),
                TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(commsCostSummary),
                EnglandTotalWithBadDebtProvision2A = CalcResultSummaryUtil.GetEnglandCommsTotal(commsCostSummary),
                WalesTotalWithBadDebtProvision2A = CalcResultSummaryUtil.GetWalesCommsTotal(commsCostSummary),
                ScotlandTotalWithBadDebtProvision2A = CalcResultSummaryUtil.GetScotlandCommsTotal(commsCostSummary),
                NorthernIrelandTotalWithBadDebtProvision2A = CalcResultSummaryUtil.GetNorthernIrelandCommsTotal(commsCostSummary),

                //Total Bill for 2b
                TotalProducerFeeWithoutBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, runProducerMaterialDetails),
                BadDebtProvisionFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2b(calcResult, producer, runProducerMaterialDetails),
                TotalProducerFeeWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2b(calcResult, producer, runProducerMaterialDetails),
                EnglandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebt(calcResult, producer, runProducerMaterialDetails),
                WalesTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebt(calcResult, producer, runProducerMaterialDetails),
                ScotlandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebt(calcResult, producer, runProducerMaterialDetails),
                NorthernIrelandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebt(calcResult, producer, runProducerMaterialDetails),

                //Section-3
                // Percentage of Producer Reported Household Tonnage vs All Producers
                PercentageofProducerReportedHHTonnagevsAllProducers = HHTonnageVsAllProducerUtil.GetPercentageofProducerReportedHHTonnagevsAllProducers(producer, runProducerMaterialDetails),
            };

            TwoCCommsCostUtil.UpdateTwoCRows(calcResult, result, producer, runProducerMaterialDetails);

            return result;
        }
    }
}