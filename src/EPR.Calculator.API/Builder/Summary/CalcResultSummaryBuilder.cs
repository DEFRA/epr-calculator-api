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

namespace EPR.Calculator.API.Builder.Summary
{
    public class CalcResultSummaryBuilder : ICalcResultSummaryBuilder
    {
        private readonly ApplicationDBContext context;

        public static List<ProducerDetail> producerDetailList { get; set; }

        public CalcResultSummaryBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public CalcResultSummary Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult)
        {
            var result = new CalcResultSummary();

            // Get and map materials from DB
            var materialsFromDb = context.Material.ToList();
            var materials = Mappers.MaterialMapper.Map(materialsFromDb);

            var runProducerMaterialDetails = (from p in context.ProducerDetail
                                              join m in context.ProducerReportedMaterial
                                                  on p.Id equals m.ProducerDetailId
                                              where p.CalculatorRunId == resultsRequestDto.RunId
                                              select new CalcResultsProducerAndReportMaterialDetail
                                              {
                                                  ProducerDetail = p,
                                                  ProducerReportedMaterial = m
                                              }).ToList();

            // Get the ordered list of producers associated with the calculator run id
            producerDetailList = context.ProducerDetail
              .Where(pd => pd.CalculatorRunId == resultsRequestDto.RunId)
              .OrderBy(pd => pd.ProducerId)
              .ToList();

            if (producerDetailList.Count > 0)
            {
                var producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>();

                foreach (var producer in producerDetailList)
                {
                    // We have to write an additional row if a producer have at least one subsidiary
                    // This additional row will be the total of this producer and its subsidiaries
                    var producersAndSubsidiaries = producerDetailList.Where(pd => pd.ProducerId == producer.ProducerId);
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
                var allTotalRow = GetProducerTotalRow(producerDetailList.ToList(), materials, calcResult,
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

                // SA Operating cost Section 3 -this will display the total at top -Working Row100
                result.SAOperatingCostsWoTitleSection3 = ThreeSaUtil.GetSAOperatingCostsTotalWithoutBadDebtProvisionTitleSection3(calcResult);
                result.BadDebtProvisionTitleSection3 = ThreeSaUtil.GetSAOperatingCostsBadDebtProvisionTitleSection3(result.SAOperatingCostsWoTitleSection3, calcResult);
                result.SAOperatingCostsWithTitleSection3 = ThreeSaUtil.GetSAOperatingCostsTotalWithBadDebtProvisionTitleSection3(result);

                // Section-4 LA data prep costs
                LaDataPrepCostsProducer.SetValues(calcResult, result);

                // Section-5 SA setup costs
                result.SaSetupCostsTitleSection5 = SaSetupCostsSummary.GetOneOffFeeSetupCostsWithoutBadDebtProvision(calcResult);
                result.SaSetupCostsBadDebtProvisionTitleSection5 = SaSetupCostsSummary.GetBadDebtProvision(calcResult);
                result.SaSetupCostsWithBadDebtProvisionTitleSection5 = SaSetupCostsSummary.GetOneOffFeeSetupCostsWithBadDebtProvision(calcResult);
            }

            // Set headers with calculated column index
            CalcResultSummaryUtil.SetHeaders(result, materials);

            return result;
        }

        private CalcResultSummaryProducerDisposalFees GetProducerTotalRow(List<ProducerDetail> producersAndSubsidiaries,
            List<MaterialDetail> materials,
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
                Total3SAOperatingCostwoBadDebtprovision = ThreeSaUtil.GetSAOperatingCostsTotalWithoutBadDebtProvisionPrtoducerTotalSection3(materialCostSummary, commsCostSummary, materials, calcResult),
                BadDebtProvisionFor3 = ThreeSaUtil.GetBadDebtProvisionProducerTotalSection3(materialCostSummary, commsCostSummary, materials, calcResult),
                Total3SAOperatingCostswithBadDebtprovision = ThreeSaUtil.GetSAOperatingCostsTotalWithBadDebtProvisionPrtoducerTotalSection3(materialCostSummary, commsCostSummary, materials, calcResult),
                EnglandTotalWithBadDebtProvision3 = ThreeSaUtil.GetSAOperatingCostsEnglandTotalWithBadDebtProvisionSection3(materialCostSummary, commsCostSummary, materials, calcResult),
                WalesTotalWithBadDebtProvision3 = ThreeSaUtil.GetSAOperatingCostsWalesTotalWithBadDebtProvisionSection3(materialCostSummary, commsCostSummary, materials, calcResult),
                ScotlandTotalWithBadDebtProvision3 = ThreeSaUtil.GetSAOperatingCostsScotlandTotalWithBadDebtProvisionSection3(materialCostSummary, commsCostSummary, materials, calcResult),
                NorthernIrelandTotalWithBadDebtProvision3 = ThreeSaUtil.GetSAOperatingCostsNITotalWithBadDebtProvisionSection3(materialCostSummary, commsCostSummary, materials, calcResult),

                // Section-5 SA setup costs
                TotalProducerFeeWithoutBadDebtProvisionSection5 = SaSetupCostsProducer.GetProducerOneOffFeeWithoutBadDebtProvisionTotal(producerDetailList, producersAndSubsidiaries, materials, calcResult),
                BadDebtProvisionSection5 = SaSetupCostsProducer.GetBadDebtProvisionTotal(producerDetailList, producersAndSubsidiaries, materials, calcResult),
                TotalProducerFeeWithBadDebtProvisionSection5 = SaSetupCostsProducer.GetProducerOneOffFeeWithBadDebtProvisionTotal(producerDetailList, producersAndSubsidiaries, materials, calcResult),
                EnglandTotalWithBadDebtProvisionSection5 = SaSetupCostsProducer.GetEnglandOverallTotalWithBadDebtProvision(producerDetailList, producersAndSubsidiaries, materials, calcResult),
                WalesTotalWithBadDebtProvisionSection5 = SaSetupCostsProducer.GetWalesOverallTotalWithBadDebtProvision(producerDetailList, producersAndSubsidiaries, materials, calcResult),
                ScotlandTotalWithBadDebtProvisionSection5 = SaSetupCostsProducer.GetScotlandOverallTotalWithBadDebtProvision(producerDetailList, producersAndSubsidiaries, materials, calcResult),
                NorthernIrelandTotalWithBadDebtProvisionSection5 = SaSetupCostsProducer.GetNorthernIrelandOverallTotalWithBadDebtProvision(producerDetailList, producersAndSubsidiaries, materials, calcResult),

                // Percentage of Producer Reported Household Tonnage vs All Producers
                PercentageofProducerReportedHHTonnagevsAllProducers = HHTonnageVsAllProducerUtil.GetPercentageofProducerReportedHHTonnagevsAllProducersTotal(producersAndSubsidiaries, runProducerMaterialDetails),

                isTotalRow = true
            };

            TwoCCommsCostUtil.UpdateTwoCTotals(calcResult, producerDisposalFees, isOverAllTotalRow, totalRow,
                producersAndSubsidiaries, runProducerMaterialDetails);

            // Section-TotalBill
            totalRow.TotalProducerBillWithoutBadDebtProvision = TotalBillBreakdownProducer.GetTotalProducerBillWithoutBadDebtProvisionTotal(totalRow);
            totalRow.BadDebtProvisionForTotalProducerBill = TotalBillBreakdownProducer.GetBadDebtProvisionForTotalProducerBillTotal(totalRow);
            totalRow.TotalProducerBillWithBadDebtProvision = TotalBillBreakdownProducer.GetTotalProducerBillWithBadDebtProvisionTotal(totalRow);
            totalRow.EnglandTotalWithBadDebtProvisionTotalBill = TotalBillBreakdownProducer.GetEnglandTotalWithBadDebtProvisionTotal(totalRow);
            totalRow.WalesTotalWithBadDebtProvisionTotalBill = TotalBillBreakdownProducer.GetWalesTotalWithBadDebtProvisionTotal(totalRow);
            totalRow.ScotlandTotalWithBadDebtProvisionTotalBill = TotalBillBreakdownProducer.GetScotlandTotalWithBadDebtProvisionTotal(totalRow);
            totalRow.NorthernIrelandTotalWithBadDebtProvisionTotalBill = TotalBillBreakdownProducer.GetNorthernIrelandTotalWithBadDebtProvisionTotal(totalRow);

            return totalRow;
        }

        private CalcResultSummaryProducerDisposalFees GetProducerRow(
            List<CalcResultSummaryProducerDisposalFees> producerDisposalFeesLookup,
            ProducerDetail producer,
            List<MaterialDetail> materials,
            CalcResult calcResult,
            List<CalcResultsProducerAndReportMaterialDetail> runProducerMaterialDetails)
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
                Total3SAOperatingCostwoBadDebtprovision = ThreeSaUtil.GetSAOperatingCostsTotalWithoutBadDebtProvisionPrtoducerTotalSection3(materialCostSummary, commsCostSummary, materials, calcResult),
                BadDebtProvisionFor3 = ThreeSaUtil.GetBadDebtProvisionProducerTotalSection3(materialCostSummary, commsCostSummary, materials, calcResult),
                Total3SAOperatingCostswithBadDebtprovision = ThreeSaUtil.GetSAOperatingCostsTotalWithBadDebtProvisionPrtoducerTotalSection3(materialCostSummary, commsCostSummary, materials, calcResult),
                EnglandTotalWithBadDebtProvision3 = ThreeSaUtil.GetSAOperatingCostsEnglandTotalWithBadDebtProvisionSection3(materialCostSummary, commsCostSummary, materials, calcResult),
                WalesTotalWithBadDebtProvision3 = ThreeSaUtil.GetSAOperatingCostsWalesTotalWithBadDebtProvisionSection3(materialCostSummary, commsCostSummary, materials, calcResult),
                ScotlandTotalWithBadDebtProvision3 = ThreeSaUtil.GetSAOperatingCostsScotlandTotalWithBadDebtProvisionSection3(materialCostSummary, commsCostSummary, materials, calcResult),
                NorthernIrelandTotalWithBadDebtProvision3 = ThreeSaUtil.GetSAOperatingCostsNITotalWithBadDebtProvisionSection3(materialCostSummary, commsCostSummary, materials, calcResult),

                // Scheme administrator costs section 5
                TotalProducerFeeWithoutBadDebtProvisionSection5 = SaSetupCostsProducer.GetProducerOneOffFeeWithoutBadDebtProvision(producerDetailList, materials, calcResult, materialCostSummary, commsCostSummary),
                BadDebtProvisionSection5 = SaSetupCostsProducer.GetBadDebtProvision(producerDetailList, materials, calcResult, materialCostSummary, commsCostSummary),
                TotalProducerFeeWithBadDebtProvisionSection5 = SaSetupCostsProducer.GetProducerOneOffFeeWithBadDebtProvision(producerDetailList, materials, calcResult, materialCostSummary, commsCostSummary),
                EnglandTotalWithBadDebtProvisionSection5 = SaSetupCostsProducer.GetEnglandTotalWithBadDebtProvision(producerDetailList, materials, calcResult, materialCostSummary, commsCostSummary),
                WalesTotalWithBadDebtProvisionSection5 = SaSetupCostsProducer.GetWalesTotalWithBadDebtProvision(producerDetailList, materials, calcResult, materialCostSummary, commsCostSummary),
                ScotlandTotalWithBadDebtProvisionSection5 = SaSetupCostsProducer.GetScotlandTotalWithBadDebtProvision(producerDetailList, materials, calcResult, materialCostSummary, commsCostSummary),
                NorthernIrelandTotalWithBadDebtProvisionSection5 = SaSetupCostsProducer.GetNorthernIrelandTotalWithBadDebtProvision(producerDetailList, materials, calcResult, materialCostSummary, commsCostSummary),

                // Percentage of Producer Reported Household Tonnage vs All Producers
                PercentageofProducerReportedHHTonnagevsAllProducers = HHTonnageVsAllProducerUtil.GetPercentageofProducerReportedHHTonnagevsAllProducers(producer, runProducerMaterialDetails),

            };

            TwoCCommsCostUtil.UpdateTwoCRows(calcResult, result, producer, runProducerMaterialDetails);

            // section-TotalBill
            result.TotalProducerBillWithoutBadDebtProvision = TotalBillBreakdownProducer.GetTotalProducerBillWithoutBadDebtProvision(result);
            result.BadDebtProvisionForTotalProducerBill = TotalBillBreakdownProducer.GetBadDebtProvisionForTotalProducerBill(result);
            result.TotalProducerBillWithBadDebtProvision = TotalBillBreakdownProducer.GetTotalProducerBillWithBadDebtProvision(result);
            result.EnglandTotalWithBadDebtProvisionTotalBill = TotalBillBreakdownProducer.GetEnglandTotalWithBadDebtProvision(result);
            result.WalesTotalWithBadDebtProvisionTotalBill = TotalBillBreakdownProducer.GetWalesTotalWithBadDebtProvision(result);
            result.ScotlandTotalWithBadDebtProvisionTotalBill = TotalBillBreakdownProducer.GetScotlandTotalWithBadDebtProvision(result);
            result.NorthernIrelandTotalWithBadDebtProvisionTotalBill = TotalBillBreakdownProducer.GetNorthernIrelandTotalWithBadDebtProvision(result);

            return result;
        }

        //section bad debt total
        public static decimal GetTotal1Plus2ABadDebt(IEnumerable<MaterialDetail> materials, CalcResult calcResult)
        {
            decimal total = 0m;

            foreach (MaterialDetail material in materials)
            {
                var laDisposalTotal = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(producerDetailList, material, calcResult);
                var twoAcommsDisposal = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvisionTotal(producerDetailList, material, calcResult);
                total += laDisposalTotal + twoAcommsDisposal;
            }

            return total;
        }

        public static decimal GetTotal1Plus2ABadDebtPercentage(decimal totalLaDisposal, decimal total2aCommsCost, List<MaterialDetail> materials, CalcResult calcResult)
        {
            var total = GetTotal1Plus2ABadDebt(materials, calcResult);

            if (total == 0) return 0;

            return Math.Round((totalLaDisposal + total2aCommsCost) / total * 100, 8);

        }
    }
}