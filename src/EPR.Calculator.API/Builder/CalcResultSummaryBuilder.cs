﻿using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using System.Globalization;

namespace EPR.Calculator.API.Builder
{
    public class CalcResultSummaryBuilder : ICalcResultSummaryBuilder
    {
        private readonly ApplicationDBContext context;

        public CalcResultSummaryBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public CalcResultSummary Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult)
        {
            var result = new CalcResultSummary();

            result.ResultSummaryHeader = new CalcResultSummaryHeader
            {
                Name = "Calculation Result",
                ColumnIndex = 0
            };

            result.ProducerDisposalFeesHeader = new CalcResultSummaryHeader
            {
                Name = "1 Producer Disposal Fees with Bad Debt Provision",
                ColumnIndex = 4
            };

            var materialsFromDb = this.context.Material.ToList();

            var materials = Mappers.MaterialMapper.Map(materialsFromDb);

            var materialsBreakdownHeader = new List<CalcResultSummaryHeader>();
            var columnIndex = 4;

            foreach (var material in materials)
            {
                materialsBreakdownHeader.Add(new CalcResultSummaryHeader {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex
                });
                columnIndex = columnIndex + 11;
            }

            result.MaterialBreakdownHeaders = materialsBreakdownHeader;

            var columnHeaders = new List<string>();

            columnHeaders.AddRange([
                CalcResultSummaryHeaders.ProducerId,
                CalcResultSummaryHeaders.SubsidiaryId,
                CalcResultSummaryHeaders.ProducerOrSubsidiaryName,
                CalcResultSummaryHeaders.Level
            ]);

            foreach (var material in materials)
            {
                columnHeaders.AddRange([
                    CalcResultSummaryHeaders.ReportedHouseholdPackagingWasteTonnage,
                    CalcResultSummaryHeaders.ReportedSelfManagedConsumerWasteTonnage,
                    CalcResultSummaryHeaders.NetReportedTonnage,
                    CalcResultSummaryHeaders.PricePerTonne,
                    CalcResultSummaryHeaders.ProducerDisposalFee,
                    CalcResultSummaryHeaders.BadDebtProvision,
                    CalcResultSummaryHeaders.ProducerDisposalFeeWithBadDebtProvision,
                    CalcResultSummaryHeaders.EnglandWithBadDebtProvision,
                    CalcResultSummaryHeaders.WalesWithBadDebtProvision,
                    CalcResultSummaryHeaders.ScotlandWithBadDebtProvision,
                    CalcResultSummaryHeaders.NorthernIrelandWithBadDebtProvision
                ]);
            }

            result.ColumnHeaders = columnHeaders;

            var producerDetailList = this.context.ProducerDetail.ToList();

            var producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>();

            foreach (var producer in producerDetailList)
            {
                var costSummary = new List<CalcResultSummaryProducerDisposalFeesByMaterial>();

                var materialCostSummary = new Dictionary<MaterialDetail, IEnumerable<CalcResultSummaryProducerDisposalFeesByMaterial>>();

                foreach (var material in materials)
                {
                    costSummary.Add(new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = GetHouseholdPackagingWasteTonnage(producer, material),
                        ManagedConsumerWasteTonnage = GetManagedConsumerWasteTonnage(producer, material),
                        NetReportedTonnage = GetNetReportedTonnage(producer, material),
                        PricePerTonnage = GetPricePerTonne(material, calcResult),
                        ProducerDisposalFee = GetProducerDisposalFee(producer, material, calcResult),
                        BadDebtProvision = GetBadDebtProvision(producer, material, calcResult),
                        ProducerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult),
                        EnglandWithBadDebtProvision = GetEnglandWithBadDebtProvision(producer, material, calcResult),
                        WalesWithBadDebtProvision = GetWalesWithBadDebtProvision(producer, material, calcResult),
                        ScotlandWithBadDebtProvision = GetScotlandWithBadDebtProvision(producer, material, calcResult),
                        NorthernIrelandWithBadDebtProvision = GetNorthernIrelandWithBadDebtProvision(producer, material, calcResult)
                    });

                    materialCostSummary.Add(material, costSummary);
                }

                producerDisposalFees.Add(new CalcResultSummaryProducerDisposalFees
                {
                    ProducerId = producer.Id.ToString(),
                    ProducerName = producer.ProducerName ?? string.Empty,
                    SubsidiaryId = producer.SubsidiaryId ?? string.Empty,
                    Level = 1,
                    Order = 2,
                    ProducerDisposalFeesByMaterial = materialCostSummary
                });
            }

            result.ProducerDisposalFees = producerDisposalFees;

            return result;
        }

        private decimal GetHouseholdPackagingWasteTonnage(ProducerDetail producer, MaterialDetail material)
        {
            var householdPackagingMaterial = producer.ProducerReportedMaterials.FirstOrDefault(p => p.Material.Code == material.Code && p.PackagingType == "HH");

            return householdPackagingMaterial != null ? householdPackagingMaterial.PackagingTonnage : 0;
        }

        private decimal GetManagedConsumerWasteTonnage(ProducerDetail producer, MaterialDetail material)
        {
            var consumerWastePackagingMaterial = producer.ProducerReportedMaterials.FirstOrDefault(p => p.Material.Code == material.Code && p.PackagingType == "CW");

            return consumerWastePackagingMaterial != null ? consumerWastePackagingMaterial.PackagingTonnage : 0;
        }

        private decimal GetNetReportedTonnage(ProducerDetail producer, MaterialDetail material)
        {
            var householdPackagingWasteTonnage = GetHouseholdPackagingWasteTonnage(producer, material);
            var managedConsumerWasteTonnage = GetManagedConsumerWasteTonnage(producer, material);

            if (householdPackagingWasteTonnage == 0 || managedConsumerWasteTonnage == 0)
            {
                return 0;
            }

            return householdPackagingWasteTonnage - managedConsumerWasteTonnage;
        }

        private static decimal GetPricePerTonne(MaterialDetail material, CalcResult calcResult)
        {
            var laDisposalCostDataDetail = calcResult.CalcResultLaDisposalCostData.CalcResultLaDisposalCostDetails.FirstOrDefault(la => la.Material == material.Description);

            if (laDisposalCostDataDetail == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(laDisposalCostDataDetail.DisposalCostPricePerTonne, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out decimal value);

            return isParseSuccessful ? value : 0;
        }

        private decimal GetProducerDisposalFee(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var netReportedTonnage = GetNetReportedTonnage(producer, material);
            var pricePerTonne = GetPricePerTonne(material, calcResult);

            if (netReportedTonnage == 0 || pricePerTonne == 0)
            {
                return 0;
            }

            return netReportedTonnage * pricePerTonne;
        }

        private decimal GetBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFee = GetProducerDisposalFee(producer, material, calcResult);


            return producerDisposalFee * 6;
        }

        private decimal GetProducerDisposalFeeWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFee = GetProducerDisposalFee(producer, material, calcResult);

            return producerDisposalFee * (1 + 6);
        }

        private decimal GetEnglandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

            var countryApportionmentPercentage = calcResult.CalcResultLapcapData.CalcResultLapcapDataDetails?.FirstOrDefault(la => la.Name == "1 Country Apportionment");

            if (countryApportionmentPercentage == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(countryApportionmentPercentage.EnglandDisposalCost, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out decimal value);

            return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value : 0;
        }

        private decimal GetWalesWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

            var countryApportionmentPercentage = calcResult.CalcResultLapcapData.CalcResultLapcapDataDetails?.FirstOrDefault(la => la.Name == "1 Country Apportionment");

            if (countryApportionmentPercentage == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(countryApportionmentPercentage.WalesDisposalCost, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out decimal value);

            return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value : 0;
        }

        private decimal GetScotlandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

            var countryApportionmentPercentage = calcResult.CalcResultLapcapData.CalcResultLapcapDataDetails?.FirstOrDefault(la => la.Name == "1 Country Apportionment");

            if (countryApportionmentPercentage == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(countryApportionmentPercentage.ScotlandDisposalCost, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out decimal value);

            return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value : 0;
        }

        private decimal GetNorthernIrelandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

            var countryApportionmentPercentage = calcResult.CalcResultLapcapData.CalcResultLapcapDataDetails?.FirstOrDefault(la => la.Name == "1 Country Apportionment");

            if (countryApportionmentPercentage == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(countryApportionmentPercentage.NorthernIrelandDisposalCost, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out decimal value);

            return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value : 0;
        }
    }
}
