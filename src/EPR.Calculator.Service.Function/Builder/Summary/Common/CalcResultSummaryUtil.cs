using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions;
using EPR.Calculator.Service.Function.Builder.Summary.LaDataPrepCosts;
using EPR.Calculator.Service.Function.Builder.Summary.OnePlus2A2B2C;
using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
using EPR.Calculator.Service.Function.Builder.Summary.ThreeSa;
using EPR.Calculator.Service.Function.Builder.Summary.TotalBillBreakdown;
using EPR.Calculator.Service.Function.Builder.Summary.TwoCCommsCost;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Builder.Summary.Common
{
    [ExcludeFromCodeCoverage]
    public static class CalcResultSummaryUtil
    {
        public static int GetLevelIndex(
            IReadOnlyList<CalcResultSummaryProducerDisposalFees> producerDisposalFeesLookup,
            ProducerDetail producer)
        {
            var totalRow = producerDisposalFeesLookup.FirstOrDefault(pdf => pdf.ProducerId == producer.ProducerId.ToString() && pdf.isTotalRow);

            return totalRow == null ? (int)CalcResultSummaryLevelIndex.One : (int)CalcResultSummaryLevelIndex.Two;
        }

        public static bool IsProducerScaledup(
            ProducerDetail producer,
            IReadOnlyList<CalcResultScaledupProducer> scaledupProducers)
        {
            var scaledupProducer = scaledupProducers.FirstOrDefault(p => p.ProducerId == producer.ProducerId);
            return scaledupProducer != null;
        }

        public static bool IsProducerPartiallyObligated(
            ProducerDetail producer,
            IReadOnlyList<CalcResultPartialObligation> partialObligations,
            bool isTotalRow)
        {
            var partialObligation = isTotalRow ? partialObligations.FirstOrDefault(p => p.ProducerId == producer.ProducerId) : partialObligations.FirstOrDefault(p => p.ProducerId == producer.ProducerId && p.SubsidiaryId == producer.SubsidiaryId);
            return partialObligation != null;
        }

        public static decimal GetTonnage(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            string packagingType,
            RagRating? ragRating = null)
        {
            var prms = projectedMaterialsLookup[(producer.ProducerId, producer.SubsidiaryId)]
                .Where(p => p.MaterialId == material.Id && p.PackagingType == packagingType);

            return ragRating switch
            {
                null                   => prms.Sum(p => p.PackagingTonnage),
                RagRating.Red          => prms.Sum(p => p.PackagingTonnageRed ?? 0),
                RagRating.Amber        => prms.Sum(p => p.PackagingTonnageAmber ?? 0),
                RagRating.Green        => prms.Sum(p => p.PackagingTonnageGreen ?? 0),
                RagRating.RedMedical   => prms.Sum(p => p.PackagingTonnageRedMedical ?? 0),
                RagRating.AmberMedical => prms.Sum(p => p.PackagingTonnageAmberMedical ?? 0),
                RagRating.GreenMedical => prms.Sum(p => p.PackagingTonnageGreenMedical ?? 0),
                _                      => 0m
            };
        }

        public static decimal GetTonnageTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material,
            string packagingType,
            RagRating? ragRating = null)
        {
            return producers.Sum(producer => GetTonnage(projectedMaterialsLookup, producer, material, packagingType, ragRating));
        }

        public static decimal GetReportedTonnage(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            RagRating? ragRating = null)
        {
            var householdTonnage = GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, ragRating);
            var publicBinTonnage = GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, ragRating);
            var glassTonnage = material.Code == MaterialCodes.Glass
                ? GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, ragRating)
                : 0;

            return householdTonnage + publicBinTonnage + glassTonnage;
        }

        public static decimal GetReportedTonnageTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material,
            RagRating? ragRating = null)
        {
            return producers.Sum(producer => GetReportedTonnage(projectedMaterialsLookup, producer, material, ragRating));
        }

        public static SelfManagedConsumerWasteData SumSelfManagedConsumerWasteData(
            IReadOnlyList<ProducerDetail> producersAndSubsidiaries,
            MaterialDetail material,
            bool isOverAllTotalRow,
            SelfManagedConsumerWaste smcw)
        {
            return isOverAllTotalRow
                ? smcw.OverallTotalPerMaterials.GetValueOrDefault(material.Code) ?? SelfManagedConsumerWasteData.Zero
                : smcw.ProducerTotals
                    .Where(x => x.Level == 1 && producersAndSubsidiaries.Any(y => x.ProducerId == y.ProducerId))
                    .Select(x => x.SelfManagedConsumerWasteDataPerMaterials[material.Code])
                    .Single();
        }

        public static decimal? GetPreviousInvoicedTonnage(
            IReadOnlyList<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            IReadOnlyList<ProducerDetail> producersAndSubsidiaries,
            IReadOnlyList<CalcResultScaledupProducer> scaledUpProducers,
            IReadOnlyList<CalcResultPartialObligation> partialObligations,
            MaterialDetail material,
            bool isOverAllTotalRow,
            decimal? previousInvoicedNetTonnage)
        {
            return isOverAllTotalRow
                ? producerDisposalFees
                    .Where(fee => fee.Level == CommonConstants.LevelOne.ToString())
                    .Sum(row => row.ProducerDisposalFeesByMaterial?[material.Code].PreviousInvoicedTonnage)
                : previousInvoicedNetTonnage;
        }

        public static (decimal? total, decimal? red,  decimal? amber, decimal? green) GetPricePerTonne(
            MaterialDetail material,
            CalcResult calcResult)
        {
            var laDisposalCostDataDetail = calcResult.CalcResultLaDisposalCostData.ByMaterial.GetValueOrDefault(material.Code);

            if (laDisposalCostDataDetail == null)
            {
                return (total: null, red: null, amber: null, green: null);
            }

            var total = laDisposalCostDataDetail.DisposalCostPricePerTonne ?? 0m;

            if (calcResult.CalcResultModulation is not null) {
                return (
                    total: total,
                    red:   calcResult.CalcResultModulation.MaterialModulation[material].RedMaterialDisposalCost,
                    amber: calcResult.CalcResultModulation.MaterialModulation[material].AmberMaterialDisposalCost,
                    green: calcResult.CalcResultModulation.MaterialModulation[material].GreenMaterialDisposalCost
                );
            } else {
                return (total: total, red: null, amber: null, green: null);
            }
        }

        public static (decimal? total, decimal? red,  decimal? amber, decimal? green) GetProducerDisposalFee(
            MaterialDetail material,
            CalcResult calcResult,
            SelfManagedConsumerWasteData smcw)
        {
            var pricePerTonne = GetPricePerTonne(material, calcResult);

            if (calcResult.CalcResultModulation is not null) {
                var red   = smcw.NetReportedTonnage.red   * pricePerTonne.red;
                var amber = smcw.NetReportedTonnage.amber * pricePerTonne.amber;
                var green = smcw.NetReportedTonnage.green * pricePerTonne.green;
                return (
                    total: red + amber + green,
                    red:   red,
                    amber: amber,
                    green: green
                );
            } else {
                var total = (smcw.NetReportedTonnage.total ?? 0) * (pricePerTonne.total ?? 0);
                return (total: total, red: null, amber: null, green: null);
            }
        }

        public static decimal GetBadDebtProvision(
            CalcResult calcResult,
            decimal? producerDisposalFeeTotal)
        {
            return (producerDisposalFeeTotal ?? 0) * calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
        }

        public static decimal GetProducerDisposalFeeWithBadDebtProvision(
            CalcResult calcResult,
            decimal? producerDisposalFeeTotal)
        {
            return (producerDisposalFeeTotal ?? 0) * (1 + (calcResult.CalcResultParameterOtherCost.BadDebtValue / 100));
        }

        public static decimal GetCountryBadDebtProvision(
            CalcResult calcResult,
            Countries country,
            decimal? producerDisposalFeeTotal)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(calcResult, producerDisposalFeeTotal);

            var countryApportionment = calcResult.CalcResultLapcapData.CountryApportionment;

            var disposalCostPercentage = country switch
            {
                Countries.England         => countryApportionment.England,
                Countries.Wales           => countryApportionment.Wales,
                Countries.Scotland        => countryApportionment.Scotland,
                Countries.NorthernIreland => countryApportionment.NorthernIreland,
                _                         => throw new ArgumentOutOfRangeException(nameof(country), country, null),
            };

            return producerDisposalFeeWithBadDebtProvision * disposalCostPercentage / 100;
        }

        public static decimal GetCommsCostHeaderWithoutBadDebtFor2bTitle(CalcResult calcResult)
        {
            return calcResult.CalcResultCommsCostReportDetail.CommsCostUkWide.Total;
        }

        public static decimal GetCommsCostHeaderBadDebtProvisionFor2bTitle(
            CalcResult calcResult,
            CalcResultSummary calcResultSummary)
        {
            var commsCost = calcResultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle;
            var badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
            return commsCost * badDebtProvision;
        }

        public static decimal GetCommsCostHeaderWithBadDebtFor2bTitle(CalcResultSummary calcResultSummary)
        {
            var commsCostHeaderWithoutBadDebt = calcResultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle;
            var commsCostHeaderBadDebtProvision = calcResultSummary.CommsCostHeaderBadDebtProvisionFor2bTitle;
            return commsCostHeaderWithoutBadDebt + commsCostHeaderBadDebtProvision;
        }

        public static decimal GetCountryOnePlusFourApportionment(
            CalcResult calcResult,
            Countries country)
        {
            var onePlusFourApportionment = calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment;
            switch (country)
            {
                case Countries.England:
                    return onePlusFourApportionment.England;
                case Countries.Wales:
                    return onePlusFourApportionment.Wales;
                case Countries.Scotland:
                    return onePlusFourApportionment.Scotland;
                case Countries.NorthernIreland:
                    return onePlusFourApportionment.NorthernIreland;
                default:
                    return 0;
            }
        }

        public static decimal GetParamsOtherFourCountryApportionmentPercentage(
            CalcResult calcResult,
            Countries country)
        {
            var fourCountryApportionment = calcResult.CalcResultParameterOtherCost.CountryApportionment;

            if (fourCountryApportionment == null)
            {
                return 0;
            }

            switch (country)
            {
                case Countries.England:
                    return fourCountryApportionment.England;
                case Countries.Wales:
                    return fourCountryApportionment.Wales;
                case Countries.Scotland:
                    return fourCountryApportionment.Scotland;
                case Countries.NorthernIreland:
                    return fourCountryApportionment.NorthernIreland;
                default:
                    return 0;
            }
        }
    }
}
