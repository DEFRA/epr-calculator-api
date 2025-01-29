using System.Globalization;
using EPR.Calculator.API.Builder.Summary.Common;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.CommsCostTwoA;

public static class CalcResultSummaryCommsCostTwoA
{
    public static decimal GetEnglandWithBadDebtProvisionForCommsTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForComms(producer, material, calcResult);
        }

        return totalCost;
    }

    public static decimal GetWalesWithBadDebtProvisionForCommsTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForComms(producer, material, calcResult);
        }

        return totalCost;
    }

    public static decimal GetScotlandWithBadDebtProvisionForCommsTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForComms(producer, material, calcResult);
        }

        return totalCost;
    }

    public static decimal GetNorthernIrelandWithBadDebtProvisionForCommsTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForComms(producer, material, calcResult);
        }

        return totalCost;
    }

    public static decimal GetProducerTotalCostWithoutBadDebtProvisionTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvision(producer, material, calcResult);
        }

        return totalCost;
    }

    public static decimal GetBadDebtProvisionForCommsCostTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCost(producer, material, calcResult);
        }

        return totalCost;
    }

    public static decimal GetEnglandWithBadDebtProvisionForComms(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
    {
        var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
        return producerTotalCostwithBadDebtProvision * (Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.EnglandDisposalTotal).ToList()[4].Trim('%')) / 100);
    }

    public static decimal GetWalesWithBadDebtProvisionForComms(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
    {
        var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
        return producerTotalCostwithBadDebtProvision * (Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.WalesDisposalTotal).ToList()[4].Trim('%')) / 100);
    }

    public static decimal GetScotlandWithBadDebtProvisionForComms(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
    {
        var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
        return producerTotalCostwithBadDebtProvision * (Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.ScotlandDisposalTotal).ToList()[4].Trim('%')) / 100);
    }

    public static decimal GetNorthernIrelandWithBadDebtProvisionForComms(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
    {
        var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
        return producerTotalCostwithBadDebtProvision * (Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.NorthernIrelandDisposalTotal).ToList()[4].Trim('%')) / 100);
    }

    public static decimal GetPriceperTonneForComms(MaterialDetail material, CalcResult calcResult)
    {
        var commsCostDataDetail = calcResult.CalcResultCommsCostReportDetail.CalcResultCommsCostCommsCostByMaterial.FirstOrDefault(la => la.Name == material.Name);

        if (commsCostDataDetail == null)
        {
            return 0;
        }

        var isParseSuccessful = decimal.TryParse(commsCostDataDetail.CommsCostByMaterialPricePerTonne, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out decimal value);

        return isParseSuccessful ? value : 0;
    }

    public static decimal GetProducerTotalCostwithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
    {
        var badDebtProvision = Convert.ToDecimal(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Trim('%'));
        var producerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(producer, material, calcResult);
        return producerTotalCostWithoutBadDebtProvision * (1 + badDebtProvision / 100);
    }

    public static decimal GetProducerTotalCostWithoutBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
    {
        var hhPackagingWasteTonnage = CalcResultSummaryUtil.GetHouseholdPackagingWasteTonnage(producer, material);
        var reportedPublicBinTonnage = CalcResultSummaryUtil.GetReportedPublicBinTonnage(producer, material);
        var priceperTonne = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult);

        return (hhPackagingWasteTonnage+reportedPublicBinTonnage) * priceperTonne;
    }   

    public static decimal GetBadDebtProvisionForCommsCost(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
    {
        var badDebtProvision = Convert.ToDecimal(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Trim('%'));
        var producerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(producer, material, calcResult);
        return producerTotalCostWithoutBadDebtProvision * badDebtProvision / 100;
    }

    public static decimal GetProducerTotalCostwithBadDebtProvisionTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material, CalcResult calcResult)
    {
        decimal totalCost = 0;

        foreach (var producer in producers)
        {
            totalCost += GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
        }

        return totalCost;
    }


    public static decimal GetTotalReportedTonnage(ProducerDetail producer, MaterialDetail material)
    {
        decimal hdcTonnage = 0;
        var hhPackagingWasteTonnage = CalcResultSummaryUtil.GetHouseholdPackagingWasteTonnage(producer, material);
        var reportedPublicBinTonnage = CalcResultSummaryUtil.GetReportedPublicBinTonnage(producer, material);
        if (material.Code == "GL") hdcTonnage = CalcResultSummaryUtil.GetHDCGlassTonnage(producer, material);

        return material.Code == "GL" ? hdcTonnage + reportedPublicBinTonnage +hhPackagingWasteTonnage : hhPackagingWasteTonnage + reportedPublicBinTonnage;
    }

    public static decimal GetTotalReportedTonnageTotal(IEnumerable<ProducerDetail> producers, MaterialDetail material)
    {
        decimal totalCost = 0;
        foreach (var producer in producers)
        {
            totalCost += GetTotalReportedTonnage(producer, material);
        }
        return totalCost;
    }

}