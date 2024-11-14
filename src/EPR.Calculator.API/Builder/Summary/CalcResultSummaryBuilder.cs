using EPR.Calculator.API.Constants;
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
        private const int ProducerCommsFeesHeaderColumnIndex = 99;
        private const int MaterialsBreakdownHeaderCommsIncrementalColumnIndex = 9;

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

            result.CommsCostHeader = new CalcResultSummaryHeader
            {
                Name = CalcResultSummaryHeaders.CommsCostSummaryHeader,
                ColumnIndex = ProducerCommsFeesHeaderColumnIndex
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
            result.MaterialBreakdownHeaders = materialsBreakdownHeader;

            var commsCostColumnIndex = ProducerCommsFeesHeaderColumnIndex;

            foreach (var material in materials)
            {
                materialsBreakdownHeader.Add(new CalcResultSummaryHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = commsCostColumnIndex
                });
                commsCostColumnIndex = commsCostColumnIndex + MaterialsBreakdownHeaderCommsIncrementalColumnIndex;
            }

            materialsBreakdownHeader.Add(new CalcResultSummaryHeader
            {
                Name = CalcResultSummaryHeaders.CommsCostSummaryHeader,
                ColumnIndex = commsCostColumnIndex
            });

            // Add disposal fee summary header
            materialsBreakdownHeader.Add(new CalcResultSummaryHeader
            {
                Name = CalcResultSummaryHeaders.DisposalFeeSummary,
                ColumnIndex = columnIndex
            });

            result.MaterialBreakdownHeaders = materialsBreakdownHeader;

            // Add disposal fee summary header
            materialsBreakdownHeader.Add(new CalcResultSummaryHeader
            {
                Name = CalcResultSummaryHeaders.OneProducerDisposalFeesWithBadDebtProvision,
                ColumnIndex = columnIndex
            });

            result.MaterialBreakdownHeaders = materialsBreakdownHeader;

            result.ColumnHeaders = CalculationResultSummaryHeaders(materials);

            //result.ColumnHeaders = columnHeaders;

            var producerDetailList = context.ProducerDetail.Where(pd => pd.CalculatorRunId == resultsRequestDto.RunId).ToList();

            var producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>();

            var producerDisposalFeesByCountry = new List<CalcResultSummaryProducerDisposalFeesByCountry>();
            var producerCommsFeesByCountry = new List<CalcResultSummaryProducerCommsFeesByCountry>();

            foreach (var producer in producerDetailList)
            {
                var materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();
                var commsCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>();

                foreach (var material in materials)
                {
                    var hhPackagingWasteTonnage = GetHouseholdPackagingWasteTonnage(producer, material);
                    decimal badDebtProvision = Convert.ToDecimal(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Trim('%'));
                    decimal priceperTonne = GetPriceperTonneForComms(producer, material);
                    decimal producerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(hhPackagingWasteTonnage, priceperTonne);
                    decimal badDebtProvisionCost = GetBadDebtProvisionForCommsCost(producerTotalCostWithoutBadDebtProvision, badDebtProvision);
                    decimal producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producerTotalCostWithoutBadDebtProvision, badDebtProvision);

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
                        HouseholdPackagingWasteTonnage = hhPackagingWasteTonnage,
                        PriceperTonne = priceperTonne,
                        ProducerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(hhPackagingWasteTonnage, priceperTonne),
                        BadDebtProvision = GetBadDebtProvisionForCommsCost(hhPackagingWasteTonnage, priceperTonne),
                        ProducerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producerTotalCostWithoutBadDebtProvision, badDebtProvision),
                        EnglandWithBadDebtProvision = GetEnglandWithBadDebtProvisionForComms(producerTotalCostwithBadDebtProvision, calcResult),
                        WalesWithBadDebtProvision = GetWalesWithBadDebtProvisionForComms(producerTotalCostwithBadDebtProvision, calcResult),
                        ScotlandWithBadDebtProvision = GetScotlandWithBadDebtProvisionForComms(producerTotalCostwithBadDebtProvision, calcResult),
                        NorthernIrelandWithBadDebtProvision = GetNorthernIrelandWithBadDebtProvisionForComms(producerTotalCostwithBadDebtProvision, calcResult)
                    });
                }

                producerDisposalFees.Add(new CalcResultSummaryProducerDisposalFees
                {
                    ProducerId = producer.Id.ToString(),
                    ProducerName = producer.ProducerName ?? string.Empty,
                    SubsidiaryId = producer.SubsidiaryId ?? string.Empty,
                    Level = producer.SubsidiaryId == null ? 1 : 2,
                    ProducerDisposalFeesByMaterial = materialCostSummary,
                    CommsCostByMaterial = commsCostSummary
                });

                producerDisposalFeesByCountry.Add(new CalcResultSummaryProducerDisposalFeesByCountry
                {
                    TotalProducerDisposalFee = GetTotalProducerDisposalFee(materialCostSummary),
                    BadDebtProvision = GetTotalBadDebtProvision(materialCostSummary),
                    TotalProducerDisposalFeeWithBadDebtProvision = GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary),
                    EnglandTotal = GetEnglandTotal(materialCostSummary),
                    WalesTotal = GetWalesTotal(materialCostSummary),
                    ScotlandTotal = GetScotlandTotal(materialCostSummary),
                    NorthernIrelandTotal = GetNorthernIrelandTotal(materialCostSummary)
                });

                producerCommsFeesByCountry.Add(new CalcResultSummaryProducerCommsFeesByCountry
                {
                    ProducerId = producer.Id.ToString(),
                    //ProducerName = producer.ProducerName ?? string.Empty,
                    //SubsidiaryId = producer.SubsidiaryId ?? string.Empty,
                    TotalProducerCommsFee = GetTotalProducerCommsFee(commsCostSummary),
                    BadDebtProvision = GetCommsTotalBadDebtProvision(commsCostSummary),
                    TotalProducerCommsFeeWithBadDebtProvision = GetTotalProducerCommsFeeWithBadDebtProvision(commsCostSummary),
                    EnglandTotal = GetEnglandCommsTotal(commsCostSummary),
                    WalesTotal = GetWalesCommsTotal(commsCostSummary),
                    ScotlandTotal = GetScotlandCommsTotal(commsCostSummary),
                    NorthernIrelandTotal = GetNorthernIrelandCommsTotal(commsCostSummary)
                });
            }

            result.ProducerDisposalFees = producerDisposalFees;

            result.ProducerDisposalFeesByCountry = producerDisposalFeesByCountry;

            result.ProducerCommsFeesByCountry = producerCommsFeesByCountry;

            return result;
        }

        private static List<string> CalculationResultSummaryHeaders(List<MaterialDetail> materials)
        {
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

            var producerDisposalFeesByCountry = new List<CalcResultSummaryProducerDisposalFeesByCountry>();

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
                    ProducerId = producer.Id.ToString(),
                    ProducerName = producer.ProducerName ?? string.Empty,
                    SubsidiaryId = producer.SubsidiaryId ?? string.Empty,
                    Level = producer.SubsidiaryId == null ? 1 : 2,
                    ProducerDisposalFeesByMaterial = materialCostSummary
                });

                producerDisposalFeesByCountry.Add(new CalcResultSummaryProducerDisposalFeesByCountry
                {
                    TotalProducerDisposalFee = GetTotalProducerDisposalFee(materialCostSummary),
                    BadDebtProvision = GetTotalBadDebtProvision(materialCostSummary),
                    TotalProducerDisposalFeeWithBadDebtProvision = GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary),
                    EnglandTotal = GetEnglandTotal(materialCostSummary),
                    WalesTotal = GetWalesTotal(materialCostSummary),
                    ScotlandTotal = GetScotlandTotal(materialCostSummary),
                    NorthernIrelandTotal = GetNorthernIrelandTotal(materialCostSummary)
                });

            foreach (var material in materials)
            {
                columnHeaders.AddRange([
                CalcResultSummaryHeaders.ReportedHouseholdPackagingWasteTonnage,
                CalcResultSummaryHeaders.PricePerTonne,
                CalcResultSummaryHeaders.ProducerTotalCostWithoutBadDebtProvision,
                CalcResultSummaryHeaders.BadDebtProvision,
                CalcResultSummaryHeaders.ProducerTotalCostwithBadDebtProvision,
                CalcResultSummaryHeaders.EnglandWithBadDebtProvision,
                CalcResultSummaryHeaders.WalesWithBadDebtProvision,
                CalcResultSummaryHeaders.ScotlandWithBadDebtProvision,
                CalcResultSummaryHeaders.NorthernIrelandWithBadDebtProvision,
                ]);
            }
            columnHeaders.AddRange([
                CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision,
                CalcResultSummaryHeaders.TotalBadDebtProvision,
                CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision,
                CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision,
                CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision,
                CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision,
                CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision,
            ]);
            return columnHeaders;
        }

        private static decimal GetNorthernIrelandCommsTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal northernIrelandTotalwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                northernIrelandTotalwithBadDebtprovision += material.Value.NorthernIrelandTotalwithBadDebtprovision;
            }

            return northernIrelandTotalwithBadDebtprovision;
        }

            result.ProducerDisposalFeesByCountry = producerDisposalFeesByCountry;

            return result;
        private static decimal GetScotlandCommsTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal scotlandTotalwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                scotlandTotalwithBadDebtprovision += material.Value.ScotlandTotalwithBadDebtprovision;
            }

            return scotlandTotalwithBadDebtprovision;
        }

        private static decimal GetWalesCommsTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal walesTotalwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                walesTotalwithBadDebtprovision += material.Value.WalesTotalwithBadDebtprovision;
            }

            return walesTotalwithBadDebtprovision;
        }

        private static decimal GetEnglandCommsTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal englandTotalwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                englandTotalwithBadDebtprovision += material.Value.EnglandTotalwithBadDebtprovision;
            }

            return englandTotalwithBadDebtprovision;
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

        private static void TwoACommsCost(CalcResultSummary result, List<MaterialDetail> materials)
        {
            var materialsBreakdownHeader = new List<CalcResultSummaryHeader>();
            var columnIndex1 = 99;

            foreach (var material in materials)
            {
                materialsBreakdownHeader.Add(new CalcResultSummaryHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex1
                });
                columnIndex1 = columnIndex1 + 9;
            }

            result.MaterialBreakdownHeaders = materialsBreakdownHeader;
        }

        private static void ProducerDisposalFees(CalcResultSummary result, List<MaterialDetail> materials)
        {
            var materialsBreakdownHeader = new List<CalcResultSummaryHeader>();
            var columnIndex = 4;

            foreach (var material in materials)
            {
                materialsBreakdownHeader.Add(new CalcResultSummaryHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex
                });
                columnIndex = columnIndex + 11;
            }
            result.MaterialBreakdownHeaders = materialsBreakdownHeader;
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

        private decimal GetPriceperTonneForComms(ProducerDetail producer, MaterialDetail material)
        {
            return 0.786M; // Tim PR
        }

        private static decimal GetProducerTotalCostWithoutBadDebtProvision(decimal HHPackagingWasteTonnage, decimal PriceperTonne)
        {
            return HHPackagingWasteTonnage * PriceperTonne;
        }

        private static decimal GetBadDebtProvisionForCommsCost(decimal ProducerTotalCostWithoutBadDebtProvision, decimal BadDebtProvision)
        {
            //Formula:  F5*'Params - Other'!$B$10
            return ProducerTotalCostWithoutBadDebtProvision * BadDebtProvision;

        }

        private static decimal GetProducerTotalCostwithBadDebtProvision(decimal ProducerTotalCostWithoutBadDebtProvision, decimal BadDebtProvision)
        {
            // Formula: F5*(1+'Params - Other'!$B$10) --uday (Build the calculator - Params - Other - 3)
            return ProducerTotalCostWithoutBadDebtProvision * (1 + BadDebtProvision);
        }

        private static decimal GetEnglandWithBadDebtProvisionForComms(decimal ProducerTotalCostwithBadDebtProvision, CalcResult calcResult)
        {
            // Formula: H5*'1 + 4 Apportionment %s'!$C$6
            return ProducerTotalCostwithBadDebtProvision * (1 + Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.EnglandDisposalTotal).ToList()[4].Trim('%')));
        }

        private static decimal GetWalesWithBadDebtProvisionForComms(decimal ProducerTotalCostwithBadDebtProvision, CalcResult calcResult)
        {
            // Formula: H5*'1 + 4 Apportionment %s'!$D$6
            return ProducerTotalCostwithBadDebtProvision * (1 + Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.WalesDisposalTotal).ToList()[4].Trim('%')));
        }

        private static decimal GetScotlandWithBadDebtProvisionForComms(decimal ProducerTotalCostwithBadDebtProvision, CalcResult calcResult)
        {
            // Formula: H5*'1 + 4 Apportionment %s'!$E$6
            return ProducerTotalCostwithBadDebtProvision * (1 + Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.ScotlandDisposalTotal).ToList()[4].Trim('%')));
        }

        private static decimal GetNorthernIrelandWithBadDebtProvisionForComms(decimal ProducerTotalCostwithBadDebtProvision, CalcResult calcResult)
        {
            // Formula: H5*'1 + 4 Apportionment %s'!$F$6
            return ProducerTotalCostwithBadDebtProvision * (1 + Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.NorthernIrelandDisposalTotal).ToList()[4].Trim('%')));
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

        private static decimal GetCommsTotalBadDebtProvision(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCostSummary)
        {
            decimal totalBadDebtProvision = 0;

            foreach (var material in materialCostSummary)
            {
                totalBadDebtProvision += material.Value.BadDebtProvision;
            }

            return totalBadDebtProvision;
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

        private static decimal GetTotalProducerCommsFee(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal producerTotalCostWithoutBadDebtProvision = 0;

            foreach (var material in commsCostSummary)
            {
                producerTotalCostWithoutBadDebtProvision += material.Value.ProducerTotalCostWithoutBadDebtProvision;
            }

            return producerTotalCostWithoutBadDebtProvision;
        }

        private static decimal GetTotalBadDebtProvision(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCostSummary)
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
