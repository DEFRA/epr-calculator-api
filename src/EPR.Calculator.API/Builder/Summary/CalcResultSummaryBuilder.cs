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

        private const int ResultSummaryHeaderColumnIndex = 0;
        private const int ProducerDisposalFeesHeaderColumnIndex = 4;
        private const int CommsCostHeaderColumnIndex = 98;
        private const int MaterialsBreakdownHeaderInitialColumnIndex = 4;
        private const int MaterialsBreakdownHeaderIncrementalColumnIndex = 10;
        private const int LaDataPrepCostsSection4ColumnIndex = 209;
        private const int ProducerCommsFeesHeaderColumnIndex = 99;
        private const int MaterialsBreakdownHeaderCommsIncrementalColumnIndex = 8;

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

            // Set headers with calculated column index
            SetHeaders(result, materials);

            // Get the ordered list of producers associated with the calculator run id
            var producerDetailList = context.ProducerDetail
                .Where(pd => pd.CalculatorRunId == resultsRequestDto.RunId)
                .OrderBy(pd => pd.ProducerId)
                .ToList();

            if (producerDetailList.Count > 0)
            {
                result.LaDataPrepCostsTitleSection4 = GetLaDataPrepCostsTitleSection4(calcResult);
                result.LaDataPrepCostsBadDebtProvisionTitleSection4 = GetLaDataPrepCostsBadDebtProvisionTitleSection4(calcResult);
                result.LaDataPrepCostsWithBadDebtProvisionTitleSection4 = GetLaDataPrepCostsWithBadDebtProvisionTitleSection4(calcResult);

                var producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>();

                foreach (var producer in producerDetailList)
                {
                    // We have to write an additional row if a producer have at least one subsidiary
                    // This additional row will be the total of this producer and its subsidiaries
                    var producersAndSubsidiaries = producerDetailList.Where(pd => pd.ProducerId == producer.ProducerId);
                    // Make sure the total row is written only once
                    if (producersAndSubsidiaries.Count() > 1 && producerDisposalFees.Find(pdf => pdf.ProducerId == producer.ProducerId.ToString()) == null)
                    {
                        var totalRow = GetProducerTotalRow(producersAndSubsidiaries.ToList(), materials, calcResult);
                        producerDisposalFees.Add(totalRow);
                    }

                    // Calculate the values for the producer
                    producerDisposalFees.Add(GetProducerRow(producerDisposalFees, producer, materials, calcResult));
                }

                // Calculate the total for all the producers
                producerDisposalFees.Add(GetProducerTotalRow(producerDetailList.ToList(), materials, calcResult, true));

                result.ProducerDisposalFees = producerDisposalFees;
            }

            return result;
        }

        private CalcResultSummaryProducerDisposalFees GetProducerTotalRow(
            List<ProducerDetail> producersAndSubsidiaries, List<MaterialDetail> materials, CalcResult calcResult, bool isOverAllTotalRow = false)
        {
            var materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();
            var commsCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>();

            foreach (var material in materials)
            {
                materialCostSummary.Add(material, new CalcResultSummaryProducerDisposalFeesByMaterial
                {
                    HouseholdPackagingWasteTonnage = GetHouseholdPackagingWasteTonnageProducerTotal(producersAndSubsidiaries, material),
                    ManagedConsumerWasteTonnage = GetManagedConsumerWasteTonnageProducerTotal(producersAndSubsidiaries, material),
                    NetReportedTonnage = GetNetReportedTonnageProducerTotal(producersAndSubsidiaries, material),
                    PricePerTonne = GetPricePerTonne(material, calcResult),
                    ProducerDisposalFee = GetProducerDisposalFeeProducerTotal(producersAndSubsidiaries, material, calcResult),
                    BadDebtProvision = GetBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult),
                    ProducerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult),
                    EnglandWithBadDebtProvision = GetEnglandWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult),
                    WalesWithBadDebtProvision = GetWalesWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult),
                    ScotlandWithBadDebtProvision = GetScotlandWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult),
                    NorthernIrelandWithBadDebtProvision = GetNorthernIrelandWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult)
                });

                commsCostSummary.Add(material, new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = GetHouseholdPackagingWasteTonnageProducerTotal(producersAndSubsidiaries, material),
                    PriceperTonne = GetPriceperTonneForComms(material, calcResult),
                    ProducerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvisionTotal(producersAndSubsidiaries, material, calcResult),
                    BadDebtProvision = GetBadDebtProvisionForCommsCostTotal(producersAndSubsidiaries, material, calcResult),
                    ProducerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvisionTotal(producersAndSubsidiaries, material, calcResult),
                    EnglandWithBadDebtProvision = GetEnglandWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material, calcResult),
                    WalesWithBadDebtProvision = GetWalesWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material, calcResult),
                    ScotlandWithBadDebtProvision = GetScotlandWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material, calcResult),
                    NorthernIrelandWithBadDebtProvision = GetNorthernIrelandWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material, calcResult)
                });

            }

            return new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = isOverAllTotalRow ? string.Empty : producersAndSubsidiaries[0].ProducerId.ToString(),
                ProducerName = isOverAllTotalRow ? string.Empty : producersAndSubsidiaries[0].ProducerName ?? string.Empty,
                SubsidiaryId = string.Empty,
                Level = isOverAllTotalRow ? "Totals" : "1",
                ProducerDisposalFeesByMaterial = materialCostSummary,

                // Disposal fee summary
                TotalProducerDisposalFee = GetTotalProducerDisposalFee(materialCostSummary),
                BadDebtProvision = GetTotalBadDebtProvision(materialCostSummary),
                TotalProducerDisposalFeeWithBadDebtProvision = GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary),
                EnglandTotal = GetEnglandTotal(materialCostSummary),
                WalesTotal = GetWalesTotal(materialCostSummary),
                ScotlandTotal = GetScotlandTotal(materialCostSummary),
                NorthernIrelandTotal = GetNorthernIrelandTotal(materialCostSummary),

                //For Comms Start
                TotalProducerCommsFee = GetTotalProducerCommsFee(commsCostSummary),
                BadDebtProvisionComms = GetCommsTotalBadDebtProvision(commsCostSummary),
                TotalProducerCommsFeeWithBadDebtProvision = GetTotalProducerCommsFeeWithBadDebtProvision(commsCostSummary),
                EnglandTotalComms = GetEnglandCommsTotal(commsCostSummary),
                WalesTotalComms = GetWalesCommsTotal(commsCostSummary),
                ScotlandTotalComms = GetScotlandCommsTotal(commsCostSummary),
                NorthernIrelandTotalComms = GetNorthernIrelandCommsTotal(commsCostSummary),
                ProducerCommsFeesByMaterial = commsCostSummary,

                // LA data prep costs section 4
                LaDataPrepCostsTotalWithoutBadDebtProvisionSection4 = GetLaDataPrepCostsTotalWithoutBadDebtProvisionSection4(),
                LaDataPrepCostsBadDebtProvisionSection4 = GetLaDataPrepCostsBadDebtProvisionSection4(),
                LaDataPrepCostsTotalWithBadDebtProvisionSection4 = GetLaDataPrepCostsTotalWithBadDebtProvisionSection4(),
                LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4 = GetLaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4(),
                LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4 = GetLaDataPrepCostsWalesTotalWithBadDebtProvisionSection4(),
                LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4 = GetLaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4(),
                LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4 = GetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4(),

                //For Comms End
                isTotalRow = true
            };

        }

        private CalcResultSummaryProducerDisposalFees GetProducerRow(
            List<CalcResultSummaryProducerDisposalFees> producerDisposalFeesLookup, ProducerDetail producer, List<MaterialDetail> materials, CalcResult calcResult)
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
                    ProducerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult),
                    EnglandWithBadDebtProvision = GetEnglandWithBadDebtProvision(producer, material, calcResult),
                    WalesWithBadDebtProvision = GetWalesWithBadDebtProvision(producer, material, calcResult),
                    ScotlandWithBadDebtProvision = GetScotlandWithBadDebtProvision(producer, material, calcResult),
                    NorthernIrelandWithBadDebtProvision = GetNorthernIrelandWithBadDebtProvision(producer, material, calcResult)
                });

                commsCostSummary.Add(material, new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = GetHouseholdPackagingWasteTonnage(producer, material),
                    PriceperTonne = GetPriceperTonneForComms(material, calcResult),
                    ProducerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(producer, material, calcResult),
                    BadDebtProvision = GetBadDebtProvisionForCommsCost(producer, material, calcResult),
                    ProducerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult),
                    EnglandWithBadDebtProvision = GetEnglandWithBadDebtProvisionForComms(producer, material, calcResult),
                    WalesWithBadDebtProvision = GetWalesWithBadDebtProvisionForComms(producer, material, calcResult),
                    ScotlandWithBadDebtProvision = GetScotlandWithBadDebtProvisionForComms(producer, material, calcResult),
                    NorthernIrelandWithBadDebtProvision = GetNorthernIrelandWithBadDebtProvisionForComms(producer, material, calcResult)
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
                TotalProducerDisposalFeeWithBadDebtProvision = GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary),
                EnglandTotal = GetEnglandTotal(materialCostSummary),
                WalesTotal = GetWalesTotal(materialCostSummary),
                ScotlandTotal = GetScotlandTotal(materialCostSummary),
                NorthernIrelandTotal = GetNorthernIrelandTotal(materialCostSummary),

                //For comms
                TotalProducerCommsFee = GetTotalProducerCommsFee(commsCostSummary),
                BadDebtProvisionComms = GetCommsTotalBadDebtProvision(commsCostSummary),
                TotalProducerCommsFeeWithBadDebtProvision = GetTotalProducerCommsFeeWithBadDebtProvision(commsCostSummary),
                EnglandTotalComms = GetEnglandCommsTotal(commsCostSummary),
                WalesTotalComms = GetWalesCommsTotal(commsCostSummary),
                ScotlandTotalComms = GetScotlandCommsTotal(commsCostSummary),
                NorthernIrelandTotalComms = GetNorthernIrelandCommsTotal(commsCostSummary),
                ProducerCommsFeesByMaterial = commsCostSummary,

                // LA data prep costs section 4
                LaDataPrepCostsTotalWithoutBadDebtProvisionSection4 = GetLaDataPrepCostsTotalWithoutBadDebtProvisionSection4(),
                LaDataPrepCostsBadDebtProvisionSection4 = GetLaDataPrepCostsBadDebtProvisionSection4(),
                LaDataPrepCostsTotalWithBadDebtProvisionSection4 = GetLaDataPrepCostsTotalWithBadDebtProvisionSection4(),
                LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4 = GetLaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4(),
                LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4 = GetLaDataPrepCostsWalesTotalWithBadDebtProvisionSection4(),
                LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4 = GetLaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4(),
                LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4 = GetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4(),
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

            var isParseSuccessful = decimal.TryParse(laDisposalCostDataDetail.DisposalCostPricePerTonne, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out decimal value);

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


            return producerDisposalFee * 6;
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

            return producerDisposalFee * (1 + 6);
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

            return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value : 0;
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

            return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value : 0;
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

            return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value : 0;
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

            return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value : 0;
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

            result.MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(materials);

            result.ColumnHeaders = GetColumnHeaders(materials);
        }

        private static List<CalcResultSummaryHeader> GetProducerDisposalFeesHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.OneProducerDisposalFeesWithBadDebtProvision, ColumnIndex = ProducerDisposalFeesHeaderColumnIndex },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostHeader, ColumnIndex = CommsCostHeaderColumnIndex },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.LaDataPrepCostsWithoutBadDebtProvisionTitleSection4, ColumnIndex = LaDataPrepCostsSection4ColumnIndex },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvisionTitleSection4 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.LaDataPrepCostsWithBadDebtProvisionTitleSection4 },
            ];
        }

        private static List<CalcResultSummaryHeader> GetMaterialsBreakdownHeader(List<MaterialDetail> materials)
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
                ColumnIndex = columnIndex
            });

            var commsCostColumnIndex = columnIndex + 6;

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

            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = "£36,500.00", ColumnIndex = LaDataPrepCostsSection4ColumnIndex - 4 },
                new CalcResultSummaryHeader { Name = "£2,190.00" },
                new CalcResultSummaryHeader { Name = "£38,690.00" }
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

            var columnIndex = 4;

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

                columnIndex += 11;
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

            columnIndex += 7;

            // LA data prep costs section 4 column headers
            columnHeaders.AddRange([
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeWithoutBadDebtProvisionSection4, ColumnIndex = columnIndex + 1 },
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
                totalCommsCostsbyMaterialwithBadDebtprovision += material.Value.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision;
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
            var priceperTonne = GetPriceperTonneForComms(material,calcResult);

            return hhPackagingWasteTonnage * priceperTonne;
        }

        private static decimal GetBadDebtProvisionForCommsCost(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var badDebtProvision = Convert.ToDecimal(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Trim('%'));
            var producerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(producer, material, calcResult);
            return producerTotalCostWithoutBadDebtProvision * badDebtProvision;
        }

        private static decimal GetProducerTotalCostwithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var badDebtProvision = Convert.ToDecimal(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Trim('%'));
            var producerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(producer, material, calcResult);
            return producerTotalCostWithoutBadDebtProvision * (1 + badDebtProvision);
        }

        private static decimal GetEnglandWithBadDebtProvisionForComms(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * (1 + Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.EnglandDisposalTotal).ToList()[4].Trim('%')));
        }

        private static decimal GetWalesWithBadDebtProvisionForComms(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * (1 + Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.WalesDisposalTotal).ToList()[4].Trim('%')));
        }

        private static decimal GetScotlandWithBadDebtProvisionForComms(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * (1 + Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.ScotlandDisposalTotal).ToList()[4].Trim('%')));
        }

        private static decimal GetNorthernIrelandWithBadDebtProvisionForComms(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * (1 + Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.NorthernIrelandDisposalTotal).ToList()[4].Trim('%')));
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

    }
}