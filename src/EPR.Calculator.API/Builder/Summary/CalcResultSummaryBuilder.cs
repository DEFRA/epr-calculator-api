﻿using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Utils;
using System.Globalization;

namespace EPR.Calculator.API.Builder.Summary
{
    public class CalcResultSummaryBuilder : ICalcResultSummaryBuilder
    {
        private readonly ApplicationDBContext context;

        private const int ResultSummaryHeaderColumnIndex = 0;
        private const int ProducerDisposalFeesHeaderColumnIndex = 4;
        private const int MaterialsBreakdownHeaderInitialColumnIndex = 4;
        private const int MaterialsBreakdownHeaderIncrementalColumnIndex = 11;

        public CalcResultSummaryBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public CalcResultSummary Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult)
        {
            var result = new CalcResultSummary();

            result.ResultSummaryHeader = new CalcResultSummaryHeader
            {
                Name = CalcResultSummaryHeaders.CalculationResult,
                ColumnIndex = ResultSummaryHeaderColumnIndex
            };

            result.ProducerDisposalFeesHeader = new CalcResultSummaryHeader
            {
                Name = CalcResultSummaryHeaders.OneProducerDisposalFeesWithBadDebtProvision,
                ColumnIndex = ProducerDisposalFeesHeaderColumnIndex
            };

            var materialsFromDb = context.Material.ToList();

            var materials = Mappers.MaterialMapper.Map(materialsFromDb);

            var materialsBreakdownHeader = new List<CalcResultSummaryHeader>();
            var columnIndex = MaterialsBreakdownHeaderInitialColumnIndex;

            foreach (var material in materials)
            {
                materialsBreakdownHeader.Add(new CalcResultSummaryHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex
                });
                columnIndex = columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex;
            }

            // Add disposal fee summary header
            materialsBreakdownHeader.Add(new CalcResultSummaryHeader
            {
                Name = CalcResultSummaryHeaders.DisposalFeeSummary,
                ColumnIndex = columnIndex
            });

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

            columnHeaders.AddRange([
                CalcResultSummaryHeaders.TotalProducerDisposalFee,
                CalcResultSummaryHeaders.BadDebtProvision,
                CalcResultSummaryHeaders.TotalProducerDisposalFeeWithBadDebtProvision,
                CalcResultSummaryHeaders.EnglandTotal,
                CalcResultSummaryHeaders.WalesTotal,
                CalcResultSummaryHeaders.ScotlandTotal,
                CalcResultSummaryHeaders.NorthernIrelandTotal
            ]);

            result.ColumnHeaders = columnHeaders;

            var producerDetailList = context.ProducerDetail.Where(pd => pd.CalculatorRunId == resultsRequestDto.RunId).ToList();

            var producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>();

            foreach (var producer in producerDetailList)
            {
                var materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();

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
                }

                producerDisposalFees.Add(new CalcResultSummaryProducerDisposalFees
                {
                    ProducerId = producer.ProducerId.ToString(),
                    ProducerName = producer.ProducerName ?? string.Empty,
                    SubsidiaryId = producer.SubsidiaryId ?? string.Empty,
                    Level = producer.SubsidiaryId == null ? 1 : 2,
                    TotalProducerDisposalFee = GetTotalProducerDisposalFee(materialCostSummary),
                    BadDebtProvision = GetTotalBadDebtProvision(materialCostSummary),
                    TotalProducerDisposalFeeWithBadDebtProvision = GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary),
                    EnglandTotal = GetEnglandTotal(materialCostSummary),
                    WalesTotal = GetWalesTotal(materialCostSummary),
                    ScotlandTotal = GetScotlandTotal(materialCostSummary),
                    NorthernIrelandTotal = GetNorthernIrelandTotal(materialCostSummary),
                    ProducerDisposalFeesByMaterial = materialCostSummary
                });
            }

            result.ProducerDisposalFees = producerDisposalFees;

            return result;
        }

        private static decimal GetHouseholdPackagingWasteTonnage(ProducerDetail producer, MaterialDetail material)
        {
            var householdPackagingMaterial = producer.ProducerReportedMaterials.FirstOrDefault(p => p.Material.Code == material.Code && p.PackagingType == "HH");

            return householdPackagingMaterial != null ? householdPackagingMaterial.PackagingTonnage : 0;
        }

        private static decimal GetManagedConsumerWasteTonnage(ProducerDetail producer, MaterialDetail material)
        {
            var consumerWastePackagingMaterial = producer.ProducerReportedMaterials.FirstOrDefault(p => p.Material.Code == material.Code && p.PackagingType == "CW");

            return consumerWastePackagingMaterial != null ? consumerWastePackagingMaterial.PackagingTonnage : 0;
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

        private static decimal GetBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFee = GetProducerDisposalFee(producer, material, calcResult);


            return producerDisposalFee * 6;
        }

        private static decimal GetProducerDisposalFeeWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFee = GetProducerDisposalFee(producer, material, calcResult);

            return producerDisposalFee * (1 + 6);
        }

        private static decimal GetEnglandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

            var countryApportionmentPercentage = GetCountryApportionmentPercentage(calcResult);
            if (countryApportionmentPercentage == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(countryApportionmentPercentage.EnglandDisposalCost, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out decimal value);

            return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value : 0;
        }

        private static decimal GetWalesWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

            var countryApportionmentPercentage = GetCountryApportionmentPercentage(calcResult);
            if (countryApportionmentPercentage == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(countryApportionmentPercentage.WalesDisposalCost, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out decimal value);

            return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value : 0;
        }

        private static decimal GetScotlandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

            var countryApportionmentPercentage = GetCountryApportionmentPercentage(calcResult);
            if (countryApportionmentPercentage == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(countryApportionmentPercentage.ScotlandDisposalCost, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out decimal value);

            return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value : 0;
        }

        private static decimal GetNorthernIrelandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

            var countryApportionmentPercentage = GetCountryApportionmentPercentage(calcResult);
            if (countryApportionmentPercentage == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(countryApportionmentPercentage.NorthernIrelandDisposalCost, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out decimal value);

            return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value : 0;
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
    }
}
