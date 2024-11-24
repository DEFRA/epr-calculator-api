using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using System.Globalization;

namespace EPR.Calculator.API.Builder.Summary
{
    public class CalcResultSummaryBuilder : ICalcResultSummaryBuilder
    {
        private readonly ApplicationDBContext context;

        private const int ResultSummaryHeaderColumnIndex = 1;
        private const int ProducerDisposalFeesHeaderColumnIndex = 5;
        private const int CommsCostHeaderColumnIndex = 100;
        private const int MaterialsBreakdownHeaderInitialColumnIndex = 5;
        private const int MaterialsBreakdownHeaderIncrementalColumnIndex = 11;
        private const int DisposalFeeSummaryColumnIndex = 93;
        private const int LaDataPrepCostsSection4ColumnIndex = 216;
        private const int MaterialsBreakdownHeaderCommsInitialColumnIndex = 100;
        private const int MaterialsBreakdownHeaderCommsIncrementalColumnIndex = 9;
        //Section-(1) & (2a)
        private const int decimalRoundUp = 2;
        private const int DisposalFeeCommsCostsHeaderInitialColumnIndex = 179;

        // Percentage of Producer Reported Household Tonnage vs All Producers
        private const int PercentageofProducerReportedHHTonnageColumnIndex = 193;

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


            var runProducerMaterialDetails =  (from p in context.ProducerDetail
                    join m in context.ProducerReportedMaterial
                        on p.Id equals m.ProducerDetailId
                    where p.CalculatorRunId == resultsRequestDto.RunId
                    select new CalcResultsProducerAndReportMaterialDetail
                    {
                        ProducerDetail = p,
                        ProducerReportedMaterial = m
                    }).ToList();

            // Get the ordered list of producers associated with the calculator run id
            var producerDetailList = runProducerMaterialDetails.Select(x => x.ProducerDetail).Distinct()
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
                            runProducerMaterialDetails);
                        producerDisposalFees.Add(totalRow);
                    }

                    // Calculate the values for the producer
                    producerDisposalFees.Add(GetProducerRow(producerDisposalFees, producer, materials, calcResult,
                        runProducerMaterialDetails));
                }

                // Calculate the total for all the producers
                producerDisposalFees.Add(GetProducerTotalRow(producerDetailList.ToList(), materials, calcResult,
                    runProducerMaterialDetails, true));

                result.ProducerDisposalFees = producerDisposalFees;

                //Section-(1) & (2a)
                result.TotalFeeforLADisposalCostswoBadDebtprovision1 =
                    GetTotalDisposalCostswoBadDebtprovision1(producerDisposalFees);
                result.BadDebtProvisionFor1 = GetTotalBadDebtprovision1(producerDisposalFees);
                result.TotalFeeforLADisposalCostswithBadDebtprovision1 =
                    GetTotalDisposalCostswithBadDebtprovision1(producerDisposalFees);

                result.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A =
                    GetTotalCommsCostswoBadDebtprovision2A(producerDisposalFees);
                result.BadDebtProvisionFor2A = GetTotalBadDebtprovision2A(producerDisposalFees);
                result.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A =
                    GetTotalCommsCostswithBadDebtprovision2A(producerDisposalFees);

                // LA data prep costs section 4
                result.LaDataPrepCostsTitleSection4 = GetLaDataPrepCostsTitleSection4(calcResult);
                result.LaDataPrepCostsBadDebtProvisionTitleSection4 =
                    GetLaDataPrepCostsBadDebtProvisionTitleSection4(calcResult);
                result.LaDataPrepCostsWithBadDebtProvisionTitleSection4 =
                    GetLaDataPrepCostsWithBadDebtProvisionTitleSection4(calcResult);
            }

            // Set headers with calculated column index
            SetHeaders(result, materials);

            return result;
        }

        private CalcResultSummaryProducerDisposalFees GetProducerTotalRow(
            List<ProducerDetail> producersAndSubsidiaries, 
            List<MaterialDetail> materials,
            CalcResult calcResult,
            IEnumerable<CalcResultsProducerAndReportMaterialDetail> runProducerMaterialDetails,
            bool isOverAllTotalRow = false)
        {
            var materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();
            var commsCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>();

            foreach (var material in materials)
            {
                materialCostSummary.Add(material, new CalcResultSummaryProducerDisposalFeesByMaterial
                {
                    HouseholdPackagingWasteTonnage =
                        GetHouseholdPackagingWasteTonnageProducerTotal(producersAndSubsidiaries, material),
                    ManagedConsumerWasteTonnage =
                        GetManagedConsumerWasteTonnageProducerTotal(producersAndSubsidiaries, material),
                    NetReportedTonnage = GetNetReportedTonnageProducerTotal(producersAndSubsidiaries, material),
                    PricePerTonne = GetPricePerTonne(material, calcResult),
                    ProducerDisposalFee =
                        GetProducerDisposalFeeProducerTotal(producersAndSubsidiaries, material, calcResult),
                    BadDebtProvision = GetBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult),
                    ProducerDisposalFeeWithBadDebtProvision =
                        GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material,
                            calcResult),
                    EnglandWithBadDebtProvision =
                        GetEnglandWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult),
                    WalesWithBadDebtProvision =
                        GetWalesWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult),
                    ScotlandWithBadDebtProvision =
                        GetScotlandWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult),
                    NorthernIrelandWithBadDebtProvision =
                        GetNorthernIrelandWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material,
                            calcResult)
                });

                commsCostSummary.Add(material, new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage =
                        GetHouseholdPackagingWasteTonnageProducerTotal(producersAndSubsidiaries, material),
                    PriceperTonne = GetPriceperTonneForComms(material, calcResult),
                    ProducerTotalCostWithoutBadDebtProvision =
                        GetProducerTotalCostWithoutBadDebtProvisionTotal(producersAndSubsidiaries, material,
                            calcResult),
                    BadDebtProvision =
                        GetBadDebtProvisionForCommsCostTotal(producersAndSubsidiaries, material, calcResult),
                    ProducerTotalCostwithBadDebtProvision =
                        GetProducerTotalCostwithBadDebtProvisionTotal(producersAndSubsidiaries, material, calcResult),
                    EnglandWithBadDebtProvision =
                        GetEnglandWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material, calcResult),
                    WalesWithBadDebtProvision =
                        GetWalesWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material, calcResult),
                    ScotlandWithBadDebtProvision =
                        GetScotlandWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material, calcResult),
                    NorthernIrelandWithBadDebtProvision =
                        GetNorthernIrelandWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material,
                            calcResult)
                });

            }

            return new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = isOverAllTotalRow ? string.Empty : producersAndSubsidiaries[0].ProducerId.ToString(),
                ProducerName = isOverAllTotalRow
                    ? string.Empty
                    : producersAndSubsidiaries[0].ProducerName ?? string.Empty,
                SubsidiaryId = string.Empty,
                Level = isOverAllTotalRow ? "Totals" : "1",
                ProducerDisposalFeesByMaterial = materialCostSummary,

                // Disposal fee summary
                TotalProducerDisposalFee = GetTotalProducerDisposalFee(materialCostSummary),
                BadDebtProvision = GetTotalBadDebtProvision(materialCostSummary),
                TotalProducerDisposalFeeWithBadDebtProvision =
                    GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary),
                EnglandTotal = GetEnglandTotal(materialCostSummary),
                WalesTotal = GetWalesTotal(materialCostSummary),
                ScotlandTotal = GetScotlandTotal(materialCostSummary),
                NorthernIrelandTotal = GetNorthernIrelandTotal(materialCostSummary),

                //For Comms Start
                TotalProducerCommsFee = GetTotalProducerCommsFee(commsCostSummary),
                BadDebtProvisionComms = GetCommsTotalBadDebtProvision(commsCostSummary),
                TotalProducerCommsFeeWithBadDebtProvision =
                    GetTotalProducerCommsFeeWithBadDebtProvision(commsCostSummary),
                EnglandTotalComms = GetEnglandCommsTotal(commsCostSummary),
                WalesTotalComms = GetWalesCommsTotal(commsCostSummary),
                ScotlandTotalComms = GetScotlandCommsTotal(commsCostSummary),
                NorthernIrelandTotalComms = GetNorthernIrelandCommsTotal(commsCostSummary),
                ProducerCommsFeesByMaterial = commsCostSummary,

                //Section-(1) & (2a)
                TotalProducerFeeforLADisposalCostswoBadDebtprovision = GetTotalProducerDisposalFee(materialCostSummary),
                BadDebtProvisionFor1 = GetTotalBadDebtProvision(materialCostSummary),
                TotalProducerFeeforLADisposalCostswithBadDebtprovision =
                    GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary),
                EnglandTotalwithBadDebtprovision = GetEnglandTotal(materialCostSummary),
                WalesTotalwithBadDebtprovision = GetWalesTotal(materialCostSummary),
                ScotlandTotalwithBadDebtprovision = GetScotlandTotal(materialCostSummary),
                NorthernIrelandTotalwithBadDebtprovision = GetNorthernIrelandTotal(materialCostSummary),

                TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision = GetTotalProducerCommsFee(commsCostSummary),
                BadDebtProvisionFor2A = GetTotalBadDebtProvision(materialCostSummary),
                TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision =
                    GetTotalProducerCommsFeeWithBadDebtProvision(commsCostSummary),
                EnglandTotalwithBadDebtprovision2A = GetEnglandCommsTotal(commsCostSummary),
                WalesTotalwithBadDebtprovision2A = GetWalesCommsTotal(commsCostSummary),
                ScotlandTotalwithBadDebtprovision2A = GetScotlandCommsTotal(commsCostSummary),
                NorthernIrelandTotalwithBadDebtprovision2A = GetNorthernIrelandCommsTotal(commsCostSummary),

                // LA data prep costs section 4
                LaDataPrepCostsTotalWithoutBadDebtProvisionSection4 =
                    GetLaDataPrepCostsTotalWithoutBadDebtProvisionSection4(),
                LaDataPrepCostsBadDebtProvisionSection4 = GetLaDataPrepCostsBadDebtProvisionSection4(),
                LaDataPrepCostsTotalWithBadDebtProvisionSection4 =
                    GetLaDataPrepCostsTotalWithBadDebtProvisionSection4(),
                LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4 =
                    GetLaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4(),
                LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4 =
                    GetLaDataPrepCostsWalesTotalWithBadDebtProvisionSection4(),
                LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4 =
                    GetLaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4(),
                LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4 =
                    GetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4(),

                // Percentage of Producer Reported Household Tonnage vs All Producers
                PercentageofProducerReportedHHTonnagevsAllProducers =
                    GetPercentageofProducerReportedHHTonnagevsAllProducersTotal(producersAndSubsidiaries, runProducerMaterialDetails),
                isTotalRow = true
            };

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
                    HouseholdPackagingWasteTonnage = GetHouseholdPackagingWasteTonnage(producer, material),
                    ManagedConsumerWasteTonnage = GetManagedConsumerWasteTonnage(producer, material),
                    NetReportedTonnage = GetNetReportedTonnage(producer, material),
                    PricePerTonne = GetPricePerTonne(material, calcResult),
                    ProducerDisposalFee = GetProducerDisposalFee(producer, material, calcResult),
                    BadDebtProvision = GetBadDebtProvision(producer, material, calcResult),
                    ProducerDisposalFeeWithBadDebtProvision =
                        GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult),
                    EnglandWithBadDebtProvision = GetEnglandWithBadDebtProvision(producer, material, calcResult),
                    WalesWithBadDebtProvision = GetWalesWithBadDebtProvision(producer, material, calcResult),
                    ScotlandWithBadDebtProvision = GetScotlandWithBadDebtProvision(producer, material, calcResult),
                    NorthernIrelandWithBadDebtProvision =
                        GetNorthernIrelandWithBadDebtProvision(producer, material, calcResult)
                });

                commsCostSummary.Add(material, new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = GetHouseholdPackagingWasteTonnage(producer, material),
                    PriceperTonne = GetPriceperTonneForComms(material, calcResult),
                    ProducerTotalCostWithoutBadDebtProvision =
                        GetProducerTotalCostWithoutBadDebtProvision(producer, material, calcResult),
                    BadDebtProvision = GetBadDebtProvisionForCommsCost(producer, material, calcResult),
                    ProducerTotalCostwithBadDebtProvision =
                        GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult),
                    EnglandWithBadDebtProvision =
                        GetEnglandWithBadDebtProvisionForComms(producer, material, calcResult),
                    WalesWithBadDebtProvision = GetWalesWithBadDebtProvisionForComms(producer, material, calcResult),
                    ScotlandWithBadDebtProvision =
                        GetScotlandWithBadDebtProvisionForComms(producer, material, calcResult),
                    NorthernIrelandWithBadDebtProvision =
                        GetNorthernIrelandWithBadDebtProvisionForComms(producer, material, calcResult)
                });

            }

            return new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = producer.ProducerId.ToString(),
                ProducerName = producer.ProducerName ?? string.Empty,
                SubsidiaryId = producer.SubsidiaryId ?? string.Empty,
                Level = GetLevelIndex(producerDisposalFeesLookup, producer).ToString(),
                ProducerDisposalFeesByMaterial = materialCostSummary,

                // Disposal fee summary
                TotalProducerDisposalFee = GetTotalProducerDisposalFee(materialCostSummary),
                BadDebtProvision = GetTotalBadDebtProvision(materialCostSummary),
                TotalProducerDisposalFeeWithBadDebtProvision =
                    GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary),
                EnglandTotal = GetEnglandTotal(materialCostSummary),
                WalesTotal = GetWalesTotal(materialCostSummary),
                ScotlandTotal = GetScotlandTotal(materialCostSummary),
                NorthernIrelandTotal = GetNorthernIrelandTotal(materialCostSummary),

                //For comms
                TotalProducerCommsFee = GetTotalProducerCommsFee(commsCostSummary),
                BadDebtProvisionComms = GetCommsTotalBadDebtProvision(commsCostSummary),
                TotalProducerCommsFeeWithBadDebtProvision =
                    GetTotalProducerCommsFeeWithBadDebtProvision(commsCostSummary),
                EnglandTotalComms = GetEnglandCommsTotal(commsCostSummary),
                WalesTotalComms = GetWalesCommsTotal(commsCostSummary),
                ScotlandTotalComms = GetScotlandCommsTotal(commsCostSummary),
                NorthernIrelandTotalComms = GetNorthernIrelandCommsTotal(commsCostSummary),
                ProducerCommsFeesByMaterial = commsCostSummary,

                //Section-(1) & (2a)
                TotalProducerFeeforLADisposalCostswoBadDebtprovision = GetTotalProducerDisposalFee(materialCostSummary),
                BadDebtProvisionFor1 = GetTotalBadDebtProvision(materialCostSummary),
                TotalProducerFeeforLADisposalCostswithBadDebtprovision =
                    GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary),
                EnglandTotalwithBadDebtprovision = GetEnglandTotal(materialCostSummary),
                WalesTotalwithBadDebtprovision = GetWalesTotal(materialCostSummary),
                ScotlandTotalwithBadDebtprovision = GetScotlandTotal(materialCostSummary),
                NorthernIrelandTotalwithBadDebtprovision = GetNorthernIrelandTotal(materialCostSummary),

                TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision = GetTotalProducerCommsFee(commsCostSummary),
                BadDebtProvisionFor2A = GetTotalBadDebtProvision(materialCostSummary),
                TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision =
                    GetTotalProducerCommsFeeWithBadDebtProvision(commsCostSummary),
                EnglandTotalwithBadDebtprovision2A = GetEnglandCommsTotal(commsCostSummary),
                WalesTotalwithBadDebtprovision2A = GetWalesCommsTotal(commsCostSummary),
                ScotlandTotalwithBadDebtprovision2A = GetScotlandCommsTotal(commsCostSummary),
                NorthernIrelandTotalwithBadDebtprovision2A = GetNorthernIrelandCommsTotal(commsCostSummary),

                // LA data prep costs section 4
                LaDataPrepCostsTotalWithoutBadDebtProvisionSection4 =
                    GetLaDataPrepCostsTotalWithoutBadDebtProvisionSection4(),
                LaDataPrepCostsBadDebtProvisionSection4 = GetLaDataPrepCostsBadDebtProvisionSection4(),
                LaDataPrepCostsTotalWithBadDebtProvisionSection4 =
                    GetLaDataPrepCostsTotalWithBadDebtProvisionSection4(),
                LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4 =
                    GetLaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4(),
                LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4 =
                    GetLaDataPrepCostsWalesTotalWithBadDebtProvisionSection4(),
                LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4 =
                    GetLaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4(),
                LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4 =
                    GetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4(),

                // Percentage of Producer Reported Household Tonnage vs All Producers
                PercentageofProducerReportedHHTonnagevsAllProducers =
                    GetPercentageofProducerReportedHHTonnagevsAllProducers(producer, runProducerMaterialDetails),
            };
        }

        private static int GetLevelIndex(List<CalcResultSummaryProducerDisposalFees> producerDisposalFeesLookup, ProducerDetail producer)
        {
            var totalRow = producerDisposalFeesLookup.Find(pdf => pdf.ProducerId == producer.ProducerId.ToString() && pdf.isTotalRow);

            return totalRow == null ? 1 : 2;
        }

        private static decimal GetHouseholdPackagingWasteTonnage(ProducerDetail producer, MaterialDetail material)
        {
            var householdPackagingMaterial = producer.ProducerReportedMaterials.FirstOrDefault(p => p.Material.Code == material.Code && p.PackagingType == "HH");

            return householdPackagingMaterial != null ? householdPackagingMaterial.PackagingTonnage : 0;
        }

        private static decimal GetHouseholdPackagingWasteTonnageProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetHouseholdPackagingWasteTonnage(producer, material);
            }

            return totalCost;
        }

        private static decimal GetManagedConsumerWasteTonnage(ProducerDetail producer, MaterialDetail material)
        {
            var consumerWastePackagingMaterial = producer.ProducerReportedMaterials.FirstOrDefault(p => p.Material.Code == material.Code && p.PackagingType == "CW");

            return consumerWastePackagingMaterial != null ? consumerWastePackagingMaterial.PackagingTonnage : 0;
        }

        private static decimal GetManagedConsumerWasteTonnageProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetManagedConsumerWasteTonnage(producer, material);
            }

            return totalCost;
        }

        private static decimal GetNetReportedTonnage(ProducerDetail producer, MaterialDetail material)
        {
            var householdPackagingWasteTonnage = GetHouseholdPackagingWasteTonnage(producer, material);
            var managedConsumerWasteTonnage = GetManagedConsumerWasteTonnage(producer, material);

            if (householdPackagingWasteTonnage == 0 || managedConsumerWasteTonnage == 0)
            {
                return 0;
            }

            return householdPackagingWasteTonnage - managedConsumerWasteTonnage;
        }

        private static decimal GetNetReportedTonnageProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetNetReportedTonnage(producer, material);
            }

            return totalCost;
        }

        private static decimal GetPricePerTonne(MaterialDetail material, CalcResult calcResult)
        {
            var laDisposalCostDataDetail = calcResult.CalcResultLaDisposalCostData.CalcResultLaDisposalCostDetails.FirstOrDefault(la => la.Name == material.Name);

            if (laDisposalCostDataDetail == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(laDisposalCostDataDetail.DisposalCostPricePerTonne, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-GB"), out decimal value);

            return isParseSuccessful ? value : 0;
        }

        private static decimal GetProducerDisposalFee(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var netReportedTonnage = GetNetReportedTonnage(producer, material);
            var pricePerTonne = GetPricePerTonne(material, calcResult);

            if (netReportedTonnage == 0 || pricePerTonne == 0)
            {
                return 0;
            }

            return netReportedTonnage * pricePerTonne;
        }

        private static decimal GetProducerDisposalFeeProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetProducerDisposalFee(producer, material, calcResult);
            }

            return totalCost;
        }

        private static decimal GetBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFee = GetProducerDisposalFee(producer, material, calcResult);

            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                return (producerDisposalFee * value) / 100;
            }

            return 0;
        }

        private static decimal GetBadDebtProvisionProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetBadDebtProvision(producer, material, calcResult);
            }

            return totalCost;
        }

        private static decimal GetProducerDisposalFeeWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFee = GetProducerDisposalFee(producer, material, calcResult);

            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                return producerDisposalFee * (1 + (value / 100));
            }

            return 0;
        }

        private static decimal GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);
            }

            return totalCost;
        }

        private static decimal GetEnglandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

            var countryApportionmentPercentage = GetCountryApportionmentPercentage(calcResult);
            if (countryApportionmentPercentage == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(countryApportionmentPercentage.EnglandDisposalCost.Replace("%", string.Empty), out decimal value);

            return isParseSuccessful ? (producerDisposalFeeWithBadDebtProvision * value) / 100 : 0;
        }

        private static decimal GetEnglandWithBadDebtProvisionProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetEnglandWithBadDebtProvision(producer, material, calcResult);
            }

            return totalCost;
        }

        private static decimal GetWalesWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

            var countryApportionmentPercentage = GetCountryApportionmentPercentage(calcResult);
            if (countryApportionmentPercentage == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(countryApportionmentPercentage.WalesDisposalCost.Replace("%", string.Empty), out decimal value);

            return isParseSuccessful ? (producerDisposalFeeWithBadDebtProvision * value) / 100 : 0;
        }

        private static decimal GetWalesWithBadDebtProvisionProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetWalesWithBadDebtProvision(producer, material, calcResult);
            }

            return totalCost;
        }

        private static decimal GetScotlandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

            var countryApportionmentPercentage = GetCountryApportionmentPercentage(calcResult);
            if (countryApportionmentPercentage == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(countryApportionmentPercentage.ScotlandDisposalCost.Replace("%", string.Empty), out decimal value);

            return isParseSuccessful ? (producerDisposalFeeWithBadDebtProvision * value) / 100 : 0;
        }

        private static decimal GetScotlandWithBadDebtProvisionProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetScotlandWithBadDebtProvision(producer, material, calcResult);
            }

            return totalCost;
        }

        private static decimal GetNorthernIrelandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

            var countryApportionmentPercentage = GetCountryApportionmentPercentage(calcResult);
            if (countryApportionmentPercentage == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(countryApportionmentPercentage.NorthernIrelandDisposalCost.Replace("%", string.Empty), out decimal value);

            return isParseSuccessful ? (producerDisposalFeeWithBadDebtProvision * value) / 100 : 0;
        }

        private static decimal GetNorthernIrelandWithBadDebtProvisionProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetNorthernIrelandWithBadDebtProvision(producer, material, calcResult);
            }

            return totalCost;
        }

        private static CalcResultLapcapDataDetails? GetCountryApportionmentPercentage(CalcResult calcResult)
        {
            return calcResult.CalcResultLapcapData.CalcResultLapcapDataDetails?.FirstOrDefault(la => la.Name == CalcResultSummaryHeaders.OneCountryApportionment);
        }

        private static decimal GetTotalProducerDisposalFee(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            decimal totalProducerDisposalFee = 0;

            foreach (var material in materialCostSummary)
            {
                totalProducerDisposalFee += material.Value.ProducerDisposalFee;
            }

            return totalProducerDisposalFee;
        }

        private static decimal GetTotalBadDebtProvision(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            decimal totalBadDebtProvision = 0;

            foreach (var material in materialCostSummary)
            {
                totalBadDebtProvision += material.Value.BadDebtProvision;
            }

            return totalBadDebtProvision;
        }

        private static decimal GetTotalProducerDisposalFeeWithBadDebtProvision(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            decimal totalProducerDisposalFeeWithBadDebtProvision = 0;

            foreach (var material in materialCostSummary)
            {
                totalProducerDisposalFeeWithBadDebtProvision += material.Value.ProducerDisposalFeeWithBadDebtProvision;
            }

            return totalProducerDisposalFeeWithBadDebtProvision;
        }

        private static decimal GetEnglandTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            decimal totalEngland = 0;

            foreach (var material in materialCostSummary)
            {
                totalEngland += material.Value.EnglandWithBadDebtProvision;
            }

            return totalEngland;
        }

        private static decimal GetWalesTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            decimal totalWales = 0;

            foreach (var material in materialCostSummary)
            {
                totalWales += material.Value.WalesWithBadDebtProvision;
            }

            return totalWales;
        }

        private static decimal GetScotlandTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            decimal totalScotland = 0;

            foreach (var material in materialCostSummary)
            {
                totalScotland += material.Value.ScotlandWithBadDebtProvision;
            }

            return totalScotland;
        }

        private static decimal GetNorthernIrelandTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            decimal totalNorthernIreland = 0;

            foreach (var material in materialCostSummary)
            {
                totalNorthernIreland += material.Value.NorthernIrelandWithBadDebtProvision;
            }

            return totalNorthernIreland;
        }

        private static decimal GetLaDataPrepCostsTitleSection4(CalcResult calcResult)
        {
            return calcResult.CalcResultParameterOtherCost.Details.ToList()[0].TotalValue;
        }

        private static decimal GetLaDataPrepCostsBadDebtProvisionTitleSection4(CalcResult calcResult)
        {
            var isConversionSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            return isConversionSuccessful ? GetLaDataPrepCostsTitleSection4(calcResult) * value : 0;
        }

        private static decimal GetLaDataPrepCostsWithoutBadDebtProvisionTitleSection4(CalcResult calcResult)
        {
            return GetLaDataPrepCostsTitleSection4(calcResult) + GetLaDataPrepCostsBadDebtProvisionTitleSection4(calcResult);
        }

        private static decimal GetLaDataPrepCostsWithBadDebtProvisionTitleSection4(CalcResult calcResult)
        {
            return GetLaDataPrepCostsTitleSection4(calcResult) + GetLaDataPrepCostsBadDebtProvisionTitleSection4(calcResult);
        }

        private static decimal GetLaDataPrepCostsTotalWithoutBadDebtProvisionSection4()
        {
            return 99;
        }

        private static decimal GetLaDataPrepCostsBadDebtProvisionSection4()
        {
            return 99;
        }

        private static decimal GetLaDataPrepCostsTotalWithBadDebtProvisionSection4()
        {
            return 99;
        }

        private static decimal GetLaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4()
        {
            return 99;
        }

        private static decimal GetLaDataPrepCostsWalesTotalWithBadDebtProvisionSection4()
        {
            return 99;
        }

        private static decimal GetLaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4()
        {
            return 99;
        }

        private static decimal GetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4()
        {
            return 99;
        }

        private static void SetHeaders(CalcResultSummary result, List<MaterialDetail> materials)
        {
            result.ResultSummaryHeader = new CalcResultSummaryHeader
            {
                Name = CalcResultSummaryHeaders.CalculationResult,
                ColumnIndex = ResultSummaryHeaderColumnIndex
            };

            result.ProducerDisposalFeesHeaders = GetProducerDisposalFeesHeaders();

            result.MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(result, materials);

            result.ColumnHeaders = GetColumnHeaders(materials);
        }

        private static List<CalcResultSummaryHeader> GetProducerDisposalFeesHeaders()
        {
            return [
                //Section-1 Title headers
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.OneProducerDisposalFeesWithBadDebtProvision, ColumnIndex = ProducerDisposalFeesHeaderColumnIndex },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostHeader, ColumnIndex = CommsCostHeaderColumnIndex },
                
                //Section-(1) & (2a) Title headers   
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforLADisposalCostswoBadDebtprovision1, ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision},
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforLADisposalCostswithBadDebtprovision1 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforCommsCostsbyMaterialwoBadDebtprovision2A, ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex + 5 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision},
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforCommsCostsbyMaterialwithBadDebtprovision2A },

                //Section-4 Title headers
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.LaDataPrepCostsWithoutBadDebtProvisionTitleSection4, ColumnIndex = LaDataPrepCostsSection4ColumnIndex },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvisionTitleSection4 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.LaDataPrepCostsWithBadDebtProvisionTitleSection4 },
            ];
        }

        private static List<CalcResultSummaryHeader> GetMaterialsBreakdownHeader(CalcResultSummary result, List<MaterialDetail> materials)
        {
            var materialsBreakdownHeaders = new List<CalcResultSummaryHeader>();
            var columnIndex = MaterialsBreakdownHeaderInitialColumnIndex;

            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultSummaryHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex
                });
                columnIndex = columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex;
            }

            // Add disposal fee summary header
            materialsBreakdownHeaders.Add(new CalcResultSummaryHeader
            {
                Name = CalcResultSummaryHeaders.DisposalFeeSummary,
                ColumnIndex = DisposalFeeSummaryColumnIndex
            });

            var commsCostColumnIndex = MaterialsBreakdownHeaderCommsInitialColumnIndex;

            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultSummaryHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = commsCostColumnIndex
                });
                commsCostColumnIndex = commsCostColumnIndex + MaterialsBreakdownHeaderCommsIncrementalColumnIndex;
            }

            materialsBreakdownHeaders.Add(new CalcResultSummaryHeader
            {
                Name = CalcResultSummaryHeaders.CommsCostSummaryHeader,
                ColumnIndex = commsCostColumnIndex
            });

            //Section-(1) & (2a)
            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalFeeforLADisposalCostswoBadDebtprovision1, decimalRoundUp)}", ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.BadDebtProvisionFor1, decimalRoundUp)}" },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalFeeforLADisposalCostswithBadDebtprovision1, decimalRoundUp)}" }
            ]);

            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A, decimalRoundUp)}", ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex + 5 },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.BadDebtProvisionFor2A, decimalRoundUp)}" },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A, decimalRoundUp)}" }
            ]);

            // LA data prep costs section 4
            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"{result.LaDataPrepCostsTitleSection4}", ColumnIndex = LaDataPrepCostsSection4ColumnIndex },
                new CalcResultSummaryHeader { Name = $"{result.LaDataPrepCostsBadDebtProvisionTitleSection4}" },
                new CalcResultSummaryHeader { Name = $"{result.LaDataPrepCostsWithBadDebtProvisionTitleSection4}" }
            ]);

            return materialsBreakdownHeaders;
        }

        private static List<CalcResultSummaryHeader> GetColumnHeaders(List<MaterialDetail> materials)
        {
            var columnHeaders = new List<CalcResultSummaryHeader>();

            columnHeaders.AddRange([
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerId },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.SubsidiaryId },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerOrSubsidiaryName },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.Level }
            ]);

            foreach (var material in materials)
            {
                columnHeaders.AddRange([
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ReportedHouseholdPackagingWasteTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ReportedSelfManagedConsumerWasteTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NetReportedTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PricePerTonne },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerDisposalFee },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerDisposalFeeWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandWithBadDebtProvision }
                ]);
            }

            columnHeaders.AddRange([
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerDisposalFee },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerDisposalFeeWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotal },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotal },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotal },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotal }
            ]);

            foreach (var material in materials)
            {
                columnHeaders.AddRange([
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ReportedHouseholdPackagingWasteTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PricePerTonne },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerTotalCostWithoutBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerTotalCostwithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandWithBadDebtProvision }
                ]);
            }

            columnHeaders.AddRange([
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalBadDebtProvision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision }
            ]);

            //Section-(1) & (2a)
            columnHeaders.AddRange([
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforLADisposalCostswoBadDebtprovision, ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvisionFor1 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforLADisposalCostswithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision }
            ]);
            columnHeaders.AddRange([
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision2A, ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvisionfor2A },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision2A },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision }
            ]);

            // skippping columns for other sections
            columnHeaders.AddRange([
                new CalcResultSummaryHeader { Name = "" }, 
                new CalcResultSummaryHeader { Name = "" }
            ]);

            // Percentage of Producer Reported Household Tonnage vs All Producers
            columnHeaders.AddRange([
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PercentageofProducerReportedHHTonnagevsAllProducers, ColumnIndex = PercentageofProducerReportedHHTonnageColumnIndex },
            ]);

            // skippping columns for other sections
            for (int i = PercentageofProducerReportedHHTonnageColumnIndex; i <= LaDataPrepCostsSection4ColumnIndex; i++)
            {
                columnHeaders.AddRange([new CalcResultSummaryHeader { Name = "", ColumnIndex = i }]);
            }

            // LA data prep costs section 4 column headers
            columnHeaders.AddRange([
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeWithoutBadDebtProvisionSection4, ColumnIndex = LaDataPrepCostsSection4ColumnIndex },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvisionSection4 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeWithBadDebtProvisionSection4 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotalWithBadDebtProvisionSection4 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotalWithBadDebtProvisionSection4 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotalWithBadDebtProvisionSection4 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotalWithBadDebtProvisionSection4 }
            ]);

            return columnHeaders;
        }

        private static decimal GetProducerTotalCostWithoutBadDebtProvisionTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetProducerTotalCostWithoutBadDebtProvision(producer, material, calcResult);
            }

            return totalCost;
        }

        private static decimal GetBadDebtProvisionForCommsCostTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetBadDebtProvisionForCommsCost(producer, material, calcResult);
            }

            return totalCost;
        }

        private static decimal GetProducerTotalCostwithBadDebtProvisionTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
            }

            return totalCost;
        }

        private static decimal GetEnglandWithBadDebtProvisionForCommsTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetEnglandWithBadDebtProvisionForComms(producer, material, calcResult);
            }

            return totalCost;
        }

        private static decimal GetWalesWithBadDebtProvisionForCommsTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetWalesWithBadDebtProvisionForComms(producer, material, calcResult);
            }

            return totalCost;
        }

        private static decimal GetScotlandWithBadDebtProvisionForCommsTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetScotlandWithBadDebtProvisionForComms(producer, material, calcResult);
            }

            return totalCost;
        }

        private static decimal GetNorthernIrelandWithBadDebtProvisionForCommsTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
        {
            decimal totalCost = 0;

            foreach (var producer in producers)
            {
                totalCost += GetNorthernIrelandWithBadDebtProvisionForComms(producer, material, calcResult);
            }

            return totalCost;
        }

        private static decimal GetTotalProducerCommsFee(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal producerTotalCostWithoutBadDebtProvision = 0;

            foreach (var material in commsCostSummary)
            {
                producerTotalCostWithoutBadDebtProvision += material.Value.ProducerTotalCostWithoutBadDebtProvision;
            }

            return producerTotalCostWithoutBadDebtProvision;
        }

        private static decimal GetCommsTotalBadDebtProvision(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCostSummary)
        {
            decimal totalBadDebtProvision = 0;

            foreach (var material in materialCostSummary)
            {
                totalBadDebtProvision += material.Value.BadDebtProvision;
            }

            return totalBadDebtProvision;
        }

        private static decimal GetTotalProducerCommsFeeWithBadDebtProvision(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal totalCommsCostsbyMaterialwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                totalCommsCostsbyMaterialwithBadDebtprovision += material.Value.ProducerTotalCostwithBadDebtProvision;
            }

            return totalCommsCostsbyMaterialwithBadDebtprovision;
        }

        private static decimal GetNorthernIrelandCommsTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal northernIrelandTotalwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                northernIrelandTotalwithBadDebtprovision += material.Value.NorthernIrelandWithBadDebtProvision;
            }

            return northernIrelandTotalwithBadDebtprovision;
        }

        private static decimal GetScotlandCommsTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal scotlandTotalwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                scotlandTotalwithBadDebtprovision += material.Value.ScotlandWithBadDebtProvision;
            }

            return scotlandTotalwithBadDebtprovision;
        }

        private static decimal GetWalesCommsTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal walesTotalwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                walesTotalwithBadDebtprovision += material.Value.WalesWithBadDebtProvision;
            }

            return walesTotalwithBadDebtprovision;
        }

        private static decimal GetEnglandCommsTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal englandTotalwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                englandTotalwithBadDebtprovision += material.Value.EnglandWithBadDebtProvision;
            }

            return englandTotalwithBadDebtprovision;
        }

        private static decimal GetProducerTotalCostWithoutBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var hhPackagingWasteTonnage = GetHouseholdPackagingWasteTonnage(producer, material);
            var priceperTonne = GetPriceperTonneForComms(material, calcResult);

            return hhPackagingWasteTonnage * priceperTonne;
        }

        private static decimal GetBadDebtProvisionForCommsCost(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var badDebtProvision = Convert.ToDecimal(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Trim('%'));
            var producerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(producer, material, calcResult);
            return (producerTotalCostWithoutBadDebtProvision * badDebtProvision) / 100;
        }

        private static decimal GetProducerTotalCostwithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var badDebtProvision = Convert.ToDecimal(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Trim('%'));
            var producerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(producer, material, calcResult);
            return producerTotalCostWithoutBadDebtProvision * (1 + badDebtProvision / 100);
        }

        private static decimal GetEnglandWithBadDebtProvisionForComms(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * (Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.EnglandDisposalTotal).ToList()[4].Trim('%')) / 100);
        }

        private static decimal GetWalesWithBadDebtProvisionForComms(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * (Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.WalesDisposalTotal).ToList()[4].Trim('%')) / 100);
        }

        private static decimal GetScotlandWithBadDebtProvisionForComms(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * (Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.ScotlandDisposalTotal).ToList()[4].Trim('%')) / 100);
        }

        private static decimal GetNorthernIrelandWithBadDebtProvisionForComms(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * (Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.NorthernIrelandDisposalTotal).ToList()[4].Trim('%')) / 100);
        }

        private static decimal GetPriceperTonneForComms(MaterialDetail material, CalcResult calcResult)
        {
            var commsCostDataDetail = calcResult.CalcResultCommsCostReportDetail.CalcResultCommsCostCommsCostByMaterial.FirstOrDefault(la => la.Name == material.Name);

            if (commsCostDataDetail == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(commsCostDataDetail.CommsCostByMaterialPricePerTonne, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out decimal value);

            return isParseSuccessful ? value : 0;
        }

        private static decimal GetTotalDisposalCostswoBadDebtprovision1(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.TotalProducerDisposalFee);
        }

        private static decimal GetTotalBadDebtprovision1(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.BadDebtProvisionFor1);
        }

        private static decimal GetTotalDisposalCostswithBadDebtprovision1(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.TotalProducerDisposalFeeWithBadDebtProvision);
        }

        private static decimal GetTotalCommsCostswoBadDebtprovision2A(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.TotalProducerCommsFee);
        }

        private static decimal GetTotalBadDebtprovision2A(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.BadDebtProvisionFor2A);
        }

        private static decimal GetTotalCommsCostswithBadDebtprovision2A(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.TotalProducerCommsFeeWithBadDebtProvision);
        }

        public static decimal GetTotalFee(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees, Func<CalcResultSummaryProducerDisposalFees, decimal?> selector)
        {
            if (producerDisposalFees == null)
            {
                return 0m;
            }

            var totalFee = producerDisposalFees
                .FirstOrDefault(t => t.Level == "Totals");

            return selector(totalFee) ?? 0m;
        }

        private decimal GetPercentageofProducerReportedHHTonnagevsAllProducersTotal(List<ProducerDetail> producers, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal totalPercentageofProducerReportedHH = 0;

            foreach (var producer in producers)
            {
                totalPercentageofProducerReportedHH += GetPercentageofProducerReportedHHTonnagevsAllProducers(producer, allResults);
            }

            return totalPercentageofProducerReportedHH;
        }

        private decimal GetPercentageofProducerReportedHHTonnagevsAllProducers(ProducerDetail producer,
            IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            var allProducerDetails = allResults.Select(x => x.ProducerDetail);
            var allProducerReportedMaterials = allResults.Select(x => x.ProducerReportedMaterial);

            var result =
                (from p in allProducerDetails
                 join m in allProducerReportedMaterials
                        on p.Id equals m.ProducerDetailId
                    where p.CalculatorRunId == producer.CalculatorRunId && m.PackagingType == "HH"
                    group m by p.ProducerId
                    into g
                    select new
                    {
                        ProducerId = g.Key,
                        TotalPackagingTonnage = g.Sum(x => x.PackagingTonnage)
                    }).ToList();


            var totalTonnage = result.Sum(x => x.TotalPackagingTonnage);
            var producerData = result.FirstOrDefault(x => x.ProducerId == producer.ProducerId);
            var PercentageofHHTonnage = producerData != null && totalTonnage > 0
                ? (producerData.TotalPackagingTonnage / totalTonnage) * 100
                : 0;

            return PercentageofHHTonnage;
        }
    }
}
