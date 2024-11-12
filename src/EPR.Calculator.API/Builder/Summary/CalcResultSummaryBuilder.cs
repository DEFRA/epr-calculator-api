using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using System.Collections.Generic;
using System.Globalization;

namespace EPR.Calculator.API.Builder.Summary
{
    public class CalcResultSummaryBuilder : ICalcResultSummaryBuilder
    {
        private readonly ApplicationDBContext context;
        private const string ProducerDisposalFeesHeader = "1 Producer Disposal Fees with Bad Debt Provision";
        private const string TwoACommsCostHeader = "2a Total for Comms Cost - by Material with Bad Debt provision";


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

            var materialsFromDb = context.Material.ToList();
            var materials = Mappers.MaterialMapper.Map(materialsFromDb);

            result.ProducerDisposalFeesHeader = new CalcResultSummaryHeader
            {
                Name = ProducerDisposalFeesHeader,
                ColumnIndex = 4
            };

            result.TwoACommsCostHeader = new CalcResultSummaryHeader
            {
                Name = TwoACommsCostHeader,
                ColumnIndex = 99
            };
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

            materialsBreakdownHeader.Add(new CalcResultSummaryHeader
            {
                Name = $"Summary of Fee for Comms Costs - by Material",
                ColumnIndex = columnIndex1
            });

            result.MaterialBreakdownHeaders = materialsBreakdownHeader;

            var columnHeaders = new List<string>();
            columnHeaders.AddRange([
                CalcResultSummaryHeaders.ProducerId,
                CalcResultSummaryHeaders.SubsidiaryId,
                CalcResultSummaryHeaders.ProducerOrSubsidiaryName,
                CalcResultSummaryHeaders.Level
            ]);

            BuildCalcResultSummaryHeaders(result, materials, columnHeaders);

            var producerDetailList = context.ProducerDetail.ToList();

            List<CalcResultSummaryProducerDisposalFees> calcResultSummaryData = CalcResultSummaryData(calcResult, materials, producerDetailList);

            result.ProducerDisposalFees = calcResultSummaryData;

            return result;
        }

        private List<CalcResultSummaryProducerDisposalFees> CalcResultSummaryData(CalcResult calcResult, List<MaterialDetail> materials, List<ProducerDetail> producerDetailList)
        {
            var producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>();

            foreach (var producer in producerDetailList)
            {
                var materialCostSummary = new Dictionary<MaterialDetail, IEnumerable<CalcResultSummaryProducerDisposalFeesByMaterial>>();
                var twoAMaterialCostSummary = new Dictionary<MaterialDetail, IEnumerable<CalcResultSummaryTwoACommsCostByMaterial>>();

                foreach (var material in materials)
                {
                    var hhPackagingWasteTonnage = GetHouseholdPackagingWasteTonnage(producer, material);
                    decimal BadDebtProvision = Convert.ToDecimal(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Trim('%'));
                    decimal PriceperTonne = GetPriceperTonne_FromParamOthers(producer, material); // by Tim
                    decimal ProducerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(hhPackagingWasteTonnage, PriceperTonne);
                    decimal BadDebtProvisionCost = GetBadDebtProvisionForTwoAcommsCost(ProducerTotalCostWithoutBadDebtProvision, BadDebtProvision);
                    decimal ProducerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(ProducerTotalCostWithoutBadDebtProvision, BadDebtProvision);
                    
                    var costSummary = new List<CalcResultSummaryProducerDisposalFeesByMaterial>
                    {
                        new() {
                            HouseholdPackagingWasteTonnage = hhPackagingWasteTonnage,
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
                        }
                    };

                    var twoACommsCostSummary = new List<CalcResultSummaryTwoACommsCostByMaterial>
                    {
                        new() {
                            HouseholdPackagingWasteTonnage = hhPackagingWasteTonnage,
                            PriceperTonne = PriceperTonne,
                            ProducerTotalCostWithoutBadDebtProvision = ProducerTotalCostWithoutBadDebtProvision,
                            BadDebtProvision = BadDebtProvisionCost,
                            ProducerTotalCostwithBadDebtProvision = ProducerTotalCostwithBadDebtProvision,
                            EnglandWithBadDebtProvision = GetEnglandWithBadDebtProvisionFor2aComms(ProducerTotalCostwithBadDebtProvision, calcResult),
                            WalesWithBadDebtProvision = GetWalesWithBadDebtProvisionFor2aComms(ProducerTotalCostwithBadDebtProvision, calcResult),
                            ScotlandWithBadDebtProvision = GetScotlandWithBadDebtProvisionFor2aComms(ProducerTotalCostwithBadDebtProvision, calcResult),
                            NorthernIrelandWithBadDebtProvision = GetNorthernIrelandWithBadDebtProvisionFor2aComms(ProducerTotalCostwithBadDebtProvision, calcResult)
                        }
                    };

                    materialCostSummary.Add(material, costSummary);
                    twoAMaterialCostSummary.Add(material, twoACommsCostSummary);
                    CalcSummaryTotals(twoACommsCostSummary, twoAMaterialCostSummary);
                    CalcSummaryTotals1(costSummary, materialCostSummary);

                }

                producerDisposalFees.Add(new CalcResultSummaryProducerDisposalFees
                {
                    ProducerId = producer.Id.ToString(),
                    ProducerName = producer.ProducerName ?? string.Empty,
                    SubsidiaryId = producer.SubsidiaryId ?? string.Empty,
                    Level = 1,
                    Order = 2,
                    ProducerDisposalFeesByMaterial = materialCostSummary,
                    TwoACommsCostByMaterial = twoAMaterialCostSummary
                });
            }

            return producerDisposalFees;
        }

        private void CalcSummaryTotals1(List<CalcResultSummaryProducerDisposalFeesByMaterial> costSummary, Dictionary<MaterialDetail, IEnumerable<CalcResultSummaryProducerDisposalFeesByMaterial>> materialCostSummary)
        {
            // Define variables to store summary totals
            decimal totalProducerFeeWithoutBadDebt = 0;
            decimal totalProducerFeeWithBadDebt = 0;
            decimal totalBadDebtProvision = 0;
            decimal englandTotalWithBadDebt = 0;
            decimal walesTotalWithBadDebt = 0;
            decimal scotlandTotalWithBadDebt = 0;
            decimal northernIrelandTotalWithBadDebt = 0;

            var allFees1 = materialCostSummary.Values.SelectMany(fees => fees);

            foreach (var costItem in allFees1)
            {
                totalProducerFeeWithoutBadDebt += 0;
                totalProducerFeeWithBadDebt += 0;
                totalBadDebtProvision += 0;
                englandTotalWithBadDebt += 0;
                walesTotalWithBadDebt += 0;
                scotlandTotalWithBadDebt += 0;
                northernIrelandTotalWithBadDebt += 0;
            }

            // Add item to summary list if required
            costSummary.Add(new CalcResultSummaryProducerDisposalFeesByMaterial
            {
                TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision = totalProducerFeeWithoutBadDebt,
                TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision = totalProducerFeeWithBadDebt,
                TotalBadDebtProvision = totalBadDebtProvision,
                EnglandTotalwithBadDebtprovision = englandTotalWithBadDebt,
                WalesTotalwithBadDebtprovision = walesTotalWithBadDebt,
                ScotlandTotalwithBadDebtprovision = scotlandTotalWithBadDebt,
                NorthernIrelandTotalwithBadDebtprovision = northernIrelandTotalWithBadDebt
            });
        }

        private static void CalcSummaryTotals(List<CalcResultSummaryTwoACommsCostByMaterial> twoACommsCostSummary, Dictionary<MaterialDetail, IEnumerable<CalcResultSummaryTwoACommsCostByMaterial>> twoAMaterialCostSummary)
        {
            // Define variables to store summary totals
            decimal totalProducerFeeWithoutBadDebt = 0;
            decimal totalProducerFeeWithBadDebt = 0;
            decimal totalBadDebtProvision = 0;
            decimal englandTotalWithBadDebt = 0;
            decimal walesTotalWithBadDebt = 0;
            decimal scotlandTotalWithBadDebt = 0;
            decimal northernIrelandTotalWithBadDebt = 0;

            var allFees1 = twoAMaterialCostSummary.Values.SelectMany(fees => fees);

            foreach (var costItem in allFees1)
            {
                totalProducerFeeWithoutBadDebt += costItem.ProducerTotalCostWithoutBadDebtProvision;
                totalProducerFeeWithBadDebt += costItem.ProducerTotalCostwithBadDebtProvision;
                totalBadDebtProvision += costItem.BadDebtProvision;
                englandTotalWithBadDebt += costItem.EnglandWithBadDebtProvision;
                walesTotalWithBadDebt += costItem.WalesWithBadDebtProvision;
                scotlandTotalWithBadDebt += costItem.ScotlandWithBadDebtProvision;
                northernIrelandTotalWithBadDebt += costItem.NorthernIrelandWithBadDebtProvision;
            }

            // Add item to summary list if required
            twoACommsCostSummary.Add(new CalcResultSummaryTwoACommsCostByMaterial
            {
                TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision = totalProducerFeeWithoutBadDebt,
                TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision = totalProducerFeeWithBadDebt,
                TotalBadDebtProvision = totalBadDebtProvision,
                EnglandTotalwithBadDebtprovision = englandTotalWithBadDebt,
                WalesTotalwithBadDebtprovision = walesTotalWithBadDebt,
                ScotlandTotalwithBadDebtprovision = scotlandTotalWithBadDebt,
                NorthernIrelandTotalwithBadDebtprovision = northernIrelandTotalWithBadDebt
            });
        }


        private static void BuildCalcResultSummaryHeaders(CalcResultSummary result, List<MaterialDetail> materials, List<string> columnHeaders)
        {
            if (result.ProducerDisposalFeesHeader.Name == ProducerDisposalFeesHeader)
            {
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
                    "","","","","","",""
                    ]);

            }
            if (result.TwoACommsCostHeader.Name == TwoACommsCostHeader)
            {
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
            }

            result.ColumnHeaders = columnHeaders;
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

        private decimal GetPriceperTonne_FromParamOthers(ProducerDetail producer, MaterialDetail material)
        {
            return 0.01M; // Tim PR
        }

        private static decimal GetProducerTotalCostWithoutBadDebtProvision(decimal HHPackagingWasteTonnage, decimal PriceperTonne)
        {
            return 0.01M; //HHPackagingWasteTonnage * PriceperTonne;
        }

        private static decimal GetBadDebtProvisionForTwoAcommsCost(decimal ProducerTotalCostWithoutBadDebtProvision, decimal BadDebtProvision)
        {
            //Formula:  F5*'Params - Other'!$B$10
            return 0.01M * BadDebtProvision; //ProducerTotalCostWithoutBadDebtProvision * BadDebtProvision;

        }

        private static decimal GetProducerTotalCostwithBadDebtProvision(decimal ProducerTotalCostWithoutBadDebtProvision, decimal BadDebtProvision)
        {
            // Formula: F5*(1+'Params - Other'!$B$10) --uday (Build the calculator - Params - Other - 3)
            return ProducerTotalCostWithoutBadDebtProvision * (1 + BadDebtProvision);
        }

        private static decimal GetEnglandWithBadDebtProvisionFor2aComms(decimal ProducerTotalCostwithBadDebtProvision, CalcResult calcResult)
        {
            // Formula: H5*'1 + 4 Apportionment %s'!$C$6
            return ProducerTotalCostwithBadDebtProvision * (1 + Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.EnglandDisposalTotal).ToList()[4].Trim('%')));
        }

        private static decimal GetWalesWithBadDebtProvisionFor2aComms(decimal ProducerTotalCostwithBadDebtProvision, CalcResult calcResult)
        {
            // Formula: H5*'1 + 4 Apportionment %s'!$D$6
            return ProducerTotalCostwithBadDebtProvision * (1 + Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.WalesDisposalTotal).ToList()[4].Trim('%')));
        }

        private static decimal GetScotlandWithBadDebtProvisionFor2aComms(decimal ProducerTotalCostwithBadDebtProvision, CalcResult calcResult)
        {
            // Formula: H5*'1 + 4 Apportionment %s'!$E$6
            return ProducerTotalCostwithBadDebtProvision * (1 + Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.ScotlandDisposalTotal).ToList()[4].Trim('%')));
        }

        private static decimal GetNorthernIrelandWithBadDebtProvisionFor2aComms(decimal ProducerTotalCostwithBadDebtProvision, CalcResult calcResult)
        {
            // Formula: H5*'1 + 4 Apportionment %s'!$F$6
            return ProducerTotalCostwithBadDebtProvision * (1 + Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.NorthernIrelandDisposalTotal).ToList()[4].Trim('%')));
        }
    }
}
