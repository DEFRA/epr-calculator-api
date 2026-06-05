using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoA
{
    public static class CalcResultSummaryCommsCostTwoA
    {
        public static decimal GetEnglandWithBadDebtProvisionForCommsTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer =>
                GetEnglandWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult)
            );
        }

        public static decimal GetWalesWithBadDebtProvisionForCommsTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetWalesWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult));
        }

        public static decimal GetScotlandWithBadDebtProvisionForCommsTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetScotlandWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult));
        }

        public static decimal GetNorthernIrelandWithBadDebtProvisionForCommsTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetNorthernIrelandWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult));
        }

        public static decimal GetProducerTotalCostWithoutBadDebtProvisionTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetProducerTotalCostWithoutBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult));
        }

        public static decimal GetBadDebtProvisionForCommsCostTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetBadDebtProvisionForCommsCost(projectedMaterialsLookup, producer, material, calcResult));
        }

        public static decimal GetEnglandWithBadDebtProvisionForComms(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment.England / 100;
        }

        public static decimal GetWalesWithBadDebtProvisionForComms(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment.Wales / 100;
        }

        public static decimal GetScotlandWithBadDebtProvisionForComms(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment.Scotland / 100;
        }

        public static decimal GetNorthernIrelandWithBadDebtProvisionForComms(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment.NorthernIreland / 100;
        }

        public static decimal GetPriceperTonneForComms(MaterialDetail material, CalcResult calcResult)
        {
            var commsCostDataDetail = calcResult.CalcResultCommsCostReportDetail.ByMaterial.GetValueOrDefault(material.Code);
            return commsCostDataDetail?.PricePerTonne ?? 0m;
        }

        public static decimal GetProducerTotalCostwithBadDebtProvision(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue;
            var producerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult);
            return producerTotalCostWithoutBadDebtProvision * (1 + badDebtProvision / 100);
        }

        public static decimal GetTotalReportedTonnage(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material
        )
        {
            decimal hdcTonnage = 0;
            var hhPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household);
            var reportedPublicBinTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin);

            if (material.Code == MaterialCodes.Glass)
            {
                hdcTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers);
            }

            return material.Code == MaterialCodes.Glass
                ? hdcTonnage + reportedPublicBinTonnage + hhPackagingWasteTonnage
                : hhPackagingWasteTonnage + reportedPublicBinTonnage;
        }

        public static decimal GetProducerTotalCostWithoutBadDebtProvision(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var priceperTonne = GetPriceperTonneForComms(material, calcResult);
            return GetTotalReportedTonnage(projectedMaterialsLookup, producer, material) * priceperTonne;
        }

        public static decimal GetBadDebtProvisionForCommsCost(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue;
            var producerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult);
            return producerTotalCostWithoutBadDebtProvision * badDebtProvision / 100;
        }

        public static decimal GetProducerTotalCostwithBadDebtProvisionTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetProducerTotalCostwithBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult));
        }

        public static decimal GetTotalReportedTonnageTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material
        )
        {
            return producers.Sum(producer => GetTotalReportedTonnage(projectedMaterialsLookup, producer, material));
        }
    }
}
