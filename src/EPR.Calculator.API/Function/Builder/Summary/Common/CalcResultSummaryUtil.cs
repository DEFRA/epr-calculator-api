using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions;
using EPR.Calculator.Service.Function.Builder.Summary.LaDataPrepCosts;
using EPR.Calculator.Service.Function.Builder.Summary.OnePlus2A2B2C;
using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
using EPR.Calculator.Service.Function.Builder.Summary.ThreeSa;
using EPR.Calculator.Service.Function.Builder.Summary.TotalBillBreakdown;
using EPR.Calculator.Service.Function.Builder.Summary.TwoCCommsCost;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Builder.Summary.Common
{
    [ExcludeFromCodeCoverage]
    public static class CalcResultSummaryUtil
    {
        private const int decimalRoundUp = 2;

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

        public static void SetHeaders(CalcResultSummary result, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
        {
            result.ResultSummaryHeader = new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CalculationResult, ColumnIndex = 1 };
            result.NotesHeader         = new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.Notes, ColumnIndex = 1 };

            int section1MaterialsIdx           = 1                             + StartingHeaders().Count();
            int section1DisposalFeeIdx         = section1MaterialsIdx          + Section1Materials(materials, applyModulation).Count();
            int section2aMaterialsIdx          = section1DisposalFeeIdx        + Section1DisposalFee().Count();
            int section2aCommsIdx              = section2aMaterialsIdx         + Section2aMaterials(materials).Count();
            int section1DisposalIdx            = section2aCommsIdx             + Section1Disposal().Count();
            int section2aComms2aIdx            = section1DisposalIdx           + Section2aComms().Count();
            int commsCost2aPercentageIdx       = section2aComms2aIdx           + CommsCost2aPercentage().Count();
            int commsCost2bIdx                 = commsCost2aPercentageIdx      + CommsCost2b().Count();
            int commsCost2cIdx                 = commsCost2bIdx                + CommsCost2c().Count();
            int onePlus2A2B2CProducerIdx       = commsCost2cIdx                + CommsCost2c().Count();
            int threeSaCostsSummaryIdx         = onePlus2A2B2CProducerIdx      + OnePlus2A2B2CProducer.GetHeaders().Count;
            int laDataPrepCostsProducerIdx     = threeSaCostsSummaryIdx        + ThreeSaCostsProducer.GetHeaders().Count;
            int saSetupCostsSummaryIdx         = laDataPrepCostsProducerIdx    + LaDataPrepCostsProducer.GetHeaders().Count;
            int totalBillBreakdownProducerIdx  = saSetupCostsSummaryIdx        + SaSetupCostsProducer.GetHeaders().Count;
            int billingInstructionsProducerIdx = totalBillBreakdownProducerIdx + TotalBillBreakdownProducer.GetHeaders().Count;

            var resultSummaryHeaders = new List<CalcResultSummaryHeader>();
            resultSummaryHeaders.AddRange([
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.OneProducerDisposalFeesWithBadDebtProvision, ColumnIndex = section1MaterialsIdx },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostHeader, ColumnIndex = section2aMaterialsIdx },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforLADisposalCostswoBadDebtprovision1, ColumnIndex = section1DisposalIdx },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision, ColumnIndex = section1DisposalIdx + 1 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforLADisposalCostswithBadDebtprovision1, ColumnIndex = section1DisposalIdx + 2 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforCommsCostsbyMaterialwoBadDebtprovision2A, ColumnIndex = section2aComms2aIdx },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision, ColumnIndex = section2aComms2aIdx + 1 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforCommsCostsbyMaterialwithBadDebtprovision2A, ColumnIndex = section2aComms2aIdx + 2 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostHeaderWithoutBadDebtFor2bTitle, ColumnIndex = commsCost2bIdx },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostHeaderBadDebtProvisionFor2bTitle, ColumnIndex = commsCost2bIdx + 1 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostHeaderWithBadDebtFor2bTitle,ColumnIndex = commsCost2bIdx + 2 },
                new CalcResultSummaryHeader { Name = TwoCCommsConstantsHeader.TwoCCommsCostByCountryWithout, ColumnIndex = commsCost2cIdx },
                new CalcResultSummaryHeader { Name = TwoCCommsConstantsHeader.TwoCCommsCostBadBebtProvision, ColumnIndex = commsCost2cIdx + 1 },
                new CalcResultSummaryHeader { Name = TwoCCommsConstantsHeader.TwoCCommsCostByCountryWithBadDebt, ColumnIndex = commsCost2cIdx + 2 },
            ]);
            resultSummaryHeaders.AddRange(OnePlus2A2B2CProducer.GetSummaryHeaders(onePlus2A2B2CProducerIdx));
            resultSummaryHeaders.AddRange(ThreeSaCostsProducer.GetSummaryHeaders(threeSaCostsSummaryIdx));
            resultSummaryHeaders.AddRange(LaDataPrepCostsProducer.GetSummaryHeaders(laDataPrepCostsProducerIdx));
            resultSummaryHeaders.AddRange(SaSetupCostsProducer.GetSummaryHeaders(saSetupCostsSummaryIdx));
            resultSummaryHeaders.AddRange(TotalBillBreakdownProducer.GetSummaryHeaders(totalBillBreakdownProducerIdx));
            resultSummaryHeaders.AddRange(BillingInstructionsProducer.GetSummaryHeaders(billingInstructionsProducerIdx));
            result.ProducerDisposalFeesHeaders = resultSummaryHeaders;

            var materialsBreakdownHeaders = new List<CalcResultSummaryHeader>();
            var columnIndex = section1MaterialsIdx;
            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultSummaryHeader { Name = $"{material.Name} Breakdown", ColumnIndex = columnIndex});
                columnIndex += Section1Materials([material], applyModulation).Count();
            }
            materialsBreakdownHeaders.Add(new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.DisposalFeeSummary, ColumnIndex = section1DisposalFeeIdx });

            var commsCostColumnIndex = section2aMaterialsIdx;
            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultSummaryHeader { Name = $"{material.Name} Breakdown", ColumnIndex = commsCostColumnIndex });
                commsCostColumnIndex += Section2aMaterials([material]).Count();
            }
            materialsBreakdownHeaders.Add(new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostSummaryHeader, ColumnIndex = commsCostColumnIndex});
            materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(section1DisposalIdx, result.TotalFeeforLADisposalCostswoBadDebtprovision1, result.BadDebtProvisionFor1, result.TotalFeeforLADisposalCostswithBadDebtprovision1));
            materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(section2aComms2aIdx, result.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A, result.BadDebtProvisionFor2A, result.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A));
            materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(commsCost2bIdx, result.CommsCostHeaderWithoutBadDebtFor2bTitle, result.CommsCostHeaderBadDebtProvisionFor2bTitle, result.CommsCostHeaderWithBadDebtFor2bTitle));
            materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(commsCost2cIdx, result.TwoCCommsCostsByCountryWithoutBadDebtProvision, result.TwoCBadDebtProvision, result.TwoCCommsCostsByCountryWithBadDebtProvision));
            materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(onePlus2A2B2CProducerIdx, result.TotalOnePlus2A2B2CFeeWithBadDebtProvision));
            materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(threeSaCostsSummaryIdx, result.SaOperatingCostsWoTitleSection3, result.BadDebtProvisionTitleSection3, result.SaOperatingCostsWithTitleSection3));
            materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(laDataPrepCostsProducerIdx, result.LaDataPrepCostsTitleSection4, result.LaDataPrepCostsBadDebtProvisionTitleSection4, result.LaDataPrepCostsWithBadDebtProvisionTitleSection4));
            materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(saSetupCostsSummaryIdx, result.SaSetupCostsTitleSection5, result.SaSetupCostsBadDebtProvisionTitleSection5, result.SaSetupCostsWithBadDebtProvisionTitleSection5));
            result.MaterialBreakdownHeaders = materialsBreakdownHeaders;

            var columnHeaders = new List<CalcResultSummaryHeader>();
            columnHeaders.AddRange(StartingHeaders());
            columnHeaders.AddRange(Section1Materials(materials, applyModulation));
            columnHeaders.AddRange(Section1DisposalFee());
            columnHeaders.AddRange(Section2aMaterials(materials));
            columnHeaders.AddRange(Section2aComms());
            columnHeaders.AddRange(Section1Disposal());
            columnHeaders.AddRange(Section2aComms2a());
            columnHeaders.AddRange(CommsCost2aPercentage());
            columnHeaders.AddRange(CommsCost2b());
            columnHeaders.AddRange(CommsCost2c());
            columnHeaders.AddRange(OnePlus2A2B2CProducer.GetHeaders());
            columnHeaders.AddRange(ThreeSaCostsProducer.GetHeaders());
            columnHeaders.AddRange(LaDataPrepCostsProducer.GetHeaders());
            columnHeaders.AddRange(SaSetupCostsProducer.GetHeaders());
            columnHeaders.AddRange(TotalBillBreakdownProducer.GetHeaders());
            columnHeaders.AddRange(BillingInstructionsProducer.GetHeaders());
            result.ColumnHeaders = columnHeaders;
        }

        private static IEnumerable<CalcResultSummaryHeader> CreateMoneyHeaders(int columnIndex, params decimal[] values)
        {
            return values.Select((value, i) => new CalcResultSummaryHeader { Name = $"£{Math.Round(value, decimalRoundUp)}", ColumnIndex = columnIndex + i});
        }

        private static IEnumerable<CalcResultSummaryHeader> CreateHeaders(params string[] names)
        {
            return names.Select((name, i) => new CalcResultSummaryHeader { Name = name});
        }

        private static IEnumerable<CalcResultSummaryHeader> StartingHeaders()
        {
            return CreateHeaders(
                CalcResultSummaryHeaders.ProducerId,
                CalcResultSummaryHeaders.SubsidiaryId,
                CalcResultSummaryHeaders.ProducerOrSubsidiaryName,
                CalcResultSummaryHeaders.TradingName,
                CalcResultSummaryHeaders.Level,
                CalcResultSummaryHeaders.ScaledupTonnages,
                CalcResultSummaryHeaders.PartialCalculation,
                CalcResultSummaryHeaders.StatusCode,
                CalcResultSummaryHeaders.JoinersDate,
                CalcResultSummaryHeaders.LeaversDate);
        }

        private static IEnumerable<CalcResultSummaryHeader> Section1Materials(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
        {
            return materials.SelectMany(material =>
            {
                var headers = CreateHeaders(CalcResultSummaryHeaders.PreviousInvoicedTonnage)
                    .ToList();

                headers.AddRange(CreateHeaders(CalcResultSummaryHeaders.HouseholdPackagingWasteTonnage));
                if (applyModulation)
                {
                    headers.AddRange(CreateHeaders(
                        CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageRed,
                        CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageAmber,
                        CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageGreen,
                        CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageRedMedical,
                        CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageAmberMedical,
                        CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageGreenMedical));
                }

                headers.AddRange(CreateHeaders(CalcResultSummaryHeaders.PublicBinTonnage));
                if (applyModulation)
                {
                    headers.AddRange(CreateHeaders(
                        CalcResultSummaryHeaders.PublicBinTonnageRed,
                        CalcResultSummaryHeaders.PublicBinTonnageAmber,
                        CalcResultSummaryHeaders.PublicBinTonnageGreen,
                        CalcResultSummaryHeaders.PublicBinTonnageRedMedical,
                        CalcResultSummaryHeaders.PublicBinTonnageAmberMedical,
                        CalcResultSummaryHeaders.PublicBinTonnageGreenMedical));
                }

                if (material.Code == MaterialCodes.Glass)
                {
                    headers.AddRange(CreateHeaders(CalcResultSummaryHeaders.HouseholdDrinksContainersTonnage));
                    if (applyModulation)
                    {
                        headers.AddRange(CreateHeaders(
                            CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageRed,
                            CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageAmber,
                            CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageGreen,
                            CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageRedMedical,
                            CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageAmberMedical,
                            CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageGreenMedical));
                    }
                }

                if (applyModulation) {
                    headers.AddRange(CreateHeaders(
                        CalcResultSummaryHeaders.TotalTonnage,
                        CalcResultSummaryHeaders.RedTotalTonnage,
                        CalcResultSummaryHeaders.AmberTotalTonnage,
                        CalcResultSummaryHeaders.GreenTotalTonnage,
                        CalcResultSummaryHeaders.RedMedicalTotalTonnage,
                        CalcResultSummaryHeaders.AmberMedicalTotalTonnage,
                        CalcResultSummaryHeaders.GreenMedicalTotalTonnage,
                        CalcResultSummaryHeaders.RedPlusRedMedicalTotalTonnage,
                        CalcResultSummaryHeaders.AmberPlusAmberMedicalTotalTonnage,
                        CalcResultSummaryHeaders.GreenPlusGreenMedicalTotalTonnage,
                        CalcResultSummaryHeaders.SelfManagedConsumerWasteTonnage,
                        CalcResultSummaryHeaders.ActionedSelfManagedConsumerWasteTonnage,
                        CalcResultSummaryHeaders.RedActionedSelfManagedConsumerWasteTonnage,
                        CalcResultSummaryHeaders.AmberActionedSelfManagedConsumerWasteTonnage,
                        CalcResultSummaryHeaders.GreenActionedSelfManagedConsumerWasteTonnage,
                        CalcResultSummaryHeaders.NetTonnage,
                        CalcResultSummaryHeaders.RedPlusRedMedicalNetTonnage,
                        CalcResultSummaryHeaders.AmberPlusAmberMedicalNetTonnage,
                        CalcResultSummaryHeaders.GreenPlusGreenMedicalNetTonnage,
                        CalcResultSummaryHeaders.ResidualSelfManagedConsumerWasteTonnage
                    ));
                } else {
                    headers.AddRange(CreateHeaders(
                        CalcResultSummaryHeaders.TotalTonnage,
                        CalcResultSummaryHeaders.SelfManagedConsumerWasteTonnage,
                        CalcResultSummaryHeaders.NetTonnage
                    ));
                }

                headers.AddRange(CreateHeaders(CalcResultSummaryHeaders.TonnageChange));
                if (applyModulation) {
                    headers.AddRange(CreateHeaders(
                        CalcResultSummaryHeaders.RedPlusRedMedicalMaterialPricePerTonne,
                        CalcResultSummaryHeaders.AmberPlusAmberMedicalMaterialPricePerTonne,
                        CalcResultSummaryHeaders.GreenPlusGreenMedicalMaterialPricePerTonne,
                        CalcResultSummaryHeaders.ProducerRedPlusRedMedicalMaterialDisposalCost,
                        CalcResultSummaryHeaders.ProducerAmberPlusAmberMedicalMaterialDisposalCost,
                        CalcResultSummaryHeaders.ProducerGreenPlusGreenMedicalMaterialDisposalCost));
                } else {
                    headers.AddRange(CreateHeaders(CalcResultSummaryHeaders.PricePerTonne));
                }

                headers.AddRange(CreateHeaders(
                    CalcResultSummaryHeaders.ProducerDisposalFee,
                    CalcResultSummaryHeaders.BadDebtProvision,
                    CalcResultSummaryHeaders.ProducerDisposalFeeWithBadDebtProvision,
                    CalcResultSummaryHeaders.EnglandWithBadDebtProvision,
                    CalcResultSummaryHeaders.WalesWithBadDebtProvision,
                    CalcResultSummaryHeaders.ScotlandWithBadDebtProvision,
                    CalcResultSummaryHeaders.NorthernIrelandWithBadDebtProvision));

                return headers;
            });
        }

        private static IEnumerable<CalcResultSummaryHeader> Section1DisposalFee()
        {
            return CreateHeaders(
                CalcResultSummaryHeaders.TotalProducerDisposalFee,
                CalcResultSummaryHeaders.BadDebtProvision,
                CalcResultSummaryHeaders.TotalProducerDisposalFeeWithBadDebtProvision,
                CalcResultSummaryHeaders.EnglandTotal,
                CalcResultSummaryHeaders.WalesTotal,
                CalcResultSummaryHeaders.ScotlandTotal,
                CalcResultSummaryHeaders.NorthernIrelandTotal,
                CalcResultSummaryHeaders.TonnageChangeCount,
                CalcResultSummaryHeaders.TonnageChangeAdvice);
        }

        private static IEnumerable<CalcResultSummaryHeader> Section2aMaterials(IReadOnlyList<MaterialDetail> materials)
        {
            return materials.SelectMany(material =>
            {
                var headers = new List<CalcResultSummaryHeader>();
                headers.AddRange(CreateHeaders(
                    CalcResultSummaryHeaders.HouseholdPackagingWasteTonnage,
                    CalcResultSummaryHeaders.PublicBinTonnage));

                if (material.Code == MaterialCodes.Glass)
                {
                    headers.AddRange(CreateHeaders(CalcResultSummaryHeaders.HouseholdDrinksContainersTonnage));
                }

                headers.AddRange(CreateHeaders(
                    CalcResultSummaryHeaders.TotalTonnage,
                    CalcResultSummaryHeaders.PricePerTonne,
                    CalcResultSummaryHeaders.ProducerTotalCostWithoutBadDebtProvision,
                    CalcResultSummaryHeaders.BadDebtProvision,
                    CalcResultSummaryHeaders.ProducerTotalCostwithBadDebtProvision,
                    CalcResultSummaryHeaders.EnglandWithBadDebtProvision,
                    CalcResultSummaryHeaders.WalesWithBadDebtProvision,
                    CalcResultSummaryHeaders.ScotlandWithBadDebtProvision,
                    CalcResultSummaryHeaders.NorthernIrelandWithBadDebtProvision
                ));

                return headers;
            });
        }

        private static IEnumerable<CalcResultSummaryHeader> Section2aComms()
        {
            return CreateHeaders(
                CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision,
                CalcResultSummaryHeaders.TotalBadDebtProvision,
                CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision,
                CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision,
                CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision,
                CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision,
                CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision);
        }

        private static IEnumerable<CalcResultSummaryHeader> Section1Disposal()
        {
            return CreateHeaders(
                CalcResultSummaryHeaders.TotalProducerFeeforLADisposalCostswoBadDebtprovision,
                CalcResultSummaryHeaders.BadDebtProvisionFor1,
                CalcResultSummaryHeaders.TotalProducerFeeforLADisposalCostswithBadDebtprovision,
                CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision,
                CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision,
                CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision,
                CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision);
        }

        private static IEnumerable<CalcResultSummaryHeader> Section2aComms2a()
        {
            return CreateHeaders(
                CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision2A,
                CalcResultSummaryHeaders.BadDebtProvisionfor2A,
                CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision2A,
                CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision,
                CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision,
                CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision,
                CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision);
        }

        private static IEnumerable<CalcResultSummaryHeader> CommsCost2aPercentage()
        {
            return CreateHeaders(CalcResultSummaryHeaders.PercentageofProducerTonnagevsAllProducers);
        }

        private static IEnumerable<CalcResultSummaryHeader> CommsCost2b()
        {
            return CreateHeaders(
                CalcResultSummaryHeaders.ProducerFeeWithoutBadDebtForComms2b,
                CalcResultSummaryHeaders.BadDebtProvisionForComms2b,
                CalcResultSummaryHeaders.ProducerFeeForCommsCostsWithBadDebtForComms2b,
                CalcResultSummaryHeaders.EnglandTotalWithBadDebtProvisionForComms2b,
                CalcResultSummaryHeaders.WalesTotalWithBadDebtProvisionForComms2b,
                CalcResultSummaryHeaders.ScotlandTotalWithBadDebtProvisionForComms2b,
                CalcResultSummaryHeaders.NorthernIrelandTotalWithBadDebtProvisionForComms2b);
        }

        private static IEnumerable<CalcResultSummaryHeader> CommsCost2c()
        {
            return CreateHeaders(
                TwoCCommsCostSubColumnHeader.TwoCCommsCostCountryInPropertionWithoutBadDebt,
                TwoCCommsCostSubColumnHeader.TwoCCommsCostBadDebtProvision,
                TwoCCommsCostSubColumnHeader.TwoCCommsCostCountryInPropertionWithBadDebt,
                TwoCCommsCostSubColumnHeader.TwoCCommsCostEnglandWithBadDebt,
                TwoCCommsCostSubColumnHeader.TwoCCommsCostWalesWithBadDebt,
                TwoCCommsCostSubColumnHeader.TwoCCommsCostScotlandWithBadDebt,
                TwoCCommsCostSubColumnHeader.TwoCCommsCostNIWithBadDebt);
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
