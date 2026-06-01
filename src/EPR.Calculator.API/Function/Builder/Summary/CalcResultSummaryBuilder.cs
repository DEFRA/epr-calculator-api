using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoA;
using EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoBTotalBill;
using EPR.Calculator.Service.Function.Builder.Summary.LaDataPrepCosts;
using EPR.Calculator.Service.Function.Builder.Summary.OneAndTwoA;
using EPR.Calculator.Service.Function.Builder.Summary.OnePlus2A2B2C;
using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
using EPR.Calculator.Service.Function.Builder.Summary.ThreeSa;
using EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer;
using EPR.Calculator.Service.Function.Builder.Summary.TotalBillBreakdown;
using EPR.Calculator.Service.Function.Builder.Summary.TwoCCommsCost;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.Summary
{
    public interface ICalcResultSummaryBuilder
    {
        Task<CalcResultSummary> ConstructAsync(
            IImmutableList<MaterialDetail> materialDetails,
            int runId,
            RelativeYear relativeYear,
            bool isBillingFile,
            CalcResult calcResult,
            SelfManagedConsumerWaste smcw
        );
    }

    [ExcludeFromCodeCoverage]
    public class CalcResultSummaryBuilder(
        ApplicationDBContext context,
        IInvoicedProducerService invoicedProducerService)
        : ICalcResultSummaryBuilder
    {
        public ImmutableList<CalcResultScaledupProducer> ScaledupProducers { get; set; } = [];

        public ImmutableList<CalcResultPartialObligation> PartialObligations { get; set; } = [];

        public ImmutableList<Organisation> Organisations { get; set; } = [];

        public ImmutableList<Organisation> ParentOrganisations { get; set; } = [];

        // PERF: Pre-computed dictionaries to replace O(N) linear scans inside the per-producer/per-material loops.
        private ImmutableDictionary<(int ProducerId, int MaterialId), decimal?> invoicedNetTonnageByProducerMaterial = ImmutableDictionary.Create<(int ProducerId, int MaterialId), decimal?>();
        private ImmutableDictionary<(int OrganisationId, string? SubsidiaryId), Organisation> organisationsByKey = ImmutableDictionary.Create<(int OrganisationId, string? SubsidiaryId), Organisation>();
        private ImmutableDictionary<int, Organisation> parentOrganisationsById = ImmutableDictionary.Create<int, Organisation>();

        public async Task<CalcResultSummary> ConstructAsync(
            IImmutableList<MaterialDetail> materialDetails,
            int runId,
            RelativeYear relativeYear,
            bool isBillingFile,
            CalcResult calcResult,
            SelfManagedConsumerWaste smcw
        )
        {
            ScaledupProducers = calcResult.CalcResultScaledupProducers.ScaledupProducers ?? [];
            PartialObligations = calcResult.CalcResultPartialObligations.PartialObligations ?? [];

            var runProducerMaterialDetails = await (
                from pd in context.ProducerDetail
                join prm in context.ProducerReportedMaterialProjected on pd.Id equals prm.ProducerDetailId
                where pd.CalculatorRunId == runId
                select new CalcResultProducerAndReportMaterialDetail
                {
                    ProducerDetail = pd,
                    ProducerReportedMaterialProjected = prm,
                }
            ).ToListAsync();

            var projectedMaterialsLookup = runProducerMaterialDetails
                .ToLookup(
                    x => (x.ProducerDetail.ProducerId, x.ProducerDetail.SubsidiaryId),
                    x => x.ProducerReportedMaterialProjected
                );

            var producerDetails = runProducerMaterialDetails
                .Select(x => x.ProducerDetail)
                .DistinctBy(x => (x.ProducerId, x.SubsidiaryId))
                .OrderBy(pd => pd.ProducerId)
                .ThenBy(pd => pd.SubsidiaryId)
                .ToImmutableList();

            var producerInvoicedMaterialNetTonnage = await invoicedProducerService.GetLatestAcceptedInvoicedProducers(relativeYear);

            // PREF: Replace per-(producer, material) linear scans of the invoiced records collection with an O(1) lookup.
            invoicedNetTonnageByProducerMaterial = BuildInvoicedNetTonnageByProducerMaterial(producerInvoicedMaterialNetTonnage);

            var defaultParams = await GetDefaultParamsAsync(runId);

            // Household + PublicBin + HDC.
            // PERF: wrap in an index so downstream callers (TonnageVsAllProducerUtil / 2B / 2C) get O(1)
            // per-producer percentage lookups instead of paying O(producers) per call.
            var totalPackagingTonnage = new TotalPackagingTonnageIndex(GetTotalPackagingTonnagePerRun(runProducerMaterialDetails, materialDetails, runId));

            // Get organisations
            Organisations = await (
                from run in context.CalculatorRuns
                join crodm in context.CalculatorRunOrganisationDataMaster on run.CalculatorRunOrganisationDataMasterId equals crodm.Id
                join crodd in context.CalculatorRunOrganisationDataDetails on crodm.Id equals crodd.CalculatorRunOrganisationDataMasterId
                where run.Id == runId && crodd.ObligationStatus == ObligationStates.Obligated
                select new Organisation
                {
                    OrganisationId = crodd.OrganisationId,
                    SubsidiaryId = crodd.SubsidiaryId,
                    OrganisationName = crodd.OrganisationName,
                    TradingName = crodd.TradingName,
                    StatusCode = crodd.StatusCode,
                    JoinerDate = crodd.JoinerDate,
                    LeaverDate = crodd.LeaverDate
                })
                .Distinct()
                .ToImmutableListAsync();

            ParentOrganisations = Organisations.Where(o => o.SubsidiaryId == null).ToImmutableList();

            // PERF: Replace per-row FirstOrDefault scans with O(1) dictionary lookups.
            organisationsByKey = BuildOrganisationsByKey(Organisations);
            parentOrganisationsById = BuildParentOrganisationsById(ParentOrganisations);

            var result = GetCalcResultSummary(
                projectedMaterialsLookup,
                producerDetails,
                materialDetails,
                calcResult,
                totalPackagingTonnage,
                producerInvoicedMaterialNetTonnage,
                defaultParams,
                smcw
            );

            if (isBillingFile)
            {
                await UpdateBillingInstructions(calcResult, result);
            }

            return result;
        }

        private static ImmutableDictionary<(int, int), decimal?> BuildInvoicedNetTonnageByProducerMaterial(
            IReadOnlyList<InvoicedProducer> invoicedProducers)
        {
            var builder = ImmutableDictionary.CreateBuilder<(int, int), decimal?>();
            foreach (var invoicedProducer in invoicedProducers)
            {
                // Preserve FirstOrDefault semantics (the previous LINQ kept only the first matching record).
                builder.TryAdd((invoicedProducer.ProducerId, invoicedProducer.MaterialId), invoicedProducer.InvoicedNetTonnage);
            }
            return builder.ToImmutable();
        }

        private static ImmutableDictionary<(int, string?), Organisation> BuildOrganisationsByKey(
            IReadOnlyList<Organisation> organisations)
        {
            var builder = ImmutableDictionary.CreateBuilder<(int, string?), Organisation>();
            foreach (var org in organisations)
            {
                builder.TryAdd((org.OrganisationId, org.SubsidiaryId), org);
            }
            return builder.ToImmutable();
        }

        private static ImmutableDictionary<int, Organisation> BuildParentOrganisationsById(
            IReadOnlyList<Organisation> parents)
        {
            var builder = ImmutableDictionary.CreateBuilder<int, Organisation>();
            foreach (var org in parents)
            {
                builder.TryAdd(org.OrganisationId, org);
            }
            return builder.ToImmutable();
        }

        private Task<List<DefaultParamResultsClass>> GetDefaultParamsAsync(int runId)
        {
            return (
                from run in context.CalculatorRuns.AsNoTracking()
                join defaultMaster in context.DefaultParameterSettings.AsNoTracking() on run.DefaultParameterSettingMasterId equals defaultMaster.Id
                join defaultDetail in context.DefaultParameterSettingDetail.AsNoTracking() on defaultMaster.Id equals defaultDetail.DefaultParameterSettingMasterId
                join defaultTemplate in context.DefaultParameterTemplateMasterList.AsNoTracking() on defaultDetail.ParameterUniqueReferenceId equals defaultTemplate.ParameterUniqueReferenceId
                where run.Id == runId
                select new DefaultParamResultsClass
                {
                    ParameterValue = defaultDetail.ParameterValue,
                    ParameterCategory = defaultTemplate.ParameterCategory,
                    ParameterType = defaultTemplate.ParameterType,
                    ParameterUniqueReference = defaultDetail.ParameterUniqueReferenceId
                }
            ).ToListAsync();
        }

        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
        public CalcResultSummary GetCalcResultSummary(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> orderedProducerDetails,
            IReadOnlyList<MaterialDetail> materials,
            CalcResult calcResult,
            IReadOnlyList<TotalPackagingTonnagePerRun> totalPackagingTonnage,
            IReadOnlyList<InvoicedProducer> producerInvoicedMaterialNetTonnage,
            IReadOnlyList<DefaultParamResultsClass> defaultParams,
            SelfManagedConsumerWaste smcw
        )
        {
            var result = new CalcResultSummary();

            if (orderedProducerDetails.Count > 0)
            {
                var producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>();

                foreach (var producerAndSubsidiaries in orderedProducerDetails.GroupBy(x => x.ProducerId))
                {
                    // PERF: Reuse the same materialised list across all three callers below; previous
                    // implementation invoked `.ToList()` three times per group.
                    var subsidiariesList = producerAndSubsidiaries.ToList();

                    if (!(subsidiariesList.Count == 1 && subsidiariesList[0].SubsidiaryId == null))
                        producerDisposalFees.Add(GetProducerTotalRow(projectedMaterialsLookup, subsidiariesList, materials, calcResult, producerDisposalFees, false, totalPackagingTonnage, smcw));

                    foreach (var producer in subsidiariesList)
                        producerDisposalFees.Add(GetProducerRow(projectedMaterialsLookup, producerDisposalFees, subsidiariesList, producer, materials, calcResult, totalPackagingTonnage, smcw));
                };

                // Calculate the total for all the producers
                var allTotalRow = GetProducerTotalRow(projectedMaterialsLookup, orderedProducerDetails, materials, calcResult, producerDisposalFees, true, totalPackagingTonnage, smcw);
                producerDisposalFees.Add(allTotalRow);

                result.ProducerDisposalFees = producerDisposalFees;

                // Section-(1) & (2a)
                result.TotalFeeforLADisposalCostswoBadDebtprovision1 = CalcResultOneAndTwoAUtil.GetTotalDisposalCostswoBadDebtprovision1(producerDisposalFees);
                result.BadDebtProvisionFor1 = CalcResultOneAndTwoAUtil.GetTotalBadDebtprovision1(producerDisposalFees);
                result.TotalFeeforLADisposalCostswithBadDebtprovision1 = CalcResultOneAndTwoAUtil.GetTotalDisposalCostswithBadDebtprovision1(producerDisposalFees);

                result.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A = CalcResultOneAndTwoAUtil.GetTotalCommsCostswoBadDebtprovision2A(producerDisposalFees);
                result.BadDebtProvisionFor2A = CalcResultOneAndTwoAUtil.GetTotalBadDebtprovision2A(producerDisposalFees);
                result.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = CalcResultOneAndTwoAUtil.GetTotalCommsCostswithBadDebtprovision2A(producerDisposalFees);

                // 2b comms total
                result.CommsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);
                result.CommsCostHeaderBadDebtProvisionFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderBadDebtProvisionFor2bTitle(calcResult, result);
                result.CommsCostHeaderWithBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithBadDebtFor2bTitle(result);

                TwoCCommsCostUtil.UpdateHeaderTotal(calcResult, result);

                // Section Total bill (1 + 2a + 2b + 2c)
                OnePlus2A2B2CProducer.SetValues(result);

                // Section-3 SA Operating costs section
                ThreeSaCostsProducer.GetProducerSetUpCostsSection3(calcResult, result);

                // Section-4 LA data prep costs
                LaDataPrepCostsProducer.SetValues(calcResult, result);

                // Section-5 SA setup costs
                SaSetupCostsProducer.GetProducerSetUpCosts(calcResult, result);

                // Total bill section
                TotalBillBreakdownProducer.SetValues(result);

                // Billing instructions section
                BillingInstructionsProducer.SetValues(result, producerInvoicedMaterialNetTonnage, defaultParams);

            }

            // Set headers with calculated column index
            CalcResultSummaryUtil.SetHeaders(result, materials, calcResult.ApplyModulation);

            return result;
        }

        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
        [SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "This is suppressed for now and will be refactored later.")]
        public CalcResultSummaryProducerDisposalFees GetProducerTotalRow(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producersAndSubsidiaries,
            IReadOnlyList<MaterialDetail> materials,
            CalcResult calcResult,
            IReadOnlyList<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            bool isOverAllTotalRow,
            IReadOnlyList<TotalPackagingTonnagePerRun> totalPackagingTonnage,
            SelfManagedConsumerWaste smcw
        )
        {
            var materialCosts = GetMaterialCosts(projectedMaterialsLookup, producersAndSubsidiaries, producerDisposalFees, materials, calcResult, isOverAllTotalRow, smcw);
            var communicationCosts = GetCommunicationCosts(projectedMaterialsLookup, producersAndSubsidiaries, materials, calcResult);

            // Compute Count/Advice for the producer-total (Level 1) row
            string? tonnageChangeCount = null;
            string? tonnageChangeAdvice = null;
            string isProducerScaledUp = string.Empty;
            string isPartialObligation = string.Empty;

            if (!isOverAllTotalRow)
            {
                (tonnageChangeCount, tonnageChangeAdvice) =
                    TonnageChangeUtil.ComputeCountAndAdvice(CommonConstants.LevelOne.ToString(), materialCosts);
                isProducerScaledUp = CalcResultSummaryUtil.IsProducerScaledup(producersAndSubsidiaries[0], ScaledupProducers) ? CommonConstants.Yes : CommonConstants.No;
                isPartialObligation = CalcResultSummaryUtil.IsProducerPartiallyObligated(producersAndSubsidiaries[0], PartialObligations, isTotalRow: true) ? CommonConstants.Yes : CommonConstants.No;
            }

            var producerForTotalRow = GetProducerDetailsForTotalRow(producersAndSubsidiaries[0].ProducerId, isOverAllTotalRow);
            const int overallTotalId = 0;

            var totalRow = new CalcResultSummaryProducerDisposalFees
            {
                ProducerIdInt = isOverAllTotalRow ? overallTotalId : producersAndSubsidiaries[0].ProducerId,
                ProducerId = isOverAllTotalRow ? string.Empty : producersAndSubsidiaries[0].ProducerId.ToString(),
                ProducerName = producerForTotalRow?.OrganisationName ?? string.Empty,
                SubsidiaryId = string.Empty,
                TradingName = producerForTotalRow?.TradingName ?? string.Empty,
                Level = isOverAllTotalRow ? string.Empty : CommonConstants.LevelOne.ToString(),
                IsProducerScaledup = isProducerScaledUp,
                IsPartialObligation = isPartialObligation,
                StatusCode = producerForTotalRow?.StatusCode,
                JoinerDate = producerForTotalRow?.JoinerDate,
                LeaverDate = isOverAllTotalRow ? CommonConstants.Totals : producerForTotalRow?.LeaverDate,
                ProducerDisposalFeesByMaterial = materialCosts,

                // Disposal fee summary
                TotalProducerDisposalFee = materialCosts.Sum(m => m.Value.ProducerDisposalFee.total ?? 0),
                BadDebtProvision = materialCosts.Values.Sum(m => m.BadDebtProvision),
                TotalProducerDisposalFeeWithBadDebtProvision = materialCosts.Values.Sum(m => m.ProducerDisposalFeeWithBadDebtProvision),
                EnglandTotal = materialCosts.Values.Sum(m => m.EnglandWithBadDebtProvision),
                WalesTotal = materialCosts.Values.Sum(m => m.WalesWithBadDebtProvision),
                ScotlandTotal = materialCosts.Values.Sum(m => m.ScotlandWithBadDebtProvision),
                NorthernIrelandTotal = materialCosts.Values.Sum(m => m.NorthernIrelandWithBadDebtProvision),

                // For Comms Start
                TotalProducerCommsFee = communicationCosts.Values.Sum(m => m.ProducerTotalCostWithoutBadDebtProvision),
                BadDebtProvisionComms = communicationCosts.Values.Sum(m => m.BadDebtProvision),
                TotalProducerCommsFeeWithBadDebtProvision = communicationCosts.Values.Sum(m => m.ProducerTotalCostwithBadDebtProvision),
                EnglandTotalComms = communicationCosts.Values.Sum(m => m.EnglandWithBadDebtProvision),
                WalesTotalComms = communicationCosts.Values.Sum(m => m.WalesWithBadDebtProvision),
                ScotlandTotalComms = communicationCosts.Values.Sum(m => m.ScotlandWithBadDebtProvision),
                NorthernIrelandTotalComms = communicationCosts.Values.Sum(m => m.NorthernIrelandWithBadDebtProvision),
                ProducerCommsFeesByMaterial = communicationCosts,

                // Set Count/Advice on the producer-total row
                TonnageChangeCount = tonnageChangeCount,
                TonnageChangeAdvice = tonnageChangeAdvice,

                // Section 1
                LocalAuthorityDisposalCostsSectionOne = GetLocalAuthorityDisposalCostsSectionOne(materialCosts),

                // Section 2a
                CommunicationCostsSectionTwoA = GetCommunicationCostsSectionTwoA(communicationCosts),

                // Section 2b
                CommunicationCostsSectionTwoB = GetCommunicationCostsSectionTwoB(calcResult, producersAndSubsidiaries, totalPackagingTonnage),

                // Section-3
                // Percentage of Producer Reported Tonnage vs All Producers
                PercentageofProducerReportedTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducersTotal(producersAndSubsidiaries, totalPackagingTonnage),

                isTotalRow = true,
                isOverallTotalRow = isOverAllTotalRow,
            };

            TwoCCommsCostUtil.UpdateTwoCTotals(calcResult, producerDisposalFees, isOverAllTotalRow, totalRow, producersAndSubsidiaries, totalPackagingTonnage);

            return totalRow;
        }

        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
        public CalcResultSummaryProducerDisposalFees GetProducerRow(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<CalcResultSummaryProducerDisposalFees> producerDisposalFeesLookup,
            IReadOnlyList<ProducerDetail> producerAndSubsidiaries,
            ProducerDetail producer,
            IReadOnlyList<MaterialDetail> materials,
            CalcResult calcResult,
            IReadOnlyList<TotalPackagingTonnagePerRun> totalPackagingTonnage,
            SelfManagedConsumerWaste smcw
        )
        {
            var materialCostSummary = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>();
            var commsCostSummary = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>();
            var level = CalcResultSummaryUtil.GetLevelIndex(producerDisposalFeesLookup, producer);

            // PERF: Use O(1) lookup instead of an O(orgs) FirstOrDefault per producer row.
            organisationsByKey.TryGetValue((producer.ProducerId, producer.SubsidiaryId), out var orgData);

            var result = new CalcResultSummaryProducerDisposalFees
            {
                ProducerIdInt = producer.ProducerId,
                ProducerId = producer.ProducerId.ToString(),
                ProducerName = producer.ProducerName ?? string.Empty,
                SubsidiaryId = producer.SubsidiaryId ?? string.Empty,
                TradingName = producer.TradingName ?? string.Empty,
                Level = level.ToString(),
                IsProducerScaledup = CalcResultSummaryUtil.IsProducerScaledup(producer, ScaledupProducers)
                    ? CommonConstants.Yes
                    : CommonConstants.No,
                IsPartialObligation = CalcResultSummaryUtil.IsProducerPartiallyObligated(producer, PartialObligations, isTotalRow: false)
                    ? CommonConstants.Yes
                    : CommonConstants.No,
                StatusCode = orgData?.StatusCode,
                JoinerDate = orgData?.JoinerDate,
                LeaverDate = orgData?.LeaverDate
            };

            foreach (var material in materials)
            {
                // PERF: Hoist the loop invariants - both values depend only on (producerAndSubsidiaries, material)
                // and were previously recomputed once per subsidiary.
                var l1TotalReportedTonnage = producerAndSubsidiaries.Sum(p => CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material));
                var l1SelfManagedConsumerWasteData = CalcResultSummaryUtil.SumSelfManagedConsumerWasteData(producerAndSubsidiaries, material, isOverAllTotalRow: false, smcw);

                var producerDisposalFeesByMaterial = BuildProducerDisposalFeesByMaterial(
                    projectedMaterialsLookup,
                    producer,
                    material,
                    calcResult,
                    smcw,
                    level,
                    l1TotalReportedTonnage,
                    l1SelfManagedConsumerWasteData);

                materialCostSummary.Add(material.Code, producerDisposalFeesByMaterial);
                result.TotalProducerDisposalFee += producerDisposalFeesByMaterial.ProducerDisposalFee.total ?? 0;
                result.BadDebtProvision += producerDisposalFeesByMaterial.BadDebtProvision;
                result.TotalProducerDisposalFeeWithBadDebtProvision += producerDisposalFeesByMaterial.ProducerDisposalFeeWithBadDebtProvision;
                result.EnglandTotal += producerDisposalFeesByMaterial.EnglandWithBadDebtProvision;
                result.WalesTotal += producerDisposalFeesByMaterial.WalesWithBadDebtProvision;
                result.ScotlandTotal += producerDisposalFeesByMaterial.ScotlandWithBadDebtProvision;
                result.NorthernIrelandTotal += producerDisposalFeesByMaterial.NorthernIrelandWithBadDebtProvision;

                var producerCommsFeesCostByMaterial = BuildProducerCommsFeesCostByMaterial(
                    projectedMaterialsLookup,
                    producer,
                    material,
                    calcResult
                );

                commsCostSummary.Add(material.Code, producerCommsFeesCostByMaterial);
                result.TotalProducerCommsFee += producerCommsFeesCostByMaterial.ProducerTotalCostWithoutBadDebtProvision;
                result.BadDebtProvisionComms += producerCommsFeesCostByMaterial.BadDebtProvision;
                result.TotalProducerCommsFeeWithBadDebtProvision += producerCommsFeesCostByMaterial.ProducerTotalCostwithBadDebtProvision;
                result.EnglandTotalComms += producerCommsFeesCostByMaterial.EnglandWithBadDebtProvision;
                result.WalesTotalComms += producerCommsFeesCostByMaterial.WalesWithBadDebtProvision;
                result.ScotlandTotalComms += producerCommsFeesCostByMaterial.ScotlandWithBadDebtProvision;
                result.NorthernIrelandTotalComms += producerCommsFeesCostByMaterial.NorthernIrelandWithBadDebtProvision;
            }

            result.ProducerDisposalFeesByMaterial = materialCostSummary;
            result.ProducerCommsFeesByMaterial = commsCostSummary;

            // Section 1
            result.LocalAuthorityDisposalCostsSectionOne = new CalcResultSummaryBadDebtProvision
            {
                TotalProducerFeeWithoutBadDebtProvision = result.TotalProducerDisposalFee,
                BadDebtProvision = result.BadDebtProvision,
                TotalProducerFeeWithBadDebtProvision = result.TotalProducerDisposalFeeWithBadDebtProvision,
                EnglandTotalWithBadDebtProvision = result.EnglandTotal,
                WalesTotalWithBadDebtProvision = result.WalesTotal,
                ScotlandTotalWithBadDebtProvision = result.ScotlandTotal,
                NorthernIrelandTotalWithBadDebtProvision = result.NorthernIrelandTotal
            };

            // Section 2a
            result.CommunicationCostsSectionTwoA = new CalcResultSummaryBadDebtProvision
            {
                TotalProducerFeeWithoutBadDebtProvision = result.TotalProducerCommsFee,
                BadDebtProvision = result.BadDebtProvisionComms,
                TotalProducerFeeWithBadDebtProvision = result.TotalProducerCommsFeeWithBadDebtProvision,
                EnglandTotalWithBadDebtProvision = result.EnglandTotalComms,
                WalesTotalWithBadDebtProvision = result.WalesTotalComms,
                ScotlandTotalWithBadDebtProvision = result.ScotlandTotalComms,
                NorthernIrelandTotalWithBadDebtProvision = result.NorthernIrelandTotalComms
            };

            // Section 2b
            result.CommunicationCostsSectionTwoB = new CalcResultSummaryBadDebtProvision
            {
                TotalProducerFeeWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, totalPackagingTonnage),
                BadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2b(calcResult, producer, totalPackagingTonnage),
                TotalProducerFeeWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2b(calcResult, producer, totalPackagingTonnage),
                EnglandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebt(calcResult, producer, totalPackagingTonnage),
                WalesTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebt(calcResult, producer, totalPackagingTonnage),
                ScotlandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebt(calcResult, producer, totalPackagingTonnage),
                NorthernIrelandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebt(calcResult, producer, totalPackagingTonnage)
            };

            var (countStr, advice) = TonnageChangeUtil.ComputeCountAndAdvice(result.Level, materialCostSummary);
            result.TonnageChangeCount = countStr;
            result.TonnageChangeAdvice = advice;

            // Section-3
            // Percentage of Producer Reported Tonnage vs All Producers
            result.PercentageofProducerReportedTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, totalPackagingTonnage);

            TwoCCommsCostUtil.UpdateTwoCRows(calcResult, result, producer, totalPackagingTonnage);

            return result;
        }

        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
        private CalcResultSummaryProducerDisposalFeesByMaterial BuildProducerDisposalFeesByMaterial(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult,
            SelfManagedConsumerWaste smcw,
            int level,
            decimal l1TotalReportedTonnage,
            SelfManagedConsumerWasteData l1SelfManagedConsumerWasteData)
        {
            // PERF: O(1) replacement for the original `Where(...).Select(...).FirstOrDefault()` scan.
            invoicedNetTonnageByProducerMaterial.TryGetValue((producer.ProducerId, material.Id), out var previousInvoicedNetTonnage);

            var totalReportedTonnage = CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, producer, material);

            var selfManagedConsumerWasteData = smcw
                .ProducerTotals
                .Find(x => x.ProducerId == producer.ProducerId && x.SubsidiaryId == producer.SubsidiaryId && x.Level == level)?
                .SelfManagedConsumerWasteDataPerMaterials[material.Code] ?? SelfManagedConsumerWasteData.Zero;

            var producerDisposalFee =
                l1SelfManagedConsumerWasteData.SelfManagedConsumerWasteTonnage > l1TotalReportedTonnage
                    ? (total: 0, red: 0, amber: 0, green: 0)
                    : CalcResultSummaryUtil.GetProducerDisposalFee(material, calcResult, selfManagedConsumerWasteData);

            return new CalcResultSummaryProducerDisposalFeesByMaterial
            {
                HouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household),
                HouseholdPackagingWasteTonnageRagRating = calcResult.ApplyModulation
                    ? Enum.GetValues<RagRating>().ToDictionary(
                        rag => rag,
                        rag => CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, rag))
                    : new(),

                PublicBinTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin),
                PublicBinTonnageRagRating = calcResult.ApplyModulation
                    ? Enum.GetValues<RagRating>().ToDictionary(
                        rag => rag,
                        rag => CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, rag))
                    : new(),

                HouseholdDrinksContainersTonnage = material.Code == MaterialCodes.Glass
                    ? CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers)
                    : new(),
                HouseholdDrinksContainersTonnageRagRating = calcResult.ApplyModulation && material.Code == MaterialCodes.Glass
                    ? Enum.GetValues<RagRating>().ToDictionary(
                        rag => rag,
                        rag => CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, rag))
                    : new(),

                TotalReportedTonnage = totalReportedTonnage,
                TotalReportedTonnageRagRating = calcResult.ApplyModulation
                    ? Enum.GetValues<RagRating>().ToDictionary(
                        rag => rag,
                        rag => CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, producer, material, rag))
                    : new(),

                SelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.SelfManagedConsumerWasteTonnage,
                ActionedSelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.ActionedSelfManagedConsumerWasteTonnage,
                ResidualSelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.ResidualSelfManagedConsumerWasteTonnage,
                NetReportedTonnage = selfManagedConsumerWasteData.NetReportedTonnage,
                TonnageChange = TonnageChangeUtil.ComputePerMaterialChange(level.ToString(), selfManagedConsumerWasteData.NetReportedTonnage.total, previousInvoicedNetTonnage),
                PricePerTonne = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult),
                ProducerDisposalFee = producerDisposalFee,
                BadDebtProvision = CalcResultSummaryUtil.GetBadDebtProvision(calcResult, producerDisposalFee.total),
                ProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(calcResult, producerDisposalFee.total),
                EnglandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.England, producerDisposalFee.total),
                WalesWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.Wales, producerDisposalFee.total),
                ScotlandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.Scotland, producerDisposalFee.total),
                NorthernIrelandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.NorthernIreland, producerDisposalFee.total),
                PreviousInvoicedTonnage = previousInvoicedNetTonnage.HasValue ? previousInvoicedNetTonnage.Value : null,
            };
        }

        private CalcResultSummaryProducerCommsFeesCostByMaterial BuildProducerCommsFeesCostByMaterial(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return new CalcResultSummaryProducerCommsFeesCostByMaterial
            {
                HouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household),
                PublicBinTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin),
                HouseholdDrinksContainersTonnage = material.Code == MaterialCodes.Glass
                    ? CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers)
                    : new(),
                TotalReportedTonnage = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnage(projectedMaterialsLookup, producer, material),
                PriceperTonne = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult),
                ProducerTotalCostWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult),
                BadDebtProvision = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCost(projectedMaterialsLookup, producer, material, calcResult),
                ProducerTotalCostwithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult),
                EnglandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult),
                WalesWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult),
                ScotlandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult),
                NorthernIrelandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult),
            };
        }

        public static ImmutableList<TotalPackagingTonnagePerRun> GetTotalPackagingTonnagePerRun(
            IReadOnlyList<CalcResultProducerAndReportMaterialDetail> allResults,
            IReadOnlyList<MaterialDetail> materials,
            int runId
        )
        {
            var allProducerDetails = allResults.Select(x => x.ProducerDetail).DistinctBy(x => (x.ProducerId, x.SubsidiaryId));
            var allProducerReportedMaterials = allResults.Select(x => x.ProducerReportedMaterialProjected);

            var result =
                (from p in allProducerDetails
                 join pm in allProducerReportedMaterials on p.Id equals pm.ProducerDetailId
                 join m in materials on pm.MaterialId equals m.Id
                 where p.CalculatorRunId == runId &&
                 // TODO again, need this filter?
                 (
                     pm.PackagingType == PackagingTypes.Household || pm.PackagingType == PackagingTypes.PublicBin
                       || (pm.PackagingType == PackagingTypes.HouseholdDrinksContainers && m.Code == MaterialCodes.Glass)
                 )
                 group new { m = pm, p } by new { p.ProducerId, p.SubsidiaryId } into g
                 select new TotalPackagingTonnagePerRun
                 {
                     ProducerId = g.Key.ProducerId,
                     SubsidiaryId = g.Key.SubsidiaryId,
                     TotalPackagingTonnage = g.Sum(x => x.m.PackagingTonnage),
                 }
                ).ToImmutableList();

            return result;
        }

        private Organisation? GetProducerDetailsForTotalRow(int producerId, bool isOverAllTotalRow)
        {
            if (isOverAllTotalRow)
            {
                return null;
            }

            // PERF: O(1) replacement for the previous FirstOrDefault scan of ParentOrganisations.
            parentOrganisationsById.TryGetValue(producerId, out var parentProducer);
            return parentProducer;
        }

        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
        [SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "This is suppressed for now and will be refactored later.")]
        private Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> GetMaterialCosts(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producersAndSubsidiaries,
            IReadOnlyList<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            IReadOnlyList<MaterialDetail> materials,
            CalcResult calcResult,
            bool isOverAllTotalRow,
            SelfManagedConsumerWaste smcw)
        {
            var materialCosts = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>();

            // PERF: Resolve the producer id once per call rather than re-enumerating producersAndSubsidiaries per material.
            var primaryProducerId = producersAndSubsidiaries.FirstOrDefault()?.ProducerId;

            foreach (var material in materials)
            {
                var householdPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.Household);
                var publicBinTonnage = CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.PublicBin);

                // PERF: O(1) replacement for the previous Where(...).FirstOrDefault() scan.
                decimal? previousInvoicedNetTonnage = null;
                if (primaryProducerId.HasValue)
                {
                    invoicedNetTonnageByProducerMaterial.TryGetValue((primaryProducerId.Value, material.Id), out previousInvoicedNetTonnage);
                }

                var selfManagedConsumerWasteData = CalcResultSummaryUtil.SumSelfManagedConsumerWasteData(producersAndSubsidiaries, material, isOverAllTotalRow, smcw);
                var producerDisposalFee = CalcResultSummaryUtil.GetProducerDisposalFee(material, calcResult, selfManagedConsumerWasteData);

                // Compute TonnageChange per the story:
                // - Overall totals row: sum of Level-1 values
                // - Producer totals row (Level 1): per-material logic using net - previous, with null/zero handling
                decimal? tonnageChange = isOverAllTotalRow
                    ? TonnageChangeUtil.GetOverallChangeTotal(producerDisposalFees, material.Code)
                    : TonnageChangeUtil.ComputePerMaterialChange(
                          CommonConstants.LevelOne.ToString(), // producer “total” row is Level 1 for this column
                          selfManagedConsumerWasteData.NetReportedTonnage.total,
                          previousInvoicedNetTonnage);

                materialCosts.Add(material.Code, new CalcResultSummaryProducerDisposalFeesByMaterial
                {
                    HouseholdPackagingWasteTonnage = householdPackagingWasteTonnage,
                    HouseholdPackagingWasteTonnageRagRating = calcResult.ApplyModulation
                        ? Enum.GetValues<RagRating>().ToDictionary(
                            rag => rag,
                            rag => CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.Household, rag))
                        : new(),

                    PublicBinTonnage = publicBinTonnage,
                    PublicBinTonnageRagRating = calcResult.ApplyModulation
                        ? Enum.GetValues<RagRating>().ToDictionary(
                            rag => rag,
                            rag => CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.PublicBin, rag))
                        : new(),

                    HouseholdDrinksContainersTonnage = material.Code == MaterialCodes.Glass
                        ? CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.HouseholdDrinksContainers)
                        : 0,
                    HouseholdDrinksContainersTonnageRagRating = calcResult.ApplyModulation && material.Code == MaterialCodes.Glass
                        ? Enum.GetValues<RagRating>().ToDictionary(
                            rag => rag,
                            rag => CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.HouseholdDrinksContainers, rag))
                        : new(),

                    TotalReportedTonnage = CalcResultSummaryUtil.GetReportedTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material),
                    TotalReportedTonnageRagRating = calcResult.ApplyModulation
                        ? Enum.GetValues<RagRating>().ToDictionary(
                            rag => rag,
                            rag => CalcResultSummaryUtil.GetReportedTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, rag))
                        : new(),

                    SelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.SelfManagedConsumerWasteTonnage,
                    ActionedSelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.ActionedSelfManagedConsumerWasteTonnage,
                    ResidualSelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.ResidualSelfManagedConsumerWasteTonnage,
                    NetReportedTonnage = selfManagedConsumerWasteData.NetReportedTonnage,
                    PricePerTonne = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult),
                    ProducerDisposalFee = producerDisposalFee,
                    BadDebtProvision = CalcResultSummaryUtil.GetBadDebtProvision(calcResult, producerDisposalFee.total),
                    ProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(calcResult, producerDisposalFee.total),
                    EnglandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.England, producerDisposalFee.total),
                    WalesWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.Wales, producerDisposalFee.total),
                    ScotlandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.Scotland, producerDisposalFee.total),
                    NorthernIrelandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.NorthernIreland, producerDisposalFee.total),
                    PreviousInvoicedTonnage = CalcResultSummaryUtil.GetPreviousInvoicedTonnage(producerDisposalFees, producersAndSubsidiaries, ScaledupProducers, PartialObligations, material, isOverAllTotalRow, previousInvoicedNetTonnage),
                    TonnageChange = tonnageChange
                });
            }

            return materialCosts;
        }

        private Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> GetCommunicationCosts(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producersAndSubsidiaries,
            IReadOnlyList<MaterialDetail> materials,
            CalcResult calcResult
        )
        {
            var communicationCosts = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>();

            foreach (var material in materials)
            {
                var publicBinTonnage = CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.PublicBin);

                communicationCosts.Add(material.Code, new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.Household),
                    PublicBinTonnage = publicBinTonnage,
                    HouseholdDrinksContainersTonnage = material.Code == MaterialCodes.Glass
                        ? CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.HouseholdDrinksContainers)
                        : 0,
                    TotalReportedTonnage = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material),
                    PriceperTonne = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult),
                    ProducerTotalCostWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvisionTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                    BadDebtProvision = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCostTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                    ProducerTotalCostwithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvisionTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                    EnglandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForCommsTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                    WalesWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForCommsTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                    ScotlandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForCommsTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                    NorthernIrelandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForCommsTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                });
            }

            return communicationCosts;
        }

        private CalcResultSummaryBadDebtProvision GetLocalAuthorityDisposalCostsSectionOne(
            Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            return new CalcResultSummaryBadDebtProvision
            {
                TotalProducerFeeWithoutBadDebtProvision = materialCostSummary.Sum(m => m.Value.ProducerDisposalFee.total ?? 0),
                BadDebtProvision = materialCostSummary.Values.Sum(m => m.BadDebtProvision),
                TotalProducerFeeWithBadDebtProvision = materialCostSummary.Values.Sum(m => m.ProducerDisposalFeeWithBadDebtProvision),
                EnglandTotalWithBadDebtProvision = materialCostSummary.Values.Sum(m => m.EnglandWithBadDebtProvision),
                WalesTotalWithBadDebtProvision = materialCostSummary.Values.Sum(m => m.WalesWithBadDebtProvision),
                ScotlandTotalWithBadDebtProvision = materialCostSummary.Values.Sum(m => m.ScotlandWithBadDebtProvision),
                NorthernIrelandTotalWithBadDebtProvision = materialCostSummary.Values.Sum(m => m.NorthernIrelandWithBadDebtProvision)
            };
        }

        private CalcResultSummaryBadDebtProvision GetCommunicationCostsSectionTwoA(
            Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            return new CalcResultSummaryBadDebtProvision
            {
                TotalProducerFeeWithoutBadDebtProvision = commsCostSummary.Values.Sum(m => m.ProducerTotalCostWithoutBadDebtProvision),
                BadDebtProvision = commsCostSummary.Values.Sum(m => m.BadDebtProvision),
                TotalProducerFeeWithBadDebtProvision = commsCostSummary.Values.Sum(m => m.ProducerTotalCostwithBadDebtProvision),
                EnglandTotalWithBadDebtProvision = commsCostSummary.Values.Sum(m => m.EnglandWithBadDebtProvision),
                WalesTotalWithBadDebtProvision = commsCostSummary.Values.Sum(m => m.WalesWithBadDebtProvision),
                ScotlandTotalWithBadDebtProvision = commsCostSummary.Values.Sum(m => m.ScotlandWithBadDebtProvision),
                NorthernIrelandTotalWithBadDebtProvision = commsCostSummary.Values.Sum(m => m.NorthernIrelandWithBadDebtProvision),
            };
        }

        private CalcResultSummaryBadDebtProvision GetCommunicationCostsSectionTwoB(
            CalcResult calcResult, IReadOnlyList<ProducerDetail> producersAndSubsidiaries, IReadOnlyList<TotalPackagingTonnagePerRun> totalPackagingTonnage)
        {
            return new CalcResultSummaryBadDebtProvision
            {
                TotalProducerFeeWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2bTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
                BadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2bTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
                TotalProducerFeeWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2bTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
                EnglandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
                WalesTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
                ScotlandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
                NorthernIrelandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage)
            };
        }

        internal async Task UpdateBillingInstructions(CalcResult calcResult, CalcResultSummary result)
        {
            var level1ProducerIds = result.ProducerDisposalFees
                .Where(f => f.Level == CommonConstants.LevelOne.ToString())
                .Select(f => f.ProducerIdInt)
                .Distinct()
                .ToList();

            if (level1ProducerIds.Count == 0) return;

            var entities = await context.ProducerResultFileSuggestedBillingInstruction.Where(p => p.CalculatorRunId == calcResult.CalcResultDetail.RunId &&
                                                                                            level1ProducerIds.Contains(p.ProducerId))
                                                                                            .ToDictionaryAsync(p => p.ProducerId);
            foreach (var fee in result.ProducerDisposalFees)
            {
                if (fee.Level == CommonConstants.LevelOne.ToString() &&
                    entities.TryGetValue(fee.ProducerIdInt, out var producer))
                {
                    producer.CurrentYearInvoiceTotalToDate = fee.BillingInstructionSection?.CurrentYearInvoiceTotalToDate;
                    producer.TonnageChangeSinceLastInvoice = fee.BillingInstructionSection?.TonnageChangeSinceLastInvoice;
                    producer.AmountLiabilityDifferenceCalcVsPrev = fee.BillingInstructionSection?.LiabilityDifference;
                    producer.MaterialPoundThresholdBreached = fee.BillingInstructionSection?.MaterialThresholdBreached;
                    producer.TonnagePoundThresholdBreached = fee.BillingInstructionSection?.TonnageThresholdBreached;
                    producer.PercentageLiabilityDifferenceCalcVsPrev = fee.BillingInstructionSection?.PercentageLiabilityDifference;
                    producer.TonnagePercentageThresholdBreached = fee.BillingInstructionSection?.TonnagePercentageThresholdBreached;
                    producer.SuggestedBillingInstruction = fee.BillingInstructionSection?.SuggestedBillingInstruction!;
                    producer.SuggestedInvoiceAmount = fee.BillingInstructionSection?.SuggestedInvoiceAmount ?? 0m;
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
