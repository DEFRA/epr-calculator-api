using EPR.Calculator.API.Builder.Summary.Common;
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
using System.Diagnostics;

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
            var runId = resultsRequestDto.RunId;
            var materialsFromDb = await context.Material.ToListAsync();
            var materials = Mappers.MaterialMapper.Map(materialsFromDb);

            var runProducerMaterialDetails = await (from pd in context.ProducerDetail
                                                    join prm in context.ProducerReportedMaterial on pd.Id equals prm.ProducerDetailId
                                                    where pd.CalculatorRunId == runId
                                                    select new CalcResultsProducerAndReportMaterialDetail
                                                    {
                                                        ProducerDetail = pd,
                                                        ProducerReportedMaterial = prm
                                                    }).ToListAsync();
            var producerDetails = runProducerMaterialDetails.Select(x => x.ProducerDetail).Distinct().ToList();

            var orderedProducerDetails = GetOrderedListOfProducersAssociatedRunId(
                runId, producerDetails);

            var hhTotalPackagingTonnage = GetHHTotalPackagingTonnagePerRun(runProducerMaterialDetails, runId);

            var result = GetCalcResultSummary(
                orderedProducerDetails,
                materials,
                runProducerMaterialDetails,
                calcResult, hhTotalPackagingTonnage);

            return result;
        }

        public static CalcResultSummary GetCalcResultSummary(
            IEnumerable<ProducerDetail> orderedProducerDetails,
            IEnumerable<MaterialDetail> materials,
            IEnumerable<CalcResultsProducerAndReportMaterialDetail> runProducerMaterialDetails,
            CalcResult calcResult, IEnumerable<HHTotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
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
                            runProducerMaterialDetails, producerDisposalFees, false, hhTotalPackagingTonnage);
                        producerDisposalFees.Add(totalRow);
                    }

                    // Calculate the values for the producer
                    producerDisposalFees.Add(GetProducerRow(producerDisposalFees, producer, materials, calcResult,
                        runProducerMaterialDetails, hhTotalPackagingTonnage));
                }


                // Calculate the total for all the producers
                var allTotalRow = GetProducerTotalRow(orderedProducerDetails.ToList(), materials, calcResult,
                    runProducerMaterialDetails, producerDisposalFees, true, hhTotalPackagingTonnage);
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
                result.CommsCostHeaderBadDebtProvisionFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderBadDebtProvisionFor2bTitle(calcResult, result);
                result.CommsCostHeaderWithBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithBadDebtFor2bTitle(result);

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
            bool isOverAllTotalRow,
            IEnumerable<HHTotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            var materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();
            var commsCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>();

            foreach (var material in materials)
            {
                var materialSummary = new CalcResultSummaryProducerDisposalFeesByMaterial
                {
                    HouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetHouseholdPackagingWasteTonnageProducerTotal(producersAndSubsidiaries, material),
                    PublicBinTonnage = CalcResultSummaryUtil.GetPublicBinTonnageProducerTotal(producersAndSubsidiaries, material),
                };

                if (material.Code == "GL")
                {
                    materialSummary.HouseholdDrinksContainersTonnage = CalcResultSummaryUtil.GetHouseholdDrinksContainersTonnageProducerTotal(producersAndSubsidiaries, material);
                }

                materialSummary.TotalReportedTonnage = CalcResultSummaryUtil.GetReportedTonnageProducerTotal(producersAndSubsidiaries, material);
                materialSummary.ManagedConsumerWasteTonnage = CalcResultSummaryUtil.GetManagedConsumerWasteTonnageProducerTotal(producersAndSubsidiaries, material);
                materialSummary.NetReportedTonnage = CalcResultSummaryUtil.GetNetReportedTonnageProducerTotal(producersAndSubsidiaries, material);
                materialSummary.PricePerTonne = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult);
                materialSummary.ProducerDisposalFee = CalcResultSummaryUtil.GetProducerDisposalFeeProducerTotal(producersAndSubsidiaries, material, calcResult);
                materialSummary.BadDebtProvision = CalcResultSummaryUtil.GetBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult);
                materialSummary.ProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult);
                materialSummary.EnglandWithBadDebtProvision = CalcResultSummaryUtil.GetEnglandWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult);
                materialSummary.WalesWithBadDebtProvision = CalcResultSummaryUtil.GetWalesWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult);
                materialSummary.ScotlandWithBadDebtProvision = CalcResultSummaryUtil.GetScotlandWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult);
                materialSummary.NorthernIrelandWithBadDebtProvision = CalcResultSummaryUtil.GetNorthernIrelandWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult);

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
                TotalProducerFeeWithoutBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2bTotalsRow(calcResult, producersAndSubsidiaries, hhTotalPackagingTonnage),
                BadDebtProvisionFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2bTotalsRow(calcResult, producersAndSubsidiaries, hhTotalPackagingTonnage),
                TotalProducerFeeWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2bTotalsRow(calcResult, producersAndSubsidiaries, hhTotalPackagingTonnage),
                EnglandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, hhTotalPackagingTonnage),
                WalesTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, hhTotalPackagingTonnage),
                ScotlandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, hhTotalPackagingTonnage),
                NorthernIrelandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, hhTotalPackagingTonnage),

                //Section-3
                // Percentage of Producer Reported Household Tonnage vs All Producers
                PercentageofProducerReportedHHTonnagevsAllProducers = HHTonnageVsAllProducerUtil.GetPercentageofProducerReportedHHTonnagevsAllProducersTotal(producersAndSubsidiaries, hhTotalPackagingTonnage),

                isTotalRow = true
            };

            TwoCCommsCostUtil.UpdateTwoCTotals(calcResult, producerDisposalFees, isOverAllTotalRow, totalRow,
                producersAndSubsidiaries, hhTotalPackagingTonnage);

            return totalRow;
        }

        public static CalcResultSummaryProducerDisposalFees GetProducerRow(
            List<CalcResultSummaryProducerDisposalFees> producerDisposalFeesLookup,
            ProducerDetail producer,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            IEnumerable<CalcResultsProducerAndReportMaterialDetail> runProducerMaterialDetails,
            IEnumerable<HHTotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            var materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();
            var commsCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>();
            var result = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = producer.ProducerId.ToString(),
                ProducerName = producer.ProducerName ?? string.Empty,
                SubsidiaryId = producer.SubsidiaryId ?? string.Empty,
                Level = CalcResultSummaryUtil.GetLevelIndex(producerDisposalFeesLookup, producer).ToString()
            };

            foreach (var material in materials)
            {
                var calcResultSummaryProducerDisposalFeesByMaterial = new CalcResultSummaryProducerDisposalFeesByMaterial();
                if (material.Code == "GL")
                {
                    calcResultSummaryProducerDisposalFeesByMaterial.HouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetHouseholdPackagingWasteTonnage(producer, material);
                    calcResultSummaryProducerDisposalFeesByMaterial.PublicBinTonnage = CalcResultSummaryUtil.GetPublicBinTonnage(producer, material);
                    calcResultSummaryProducerDisposalFeesByMaterial.HouseholdDrinksContainersTonnage = CalcResultSummaryUtil.GetHouseholdDrinksContainersTonnage(producer, material);
                }
                else
                {
                    calcResultSummaryProducerDisposalFeesByMaterial.HouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetHouseholdPackagingWasteTonnage(producer, material);
                    calcResultSummaryProducerDisposalFeesByMaterial.PublicBinTonnage = CalcResultSummaryUtil.GetPublicBinTonnage(producer, material);
                }

                calcResultSummaryProducerDisposalFeesByMaterial.TotalReportedTonnage = CalcResultSummaryUtil.GetReportedTonnage(producer, material);
                calcResultSummaryProducerDisposalFeesByMaterial.ManagedConsumerWasteTonnage = CalcResultSummaryUtil.GetManagedConsumerWasteTonnage(producer, material);
                calcResultSummaryProducerDisposalFeesByMaterial.NetReportedTonnage = CalcResultSummaryUtil.GetNetReportedTonnage(producer, material);
                calcResultSummaryProducerDisposalFeesByMaterial.PricePerTonne = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult);
                calcResultSummaryProducerDisposalFeesByMaterial.ProducerDisposalFee = CalcResultSummaryUtil.GetProducerDisposalFee(producer, material, calcResult);
                calcResultSummaryProducerDisposalFeesByMaterial.BadDebtProvision = CalcResultSummaryUtil.GetBadDebtProvision(producer, material, calcResult);
                calcResultSummaryProducerDisposalFeesByMaterial.ProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);
                calcResultSummaryProducerDisposalFeesByMaterial.EnglandWithBadDebtProvision = CalcResultSummaryUtil.GetEnglandWithBadDebtProvision(producer, material, calcResult);
                calcResultSummaryProducerDisposalFeesByMaterial.WalesWithBadDebtProvision = CalcResultSummaryUtil.GetWalesWithBadDebtProvision(producer, material, calcResult);
                calcResultSummaryProducerDisposalFeesByMaterial.ScotlandWithBadDebtProvision = CalcResultSummaryUtil.GetScotlandWithBadDebtProvision(producer, material, calcResult);
                calcResultSummaryProducerDisposalFeesByMaterial.NorthernIrelandWithBadDebtProvision = CalcResultSummaryUtil.GetNorthernIrelandWithBadDebtProvision(producer, material, calcResult);

                materialCostSummary.Add(material, calcResultSummaryProducerDisposalFeesByMaterial);

                result.TotalProducerDisposalFee += calcResultSummaryProducerDisposalFeesByMaterial.ProducerDisposalFee;
                result.BadDebtProvision += calcResultSummaryProducerDisposalFeesByMaterial.BadDebtProvision;
                result.TotalProducerDisposalFeeWithBadDebtProvision += calcResultSummaryProducerDisposalFeesByMaterial.ProducerDisposalFeeWithBadDebtProvision;
                result.EnglandTotal += calcResultSummaryProducerDisposalFeesByMaterial.EnglandWithBadDebtProvision;
                result.WalesTotal += calcResultSummaryProducerDisposalFeesByMaterial.WalesWithBadDebtProvision;
                result.ScotlandTotal += calcResultSummaryProducerDisposalFeesByMaterial.ScotlandWithBadDebtProvision;
                result.NorthernIrelandTotal += calcResultSummaryProducerDisposalFeesByMaterial.NorthernIrelandWithBadDebtProvision;

                var calcResultSummaryProducerCommsFeesCostByMaterial = new CalcResultSummaryProducerCommsFeesCostByMaterial
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
                };

                commsCostSummary.Add(material, calcResultSummaryProducerCommsFeesCostByMaterial);

                result.TotalProducerCommsFee += calcResultSummaryProducerCommsFeesCostByMaterial.ProducerTotalCostWithoutBadDebtProvision;
                result.BadDebtProvisionComms += calcResultSummaryProducerCommsFeesCostByMaterial.BadDebtProvision;
                result.TotalProducerCommsFeeWithBadDebtProvision += calcResultSummaryProducerCommsFeesCostByMaterial.ProducerTotalCostwithBadDebtProvision;
                result.EnglandTotalComms += calcResultSummaryProducerCommsFeesCostByMaterial.EnglandWithBadDebtProvision;
                result.WalesTotalComms += calcResultSummaryProducerCommsFeesCostByMaterial.WalesWithBadDebtProvision;
                result.ScotlandTotalComms += calcResultSummaryProducerCommsFeesCostByMaterial.ScotlandWithBadDebtProvision;
                result.NorthernIrelandTotalComms += calcResultSummaryProducerCommsFeesCostByMaterial.NorthernIrelandWithBadDebtProvision;
            }

            result.ProducerDisposalFeesByMaterial = materialCostSummary;
            result.ProducerCommsFeesByMaterial = commsCostSummary;

            //Section-(1) & (2a)
            result.TotalProducerFeeforLADisposalCostswoBadDebtprovision = result.TotalProducerDisposalFee;
            result.BadDebtProvisionFor1 = result.BadDebtProvision;
            result.TotalProducerFeeforLADisposalCostswithBadDebtprovision = result.TotalProducerDisposalFeeWithBadDebtProvision;
            result.EnglandTotalWithBadDebtProvision = result.EnglandTotal;
            result.WalesTotalWithBadDebtProvision = result.WalesTotal;
            result.ScotlandTotalWithBadDebtProvision = result.ScotlandTotal;
            result.NorthernIrelandTotalWithBadDebtProvision = result.NorthernIrelandTotal;

            result.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision = result.TotalProducerCommsFee;
            result.BadDebtProvisionFor2A = result.BadDebtProvisionComms;
            result.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision = result.TotalProducerCommsFeeWithBadDebtProvision;
            result.EnglandTotalWithBadDebtProvision2A = result.EnglandTotalComms;
            result.WalesTotalWithBadDebtProvision2A = result.WalesTotalComms;
            result.ScotlandTotalWithBadDebtProvision2A = result.ScotlandTotalComms;
            result.NorthernIrelandTotalWithBadDebtProvision2A = result.NorthernIrelandTotalComms;


            //Section-3
            // Percentage of Producer Reported Household Tonnage vs All Producers
            result.PercentageofProducerReportedHHTonnagevsAllProducers = HHTonnageVsAllProducerUtil.GetPercentageofProducerReportedHHTonnagevsAllProducers(producer, hhTotalPackagingTonnage);

            ////Total Bill for 2b
            result.TotalProducerFeeWithoutBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, hhTotalPackagingTonnage);
            result.BadDebtProvisionFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2b(calcResult, producer, hhTotalPackagingTonnage);
            result.TotalProducerFeeWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2b(calcResult, producer, hhTotalPackagingTonnage);
            result.EnglandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebt(calcResult, producer, hhTotalPackagingTonnage);
            result.WalesTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebt(calcResult, producer, hhTotalPackagingTonnage);
            result.ScotlandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebt(calcResult, producer, hhTotalPackagingTonnage);
            result.NorthernIrelandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebt(calcResult, producer, hhTotalPackagingTonnage);

            TwoCCommsCostUtil.UpdateTwoCRows(calcResult, result, producer, hhTotalPackagingTonnage);

            return result;
        }

        public static IEnumerable<HHTotalPackagingTonnagePerRun> GetHHTotalPackagingTonnagePerRun(IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults, int runId)
        {
            var allProducerDetails = allResults.Select(x => x.ProducerDetail);
            var allProducerReportedMaterials = allResults.Select(x => x.ProducerReportedMaterial);

            var result =
                (from p in allProducerDetails
                 join m in allProducerReportedMaterials
                     on p.Id equals m.ProducerDetailId
                 where p.CalculatorRunId == runId && m.PackagingType == "HH"
                 group new { m, p } by new { p.ProducerId, p.SubsidiaryId }
                    into g
                 select new HHTotalPackagingTonnagePerRun
                 {
                     ProducerId = g.Key.ProducerId,
                     SubsidiaryId = g.Key.SubsidiaryId,
                     TotalPackagingTonnage = g.Sum(x => x.m.PackagingTonnage)
                 }).ToList();

            return result;
        }
    }
}