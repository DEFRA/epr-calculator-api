using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

internal sealed class ProducerRowBuilder(
    ImmutableDictionary<(int ProducerId, int MaterialId), decimal?> invoicedNetTonnageByProducerMaterial,
    ImmutableDictionary<(int OrganisationId, string? SubsidiaryId), Organisation> organisationsByKey,
    ImmutableDictionary<int, Organisation> parentOrganisationsById)
{
    /// <summary>
    /// Builds a Level-1 total row for a producer group by aggregating its already-computed L2 rows.
    /// Tonnage and cost fields are additive sums from the L2 rows; SMCW-derived fields use the
    /// independently-computed Level-1 record from <paramref name="smcw"/> (cannot be derived by
    /// summing subsidiaries, because SMCW is computed at the group level).
    /// </summary>
    public ProducerFeeDetail GetL1TotalRow(
        int producerId,
        IReadOnlyList<ProducerFeeDetail> l2Rows,
        CalcResult calcResult,
        SelfManagedConsumerWaste smcw,
        IReadOnlyList<MaterialDetail> materials
    )
    {
        var materialCosts = new Dictionary<string, Fees>();

        var l1SmcwRecord = smcw.ProducerTotals.Single(x => x.Level == 1 && x.ProducerId == producerId);

        foreach (var material in materials)
        {
            var l2MatRows = l2Rows
                .Where(r => r.FeeDetail.FeesByMaterial.ContainsKey(material.Code))
                .Select(r => r.FeeDetail.FeesByMaterial[material.Code])
                .ToList();

            var l1Smcw = l1SmcwRecord.SmcwByMaterial.GetValueOrDefault(material.Code)
                ?? SelfManagedConsumerWasteData.Zero;

            invoicedNetTonnageByProducerMaterial.TryGetValue((producerId, material.Id), out var prevInvoiced);

            var l1TotalReportedTonnage = l2MatRows.Sum(r => r.DisposalFee.TotalTonnage.TotalRamTonnage());

            var disposalFee = l1Smcw.SmcwTonnage > l1TotalReportedTonnage
                ? new RamTonnageGroup { Total = 0m, Red = 0m, Amber = 0m, Green = 0m }
                : ProducerFeesUtil.GetProducerDisposalFee(material, calcResult, l1Smcw);

            materialCosts[material.Code] = new Fees {
                // Additive from L2 rows
                DisposalFee = new DisposalFee
                {
                    HhTonnage     = AggregateRAM(l2MatRows, r => r.DisposalFee.HhTonnage),
                    PbTonnage     = AggregateRAM(l2MatRows, r => r.DisposalFee.PbTonnage),
                    HdcTonnage    = AggregateRAM(l2MatRows, r => r.DisposalFee.HdcTonnage),
                    TotalTonnage  = AggregateRAM(l2MatRows, r => r.DisposalFee.TotalTonnage),

                    // From L1 SMCW record — not derivable by summing L2 values
                    SmcwTonnage           = l1Smcw.SmcwTonnage,
                    ActionedSmcwTonnage   = new RamTonnageGroup {
                        Total   = l1Smcw.ActionedSmcwTonnage.Total,
                        Red     = l1Smcw.ActionedSmcwTonnage.Red,
                        Amber   = l1Smcw.ActionedSmcwTonnage.Amber,
                        Green   = l1Smcw.ActionedSmcwTonnage.Green
                    },
                    ResidualSmcwTonnage   = l1Smcw.ResidualSmcwTonnage,
                    NetTonnage            = new RamTonnageGroup {
                        Total   = l1Smcw.NetTonnage.Total,
                        Red     = l1Smcw.NetTonnage.Red,
                        Amber   = l1Smcw.NetTonnage.Amber,
                        Green   = l1Smcw.NetTonnage.Green
                    },

                    // Derived from L1 SMCW
                    TonnageChange            = TonnageChangeUtil.ComputePerMaterialChange(CommonConstants.LevelOne.ToString(), l1Smcw.NetTonnage.Total, prevInvoiced),
                    PricePerTonne            = ProducerFeesUtil.GetPricePerTonne(material, calcResult),
                    Fee                      = disposalFee,
                    BadDebt                  = ProducerFeesUtil.GetBadDebtProvision(calcResult, disposalFee.Total),
                    FeeWithBadDebtByCountry  = ProducerFeesUtil.GetProducerDisposalFeeWithBadDebtProvision(calcResult, disposalFee.Total),
                    PreviousInvoicedTonnage  = prevInvoiced
                },
                CommFee            = new CommsFee
                {
                    HhTonnage      = l2MatRows.Sum(r => r.CommFee.HhTonnage),
                    PbTonnage      = l2MatRows.Sum(r => r.CommFee.PbTonnage),
                    HdcTonnage     = l2MatRows.Sum(r => r.CommFee.HdcTonnage),
                    TotalTonnage   = l2MatRows.Sum(r => r.CommFee.TotalTonnage),
                    PricePerTonne  = l2MatRows.Count > 0 ? l2MatRows[0].CommFee.PricePerTonne : 0,
                    Costs          = l2MatRows.Select(r => r.CommFee.Costs).Sum(),
                }
            };
        }

        var producerForTotalRow = GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow: false);
        var (tonnageChangeCount, tonnageChangeAdvice) = TonnageChangeUtil.ComputeCountAndAdvice(
            CommonConstants.LevelOne.ToString(), materialCosts.ToDictionary(k => k.Key, v => v.Value.DisposalFee));

        return new ProducerFeeDetail
        {
            FeeDetail = new FeeDetail
            {
                ProducerId          = producerId,
                ProducerName        = producerForTotalRow?.OrganisationName ?? string.Empty,
                SubsidiaryId        = string.Empty,
                Level               = CommonConstants.LevelOne.ToString(),
                TradingName         = producerForTotalRow?.TradingName ?? string.Empty,
                StatusCode          = producerForTotalRow?.StatusCode,
                JoinerDate          = producerForTotalRow?.JoinerDate,
                LeaverDate          = producerForTotalRow?.LeaverDate,

                FeesByMaterial        = materialCosts,
                CommsCostsSection2a   = GetCommunicationCostsSectionTwoA(materialCosts.ToDictionary(k => k.Key, v => v.Value.CommFee)),

                TonnageChangeCount    = tonnageChangeCount,
                TonnageChangeAdvice   = tonnageChangeAdvice,

                LADisposalCostsSection1   = GetLocalAuthorityDisposalCostsSectionOne(materialCosts.ToDictionary(k => k.Key, v => v.Value.DisposalFee)),
                CommsCostsSection2b       = l2Rows.Select(r => r.FeeDetail.CommsCostsSection2b).Sum(),

                ReportedTonnagePercentage = l2Rows.Sum(r => r.FeeDetail.ReportedTonnagePercentage),

                CommsCostsSection2c       = l2Rows.Select(r => r.FeeDetail.CommsCostsSection2c).Sum()
            }
        };
    }

    /// <summary>
    /// Builds the overall-total row by summing all Level-1 rows (one per producer group).
    /// All fields — including SMCW — are additive: the overall SMCW equals the sum of the
    /// Level-1 SMCW records by construction in <see cref="SelfManagedConsumerWasteService"/>.
    /// </summary>
    public static FeeDetail GetOverallTotalRow(
        IReadOnlyList<ProducerFeeDetail> l1Rows,
        IReadOnlyList<MaterialDetail> materials
    )
    {
        var materialCosts = new Dictionary<string, Fees>();

        // Accumulators for the post-loop row-level sums, folded into a single pass.
        var commsCostsSection2b = FeeWithBadDebt.Empty;
        decimal percentageOfProducerTonnage = 0;
        var commsCostsSection2c = FeeWithBadDebt.Empty;

        // Per-material sub-lists built in a single pass over l1Rows per material.
        var matRowsByCode   = materials.ToDictionary(m => m.Code, _ => new List<Fees>());

        foreach (var row in l1Rows.Select(fee => fee.FeeDetail))
        {
            commsCostsSection2b             += row.CommsCostsSection2b;
            percentageOfProducerTonnage     += row.ReportedTonnagePercentage;
            commsCostsSection2c             += row.CommsCostsSection2c;

            foreach (var materialCode in materials.Select(material => material.Code))
            {
                if (row.FeesByMaterial.TryGetValue(materialCode, out var mat))
                    matRowsByCode[materialCode].Add(mat);
            }
        }

        foreach (var materialCode in materials.Select(material => material.Code))
        {
            var l1MatRows = matRowsByCode[materialCode];

            materialCosts[materialCode] = new Fees {
                DisposalFee             = new DisposalFee
                {
                    HhTonnage                 = AggregateRAM(l1MatRows, r => r.DisposalFee.HhTonnage),
                    PbTonnage                 = AggregateRAM(l1MatRows, r => r.DisposalFee.PbTonnage),
                    HdcTonnage                = AggregateRAM(l1MatRows, r => r.DisposalFee.HdcTonnage),
                    TotalTonnage              = AggregateRAM(l1MatRows, r => r.DisposalFee.TotalTonnage),

                    // SMCW is additive: overall SMCW = sum of Level-1 SMCW records
                    SmcwTonnage               = l1MatRows.Sum(r => r.DisposalFee.SmcwTonnage),
                    ActionedSmcwTonnage       = AggregateRAMTonnageGroup(l1MatRows, r => r.DisposalFee.ActionedSmcwTonnage),
                    ResidualSmcwTonnage       = l1MatRows.Sum(r => r.DisposalFee.ResidualSmcwTonnage),
                    NetTonnage                = AggregateRAMTonnageGroup(l1MatRows, r => r.DisposalFee.NetTonnage),

                    TonnageChange             = l1MatRows.Sum(r => r.DisposalFee.TonnageChange),
                    PricePerTonne             = l1MatRows.Count > 0 ? l1MatRows[0].DisposalFee.PricePerTonne with { } : new RamTonnageGroup(),
                    Fee                       = AggregateRAMTonnageGroup(l1MatRows, r => r.DisposalFee.Fee),
                    BadDebt                   = l1MatRows.Sum(r => r.DisposalFee.BadDebt),
                    FeeWithBadDebtByCountry   = ByCountryCost.Sum([.. l1MatRows.Select(r => r.DisposalFee.FeeWithBadDebtByCountry)]),
                    PreviousInvoicedTonnage   = l1MatRows.Sum(r => r.DisposalFee.PreviousInvoicedTonnage)
                },
                CommFee           = new CommsFee
                {
                    HhTonnage     = l1MatRows.Sum(r => r.CommFee.HhTonnage),
                    PbTonnage     = l1MatRows.Sum(r => r.CommFee.PbTonnage),
                    HdcTonnage    = l1MatRows.Sum(r => r.CommFee.HdcTonnage),
                    TotalTonnage  = l1MatRows.Sum(r => r.CommFee.TotalTonnage),
                    PricePerTonne = l1MatRows.Count > 0 ? l1MatRows[0].CommFee.PricePerTonne : 0,
                    Costs         = l1MatRows.Select(r => r.CommFee.Costs).Sum(),
                }
            };
        }

        return new FeeDetail
        {
            ProducerId          = 0,
            ProducerName        = string.Empty,
            SubsidiaryId        = string.Empty,
            TradingName         = string.Empty,
            StatusCode          = null,
            JoinerDate          = null,
            LeaverDate          = CommonConstants.Totals,

            FeesByMaterial            = materialCosts,
            CommsCostsSection2a       = GetCommunicationCostsSectionTwoA(materialCosts.ToDictionary(k => k.Key, v => v.Value.CommFee)),

            LADisposalCostsSection1   = GetLocalAuthorityDisposalCostsSectionOne(materialCosts.ToDictionary(k => k.Key, v => v.Value.DisposalFee)),
            CommsCostsSection2b       = commsCostsSection2b,

            ReportedTonnagePercentage = percentageOfProducerTonnage,
            CommsCostsSection2c       = commsCostsSection2c
        };
    }

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
    public ProducerFeeDetail GetProducerRow(
        RunContext runContext,
        ILookup<(int, string?), ProducerMaterialPackaging> projectedMaterialsLookup,
        bool hasGroupTotalRow,
        IReadOnlyList<ProducerDetail> producerAndSubsidiaries,
        ProducerDetail producer,
        IReadOnlyList<MaterialDetail> materials,
        CalcResult calcResult,
        IReadOnlyList<TotalPackagingTonnagePerRun> totalPackagingTonnage,
        SelfManagedConsumerWaste smcw
    )
    {
        var materialFeeSummary = new Dictionary<string, Fees>();
        var level = hasGroupTotalRow ? (int)CalcResultSummaryLevelIndex.Two : (int)CalcResultSummaryLevelIndex.One;

        // PERF: Use O(1) lookup instead of an O(orgs) FirstOrDefault per producer row.
        organisationsByKey.TryGetValue((producer.ProducerId, producer.SubsidiaryId), out var orgData);

        var result = new ProducerFeeDetail
        {
            FeeDetail = new FeeDetail
            {

                ProducerId          = producer.ProducerId,
                SubsidiaryId        = producer.SubsidiaryId ?? string.Empty,
                Level               = level.ToString(),
                ProducerName        = producer.ProducerName ?? string.Empty,
                TradingName         = producer.TradingName ?? string.Empty,
                StatusCode          = orgData?.StatusCode,
                JoinerDate          = orgData?.JoinerDate,
                LeaverDate          = orgData?.LeaverDate
            }
        };

        var commsSection2a = FeeWithBadDebt.Empty;

        foreach (var material in materials)
        {
            // PERF: Hoist the loop invariants - both values depend only on (producerAndSubsidiaries, material)
            // and were previously recomputed once per subsidiary.
            var l1TotalReportedTonnage = producerAndSubsidiaries.Sum(p => ProducerFeesUtil.GetReportedTonnage(projectedMaterialsLookup, p, material));
            var l1SelfManagedConsumerWasteData = ProducerFeesUtil.SumSelfManagedConsumerWasteData(producerAndSubsidiaries, material, smcw);

            var producerDisposalFeesByMaterial = BuildProducerDisposalFeesByMaterial(
                runContext,
                projectedMaterialsLookup,
                producer,
                material,
                calcResult,
                smcw,
                level,
                l1TotalReportedTonnage,
                l1SelfManagedConsumerWasteData);

            result.FeeDetail.LADisposalCostsSection1 +=
                new FeeWithBadDebt
                {
                    FeeWithoutBadDebt = producerDisposalFeesByMaterial.Fee.Total ?? 0,
                    BadDebt           = producerDisposalFeesByMaterial.BadDebt,
                    ByCountry    = producerDisposalFeesByMaterial.FeeWithBadDebtByCountry
                };

            var producerCommsFeesCostByMaterial = BuildProducerCommsFeesCostByMaterial(
                projectedMaterialsLookup,
                producer,
                material,
                calcResult
            );

            materialFeeSummary.Add(material.Code, new Fees {
                DisposalFee = producerDisposalFeesByMaterial,
                CommFee = producerCommsFeesCostByMaterial
            });
            commsSection2a += producerCommsFeesCostByMaterial.Costs;
        }

        result.FeeDetail.FeesByMaterial = materialFeeSummary;
        result.FeeDetail.CommsCostsSection2a = commsSection2a;

        result.FeeDetail.CommsCostsSection2b = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsCosts(calcResult, producer, totalPackagingTonnage);

        var (countStr, advice) = TonnageChangeUtil.ComputeCountAndAdvice(result.FeeDetail.Level, materialFeeSummary.ToDictionary(k => k.Key, v => v.Value.DisposalFee));
        result.FeeDetail.TonnageChangeCount  = countStr;
        result.FeeDetail.TonnageChangeAdvice = advice;

        // Section-3: Percentage of Producer Reported Tonnage vs All Producers
        result.FeeDetail.ReportedTonnagePercentage = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, totalPackagingTonnage);

        TwoCCommsCostProducer.UpdateTwoCRows(calcResult, result.FeeDetail);

        return result;
    }

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
    [SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "This is suppressed for now and will be refactored later.")]
    private DisposalFee BuildProducerDisposalFeesByMaterial(
        RunContext runContext,
        ILookup<(int, string?), ProducerMaterialPackaging> projectedMaterialsLookup,
        ProducerDetail producer,
        MaterialDetail material,
        CalcResult calcResult,
        SelfManagedConsumerWaste smcw,
        int level,
        decimal l1TotalReportedTonnage,
        SelfManagedConsumerWasteData l1SelfManagedConsumerWasteData
    )
    {
        // PERF: O(1) replacement for the original `Where(...).Select(...).FirstOrDefault()` scan.
        invoicedNetTonnageByProducerMaterial.TryGetValue((producer.ProducerId, material.Id), out var previousInvoicedNetTonnage);

        RamTonnage hhRamTonnage;
        RamTonnage pbRamTonnage;
        RamTonnage hdcRamTonnage;

        if (runContext.RequiresModulation)
        {
            hhRamTonnage = new RamTonnage
            {
                Red = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, RagRating.Red),
                Amber = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, RagRating.Amber),
                Green = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, RagRating.Green),
                RedMedical = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, RagRating.RedMedical),
                AmberMedical = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, RagRating.AmberMedical),
                GreenMedical = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, RagRating.GreenMedical),
            };

            pbRamTonnage = new RamTonnage
            {
                Red = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, RagRating.Red),
                Amber = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, RagRating.Amber),
                Green = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, RagRating.Green),
                RedMedical = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, RagRating.RedMedical),
                AmberMedical = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, RagRating.AmberMedical),
                GreenMedical = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, RagRating.GreenMedical),
            };

            hdcRamTonnage = material.Code == MaterialCodes.Glass ? new RamTonnage
            {
                Red = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, RagRating.Red),
                Amber = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, RagRating.Amber),
                Green = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, RagRating.Green),
                RedMedical = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, RagRating.RedMedical),
                AmberMedical = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, RagRating.AmberMedical),
                GreenMedical = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, RagRating.GreenMedical),
            } : RamTonnage.Empty;
        }
        else
        {
            hhRamTonnage  = new RamTonnage { Amber = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household) };
            pbRamTonnage  = new RamTonnage { Amber = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin) };
            hdcRamTonnage = material.Code == MaterialCodes.Glass ? new RamTonnage { 
                Amber = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers)
            } : RamTonnage.Empty;
        }

        var totalRamTonnage = new RamTonnage
        {
            Red = hhRamTonnage.Red + pbRamTonnage.Red + hdcRamTonnage.Red,
            Amber = hhRamTonnage.Amber + pbRamTonnage.Amber + hdcRamTonnage.Amber,
            Green = hhRamTonnage.Green + pbRamTonnage.Green + hdcRamTonnage.Green,
            RedMedical = hhRamTonnage.RedMedical + pbRamTonnage.RedMedical + hdcRamTonnage.RedMedical,
            AmberMedical = hhRamTonnage.AmberMedical + pbRamTonnage.AmberMedical + hdcRamTonnage.AmberMedical,
            GreenMedical = hhRamTonnage.GreenMedical + pbRamTonnage.GreenMedical + hdcRamTonnage.GreenMedical,
        };

        var selfManagedConsumerWasteData = smcw
            .ProducerTotals
            .SingleOrDefault(x => x.ProducerId == producer.ProducerId && x.SubsidiaryId == producer.SubsidiaryId && x.Level == level)?
            .SmcwByMaterial[material.Code] ?? SelfManagedConsumerWasteData.Zero;

        var producerDisposalFee =
            l1SelfManagedConsumerWasteData.SmcwTonnage > l1TotalReportedTonnage
                ? new RamTonnageGroup { Total = 0m, Red = 0m, Amber = 0m, Green = 0m }
                : ProducerFeesUtil.GetProducerDisposalFee(material, calcResult, selfManagedConsumerWasteData);

        return new DisposalFee
        {
            HhTonnage = hhRamTonnage,
            PbTonnage = pbRamTonnage,
            HdcTonnage       = hdcRamTonnage,
            TotalTonnage     = totalRamTonnage,

            SmcwTonnage         = selfManagedConsumerWasteData.SmcwTonnage,
            ActionedSmcwTonnage = new RamTonnageGroup {
                    Total = selfManagedConsumerWasteData.ActionedSmcwTonnage.Total,
                    Red = selfManagedConsumerWasteData.ActionedSmcwTonnage.Red,
                    Amber = selfManagedConsumerWasteData.ActionedSmcwTonnage.Amber,
                    Green = selfManagedConsumerWasteData.ActionedSmcwTonnage.Green
                },
            ResidualSmcwTonnage = selfManagedConsumerWasteData.ResidualSmcwTonnage,
            NetTonnage                      =  new RamTonnageGroup {
                    Total = selfManagedConsumerWasteData.NetTonnage.Total,
                    Red = selfManagedConsumerWasteData.NetTonnage.Red,
                    Amber = selfManagedConsumerWasteData.NetTonnage.Amber,
                    Green = selfManagedConsumerWasteData.NetTonnage.Green
                },
            TonnageChange                           = TonnageChangeUtil.ComputePerMaterialChange(level.ToString(), selfManagedConsumerWasteData.NetTonnage.Total, previousInvoicedNetTonnage),
            PricePerTonne                           = ProducerFeesUtil.GetPricePerTonne(material, calcResult),
            Fee                     = producerDisposalFee,
            BadDebt                        = ProducerFeesUtil.GetBadDebtProvision(calcResult, producerDisposalFee.Total),
            FeeWithBadDebtByCountry = ProducerFeesUtil.GetProducerDisposalFeeWithBadDebtProvision(calcResult, producerDisposalFee.Total),
            PreviousInvoicedTonnage                 = previousInvoicedNetTonnage.HasValue ? previousInvoicedNetTonnage.Value : null
        };
    }

    private static CommsFee BuildProducerCommsFeesCostByMaterial(
        ILookup<(int, string?), ProducerMaterialPackaging> projectedMaterialsLookup,
        ProducerDetail producer,
        MaterialDetail material,
        CalcResult calcResult
    )
    {
        var hhTonnage  = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household);
        var pbTonnage  = ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin);
        var hdcTonnage = material.Code == MaterialCodes.Glass
            ? ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers)
            : 0m;
        var totalTonnage = hhTonnage + pbTonnage + hdcTonnage;

        return new CommsFee
        {
            HhTonnage   = hhTonnage,
            PbTonnage   = pbTonnage,
            HdcTonnage         = hdcTonnage,
            TotalTonnage       = totalTonnage,
            PricePerTonne      = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult),
            Costs              = CalcResultSummaryCommsCostTwoA.GetCommsFeesCosts(totalTonnage, material, calcResult)
        };
    }

    private static FeeWithBadDebt GetLocalAuthorityDisposalCostsSectionOne(
        Dictionary<string, DisposalFee> materialCostSummary
    ) =>
        materialCostSummary.Values.Select(m => new FeeWithBadDebt
        {
            FeeWithoutBadDebt = m.Fee.Total ?? 0,
            BadDebt           = m.BadDebt,
            ByCountry    = m.FeeWithBadDebtByCountry,
        }).Sum();

    private static FeeWithBadDebt GetCommunicationCostsSectionTwoA(
        Dictionary<string, CommsFee> commsCostSummary
    ) =>
        commsCostSummary.Values.Select(m => m.Costs).Sum();


    private static RamTonnage AggregateRAM(
        IReadOnlyList<Fees> rows,
        Func<Fees, RamTonnage> selector
    ) =>
        rows.Aggregate(RamTonnage.Empty, (acc, src) => {
            var tonnage = selector(src);

            return acc with
            {
                Red = acc.Red + tonnage.Red,
                Amber = acc.Amber + tonnage.Amber,
                Green = acc.Green + tonnage.Green,
                RedMedical = acc.RedMedical + tonnage.RedMedical,
                AmberMedical = acc.AmberMedical + tonnage.AmberMedical,
                GreenMedical = acc.GreenMedical + tonnage.GreenMedical,
            };
        });

    private static RamTonnageGroup AggregateRAMTonnageGroup(
        IReadOnlyList<Fees> rows,
        Func<Fees, RamTonnageGroup> selector
    ) =>
        rows.Aggregate(new RamTonnageGroup { Total = 0, Red = 0, Amber = 0, Green = 0 }, (acc, src) =>
        {
            var selected = selector(src);

            return acc with 
            {
                Total = acc.Total + (selected.Total ?? 0),
                Red =   acc.Red   + (selected.Red ?? 0),
                Amber = acc.Amber + (selected.Amber ?? 0),
                Green = acc.Green + (selected.Green ?? 0)
            };
        });

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
}

