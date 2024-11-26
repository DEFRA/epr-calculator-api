using System.Globalization;
using EPR.Calculator.API.Builder.Summary.CommsCostTwoA;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.Common;

public static class CalcResultSummaryUtil
{
    public static int GetLevelIndex(List<CalcResultSummaryProducerDisposalFees> producerDisposalFeesLookup, ProducerDetail producer)
    {
        var totalRow = producerDisposalFeesLookup.Find(pdf => pdf.ProducerId == producer.ProducerId.ToString() && pdf.isTotalRow);

        return totalRow == null ? 1 : 2;
    }

    public static decimal GetHouseholdPackagingWasteTonnage(ProducerDetail producer, MaterialDetail material)
    {
        var householdPackagingMaterial = producer.ProducerReportedMaterials.FirstOrDefault(p => p.Material.Code == material.Code && p.PackagingType == "HH");

        return householdPackagingMaterial != null ? householdPackagingMaterial.PackagingTonnage : 0;
    }

    public static decimal GetHouseholdPackagingWasteTonnageProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += GetHouseholdPackagingWasteTonnage(producer, material);
        }

        return totalCost;
    }

    public static decimal GetManagedConsumerWasteTonnage(ProducerDetail producer, MaterialDetail material)
    {
        var consumerWastePackagingMaterial = producer.ProducerReportedMaterials.FirstOrDefault(p => p.Material.Code == material.Code && p.PackagingType == "CW");

        return consumerWastePackagingMaterial != null ? consumerWastePackagingMaterial.PackagingTonnage : 0;
    }

    public static decimal GetManagedConsumerWasteTonnageProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += GetManagedConsumerWasteTonnage(producer, material);
        }

        return totalCost;
    }

    public static decimal GetNetReportedTonnage(ProducerDetail producer, MaterialDetail material)
    {
        var householdPackagingWasteTonnage = GetHouseholdPackagingWasteTonnage(producer, material);
        var managedConsumerWasteTonnage = GetManagedConsumerWasteTonnage(producer, material);

        if (householdPackagingWasteTonnage == 0 || managedConsumerWasteTonnage == 0)
        {
            return 0;
        }

        return householdPackagingWasteTonnage - managedConsumerWasteTonnage;
    }

    public static decimal GetNetReportedTonnageProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += GetNetReportedTonnage(producer, material);
        }

        return totalCost;
    }

    public static decimal GetPricePerTonne(MaterialDetail material, CalcResult calcResult)
    {
        var laDisposalCostDataDetail = calcResult.CalcResultLaDisposalCostData.CalcResultLaDisposalCostDetails.FirstOrDefault(la => la.Name == material.Name);

        if (laDisposalCostDataDetail == null)
        {
            return 0;
        }

        var isParseSuccessful = decimal.TryParse(laDisposalCostDataDetail.DisposalCostPricePerTonne, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-GB"), out decimal value);

        return isParseSuccessful ? value : 0;
    }

    public static decimal GetProducerDisposalFee(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
    {
        var netReportedTonnage = GetNetReportedTonnage(producer, material);
        var pricePerTonne = GetPricePerTonne(material, calcResult);

        if (netReportedTonnage == 0 || pricePerTonne == 0)
        {
            return 0;
        }

        return netReportedTonnage * pricePerTonne;
    }

    public static decimal GetProducerDisposalFeeProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += GetProducerDisposalFee(producer, material, calcResult);
        }

        return totalCost;
    }

    public static decimal GetBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
    {
        var producerDisposalFee = GetProducerDisposalFee(producer, material, calcResult);

        var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

        if (isParseSuccessful)
        {
            return producerDisposalFee * value / 100;
        }

        return 0;
    }

    public static decimal GetBadDebtProvisionProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += GetBadDebtProvision(producer, material, calcResult);
        }

        return totalCost;
    }

    public static decimal GetProducerDisposalFeeWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
    {
        var producerDisposalFee = GetProducerDisposalFee(producer, material, calcResult);

        var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

        if (isParseSuccessful)
        {
            return producerDisposalFee * (1 + value / 100);
        }

        return 0;
    }

    public static decimal GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);
        }

        return totalCost;
    }

    public static decimal GetEnglandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
    {
        var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

        var countryApportionmentPercentage = GetCountryApportionmentPercentage(calcResult);
        if (countryApportionmentPercentage == null)
        {
            return 0;
        }

        var isParseSuccessful = decimal.TryParse((string?)countryApportionmentPercentage.EnglandDisposalCost.Replace("%", string.Empty), out decimal value);

        return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value / 100 : 0;
    }

    public static decimal GetEnglandWithBadDebtProvisionProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += GetEnglandWithBadDebtProvision(producer, material, calcResult);
        }

        return totalCost;
    }

    public static decimal GetWalesWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
    {
        var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

        var countryApportionmentPercentage = GetCountryApportionmentPercentage(calcResult);
        if (countryApportionmentPercentage == null)
        {
            return 0;
        }

        var isParseSuccessful = decimal.TryParse((string?)countryApportionmentPercentage.WalesDisposalCost.Replace("%", string.Empty), out decimal value);

        return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value / 100 : 0;
    }

    public static decimal GetWalesWithBadDebtProvisionProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += GetWalesWithBadDebtProvision(producer, material, calcResult);
        }

        return totalCost;
    }

    public static decimal GetScotlandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
    {
        var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

        var countryApportionmentPercentage = GetCountryApportionmentPercentage(calcResult);
        if (countryApportionmentPercentage == null)
        {
            return 0;
        }

        var isParseSuccessful = decimal.TryParse((string?)countryApportionmentPercentage.ScotlandDisposalCost.Replace("%", string.Empty), out decimal value);

        return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value / 100 : 0;
    }

    public static decimal GetScotlandWithBadDebtProvisionProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += GetScotlandWithBadDebtProvision(producer, material, calcResult);
        }

        return totalCost;
    }

    public static decimal GetNorthernIrelandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
    {
        var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

        var countryApportionmentPercentage = GetCountryApportionmentPercentage(calcResult);
        if (countryApportionmentPercentage == null)
        {
            return 0;
        }

        var isParseSuccessful = decimal.TryParse((string?)countryApportionmentPercentage.NorthernIrelandDisposalCost.Replace("%", string.Empty), out decimal value);

        return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value / 100 : 0;
    }

    public static decimal GetNorthernIrelandWithBadDebtProvisionProducerTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += GetNorthernIrelandWithBadDebtProvision(producer, material, calcResult);
        }

        return totalCost;
    }

    public static CalcResultLapcapDataDetails? GetCountryApportionmentPercentage(CalcResult calcResult)
    {
        return calcResult.CalcResultLapcapData.CalcResultLapcapDataDetails?.FirstOrDefault(la => la.Name == CalcResultSummaryHeaders.OneCountryApportionment);
    }

    public static decimal GetTotalProducerDisposalFee(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
    {
        decimal totalProducerDisposalFee = 0;

        foreach (var material in materialCostSummary)
        {
            totalProducerDisposalFee += material.Value.ProducerDisposalFee;
        }

        return totalProducerDisposalFee;
    }

    public static decimal GetTotalBadDebtProvision(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
    {
        decimal totalBadDebtProvision = 0;

        foreach (var material in materialCostSummary)
        {
            totalBadDebtProvision += material.Value.BadDebtProvision;
        }

        return totalBadDebtProvision;
    }

    public static decimal GetTotalProducerDisposalFeeWithBadDebtProvision(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
    {
        decimal totalProducerDisposalFeeWithBadDebtProvision = 0;

        foreach (var material in materialCostSummary)
        {
            totalProducerDisposalFeeWithBadDebtProvision += material.Value.ProducerDisposalFeeWithBadDebtProvision;
        }

        return totalProducerDisposalFeeWithBadDebtProvision;
    }

    public static decimal GetEnglandTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
    {
        decimal totalEngland = 0;

        foreach (var material in materialCostSummary)
        {
            totalEngland += material.Value.EnglandWithBadDebtProvision;
        }

        return totalEngland;
    }

    public static decimal GetWalesTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
    {
        decimal totalWales = 0;

        foreach (var material in materialCostSummary)
        {
            totalWales += material.Value.WalesWithBadDebtProvision;
        }

        return totalWales;
    }

    public static decimal GetScotlandTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
    {
        decimal totalScotland = 0;

        foreach (var material in materialCostSummary)
        {
            totalScotland += material.Value.ScotlandWithBadDebtProvision;
        }

        return totalScotland;
    }

    public static decimal GetNorthernIrelandTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
    {
        decimal totalNorthernIreland = 0;

        foreach (var material in materialCostSummary)
        {
            totalNorthernIreland += material.Value.NorthernIrelandWithBadDebtProvision;
        }

        return totalNorthernIreland;
    }

    public static decimal GetLaDataPrepCostsTitleSection4(CalcResult calcResult)
    {
        return calcResult.CalcResultParameterOtherCost.Details.ToList()[0].TotalValue;
    }

    public static decimal GetLaDataPrepCostsBadDebtProvisionTitleSection4(CalcResult calcResult)
    {
        var isConversionSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

        return isConversionSuccessful ? GetLaDataPrepCostsTitleSection4(calcResult) * value : 0;
    }

    public static decimal GetLaDataPrepCostsWithoutBadDebtProvisionTitleSection4(CalcResult calcResult)
    {
        return GetLaDataPrepCostsTitleSection4(calcResult) + GetLaDataPrepCostsBadDebtProvisionTitleSection4(calcResult);
    }

    public static decimal GetLaDataPrepCostsWithBadDebtProvisionTitleSection4(CalcResult calcResult)
    {
        return GetLaDataPrepCostsTitleSection4(calcResult) + GetLaDataPrepCostsBadDebtProvisionTitleSection4(calcResult);
    }

    public static decimal GetTotal1Plus2ABadDebt(IEnumerable<ProducerDetail> producers, IEnumerable<MaterialDetail> materials, CalcResult calcResult)
    {
        decimal total = 0m;

        foreach (var material in materials)
        {
            var laDisposalTotal = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(producers, material, calcResult);
            var twoAcommsDisposal = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvisionTotal(producers, material, calcResult);
            total += laDisposalTotal + twoAcommsDisposal;
        }

        return total;
    }

    public static decimal GetLaDataPrepCostsTotalWithoutBadDebtProvisionSection4()
    {
        return 99;
    }

    public static decimal GetLaDataPrepCostsBadDebtProvisionSection4()
    {
        return 99;
    }

    public static decimal GetLaDataPrepCostsTotalWithBadDebtProvisionSection4()
    {
        return 99;
    }

    public static decimal GetLaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4()
    {
        return 99;
    }

    public static decimal GetLaDataPrepCostsWalesTotalWithBadDebtProvisionSection4()
    {
        return 99;
    }

    public static decimal GetLaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4()
    {
        return 99;
    }

    public static decimal GetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4()
    {
        return 99;
    }

    public static void SetHeaders(CalcResultSummary result, List<MaterialDetail> materials)
    {
        result.ResultSummaryHeader = new CalcResultSummaryHeader
        {
            Name = CalcResultSummaryHeaders.CalculationResult,
            ColumnIndex = CalcResultSummaryBuilder.ResultSummaryHeaderColumnIndex
        };

        result.ProducerDisposalFeesHeaders = GetProducerDisposalFeesHeaders();

        result.MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(result, materials);

        result.ColumnHeaders = GetColumnHeaders(materials);
    }

    public static List<CalcResultSummaryHeader> GetProducerDisposalFeesHeaders()
    {
        return [
            //Section-1 Title headers
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.OneProducerDisposalFeesWithBadDebtProvision, ColumnIndex = CalcResultSummaryBuilder.ProducerDisposalFeesHeaderColumnIndex },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostHeader, ColumnIndex = CalcResultSummaryBuilder.CommsCostHeaderColumnIndex },
                
            //Section-(1) & (2a) Title headers   
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforLADisposalCostswoBadDebtprovision1, ColumnIndex = CalcResultSummaryBuilder.DisposalFeeCommsCostsHeaderInitialColumnIndex },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision,ColumnIndex = CalcResultSummaryBuilder.DisposalFeeCommsCostsHeaderInitialColumnIndex +1 },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforLADisposalCostswithBadDebtprovision1, ColumnIndex = CalcResultSummaryBuilder.DisposalFeeCommsCostsHeaderInitialColumnIndex +2 },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforCommsCostsbyMaterialwoBadDebtprovision2A, ColumnIndex = CalcResultSummaryBuilder.DisposalFeeCommsCostsHeaderInitialColumnIndex + 5 },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision, ColumnIndex = CalcResultSummaryBuilder.DisposalFeeCommsCostsHeaderInitialColumnIndex +6 },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforCommsCostsbyMaterialwithBadDebtprovision2A,ColumnIndex = CalcResultSummaryBuilder.DisposalFeeCommsCostsHeaderInitialColumnIndex +7  },
            new CalcResultSummaryHeader {Name = CalcResultSummaryHeaders.TotalBadDebtProvision1Plus2A, ColumnIndex = CalcResultSummaryBuilder.Total1Plus2ABadDebt},
            //Section-4 Title headers
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.LaDataPrepCostsWithoutBadDebtProvisionTitleSection4, ColumnIndex = CalcResultSummaryBuilder.LaDataPrepCostsSection4ColumnIndex },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvisionTitleSection4, ColumnIndex = CalcResultSummaryBuilder.LaDataPrepCostsSection4ColumnIndex+1 },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.LaDataPrepCostsWithBadDebtProvisionTitleSection4, ColumnIndex = CalcResultSummaryBuilder.LaDataPrepCostsSection4ColumnIndex+2 },
        ];
    }

    public static List<CalcResultSummaryHeader> GetMaterialsBreakdownHeader(CalcResultSummary result, List<MaterialDetail> materials)
    {
        var materialsBreakdownHeaders = new List<CalcResultSummaryHeader>();
        var columnIndex = CalcResultSummaryBuilder.MaterialsBreakdownHeaderInitialColumnIndex;

        foreach (var material in materials)
        {
            materialsBreakdownHeaders.Add(new CalcResultSummaryHeader
            {
                Name = $"{material.Name} Breakdown",
                ColumnIndex = columnIndex
            });
            columnIndex = columnIndex + CalcResultSummaryBuilder.MaterialsBreakdownHeaderIncrementalColumnIndex;
        }

        // Add disposal fee summary header
        materialsBreakdownHeaders.Add(new CalcResultSummaryHeader
        {
            Name = CalcResultSummaryHeaders.DisposalFeeSummary,
            ColumnIndex = CalcResultSummaryBuilder.DisposalFeeSummaryColumnIndex
        });

        var commsCostColumnIndex = CalcResultSummaryBuilder.MaterialsBreakdownHeaderCommsInitialColumnIndex;

        foreach (var material in materials)
        {
            materialsBreakdownHeaders.Add(new CalcResultSummaryHeader
            {
                Name = $"{material.Name} Breakdown",
                ColumnIndex = commsCostColumnIndex
            });
            commsCostColumnIndex = commsCostColumnIndex + CalcResultSummaryBuilder.MaterialsBreakdownHeaderCommsIncrementalColumnIndex;
        }

        materialsBreakdownHeaders.Add(new CalcResultSummaryHeader
        {
            Name = CalcResultSummaryHeaders.CommsCostSummaryHeader,
            ColumnIndex = commsCostColumnIndex
        });

        //Section-(1) & (2a)
        materialsBreakdownHeaders.AddRange([
            new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalFeeforLADisposalCostswoBadDebtprovision1, CalcResultSummaryBuilder.decimalRoundUp)}", ColumnIndex = CalcResultSummaryBuilder.DisposalFeeCommsCostsHeaderInitialColumnIndex },
            new CalcResultSummaryHeader { Name = $"£{Math.Round(result.BadDebtProvisionFor1, CalcResultSummaryBuilder.decimalRoundUp)}", ColumnIndex = CalcResultSummaryBuilder.DisposalFeeCommsCostsHeaderInitialColumnIndex+1 },
            new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalFeeforLADisposalCostswithBadDebtprovision1, CalcResultSummaryBuilder.decimalRoundUp)}",ColumnIndex = CalcResultSummaryBuilder.DisposalFeeCommsCostsHeaderInitialColumnIndex+2 }
        ]);

        materialsBreakdownHeaders.AddRange([
            new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A, CalcResultSummaryBuilder.decimalRoundUp)}", ColumnIndex = CalcResultSummaryBuilder.DisposalFeeCommsCostsHeaderInitialColumnIndex + 5 },
            new CalcResultSummaryHeader { Name = $"£{Math.Round(result.BadDebtProvisionFor2A, CalcResultSummaryBuilder.decimalRoundUp)}",ColumnIndex = CalcResultSummaryBuilder.DisposalFeeCommsCostsHeaderInitialColumnIndex+6 },
            new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A, CalcResultSummaryBuilder.decimalRoundUp)}",ColumnIndex = CalcResultSummaryBuilder.DisposalFeeCommsCostsHeaderInitialColumnIndex+7 }
        ]);

        materialsBreakdownHeaders.AddRange([
            new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalOnePlus2AFeeWithBadDebtProvision, CalcResultSummaryBuilder.decimalRoundUp)}", ColumnIndex = CalcResultSummaryBuilder.Total1Plus2ABadDebt },
        ]);

        // LA data prep costs section 4
        materialsBreakdownHeaders.AddRange([
            new CalcResultSummaryHeader { Name = $"{result.LaDataPrepCostsTitleSection4}", ColumnIndex = CalcResultSummaryBuilder.LaDataPrepCostsSection4ColumnIndex },
            new CalcResultSummaryHeader { Name = $"{result.LaDataPrepCostsBadDebtProvisionTitleSection4}", ColumnIndex = CalcResultSummaryBuilder.LaDataPrepCostsSection4ColumnIndex+1 },
            new CalcResultSummaryHeader { Name = $"{result.LaDataPrepCostsWithBadDebtProvisionTitleSection4}",ColumnIndex = CalcResultSummaryBuilder.LaDataPrepCostsSection4ColumnIndex+2 }
        ]);

        return materialsBreakdownHeaders;
    }

    public static List<CalcResultSummaryHeader> GetColumnHeaders(List<MaterialDetail> materials)
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
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforLADisposalCostswoBadDebtprovision, ColumnIndex = CalcResultSummaryBuilder.DisposalFeeCommsCostsHeaderInitialColumnIndex },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvisionFor1 },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforLADisposalCostswithBadDebtprovision },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision }
        ]);

        columnHeaders.AddRange([
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision2A, ColumnIndex = CalcResultSummaryBuilder.DisposalFeeCommsCostsHeaderInitialColumnIndex },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvisionfor2A },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision2A },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision }
        ]);

        // bad debt
        columnHeaders.AddRange([
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducer1Plus2ABadDebt },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducer1Plus2ABadDebtPercentage }
        ]);

        // Percentage of Producer Reported Household Tonnage vs All Producers
        columnHeaders.AddRange([
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PercentageofProducerReportedHHTonnagevsAllProducers },
        ]);

        // LA data prep costs section 4 column headers
        columnHeaders.AddRange([
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeWithoutBadDebtProvisionSection4, ColumnIndex = CalcResultSummaryBuilder.LaDataPrepCostsSection4ColumnIndex },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvisionSection4 },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeWithBadDebtProvisionSection4 },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotalWithBadDebtProvisionSection4 },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotalWithBadDebtProvisionSection4 },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotalWithBadDebtProvisionSection4 },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotalWithBadDebtProvisionSection4 }
        ]);

        return columnHeaders;
    }

    public static decimal GetTotalProducerCommsFee(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
    {
        decimal producerTotalCostWithoutBadDebtProvision = 0;

        foreach (var material in commsCostSummary)
        {
            producerTotalCostWithoutBadDebtProvision += material.Value.ProducerTotalCostWithoutBadDebtProvision;
        }

        return producerTotalCostWithoutBadDebtProvision;
    }

    public static decimal GetCommsTotalBadDebtProvision(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCostSummary)
    {
        decimal totalBadDebtProvision = 0;

        foreach (var material in materialCostSummary)
        {
            totalBadDebtProvision += material.Value.BadDebtProvision;
        }

        return totalBadDebtProvision;
    }

    public static decimal GetTotalProducerCommsFeeWithBadDebtProvision(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
    {
        decimal totalCommsCostsbyMaterialwithBadDebtprovision = 0;

        foreach (var material in commsCostSummary)
        {
            totalCommsCostsbyMaterialwithBadDebtprovision += material.Value.ProducerTotalCostwithBadDebtProvision;
        }

        return totalCommsCostsbyMaterialwithBadDebtprovision;
    }

    public static decimal GetNorthernIrelandCommsTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
    {
        decimal northernIrelandTotalwithBadDebtprovision = 0;

        foreach (var material in commsCostSummary)
        {
            northernIrelandTotalwithBadDebtprovision += material.Value.NorthernIrelandWithBadDebtProvision;
        }

        return northernIrelandTotalwithBadDebtprovision;
    }

    public static decimal GetScotlandCommsTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
    {
        decimal scotlandTotalwithBadDebtprovision = 0;

        foreach (var material in commsCostSummary)
        {
            scotlandTotalwithBadDebtprovision += material.Value.ScotlandWithBadDebtProvision;
        }

        return scotlandTotalwithBadDebtprovision;
    }

    public static decimal GetWalesCommsTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
    {
        decimal walesTotalwithBadDebtprovision = 0;

        foreach (var material in commsCostSummary)
        {
            walesTotalwithBadDebtprovision += material.Value.WalesWithBadDebtProvision;
        }

        return walesTotalwithBadDebtprovision;
    }

    public static decimal GetEnglandCommsTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
    {
        decimal englandTotalwithBadDebtprovision = 0;

        foreach (var material in commsCostSummary)
        {
            englandTotalwithBadDebtprovision += material.Value.EnglandWithBadDebtProvision;
        }

        return englandTotalwithBadDebtprovision;
    }
}