using System.Text.Json.Serialization;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Utils;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;

public class CalculationResultsJson
{
    [JsonPropertyName("producerCalculationResultsSummary")]
    public required ProducerCalculationResultsSummary ProducerCalculationResultsSummary { get; set; }

    [JsonPropertyName("producerCalculationResults")]
    public required IEnumerable<CalcSummaryProducerCalculationResults> ProducerCalculationResults { get; set; }

    [JsonPropertyName("producerCalculationResultsTotal")]
    public CalcResultProducerCalculationResultsTotal? ProducerCalculationResultsTotal { get; set; }

    public static CalculationResultsJson From(
        BillingRunContext runContext,
        CalcResult calcResult,
        IImmutableList<MaterialDetail> materials,
        IEnumerable<FeeDetail> producerFeeDetails)
    {
        return new CalculationResultsJson
        {
            ProducerCalculationResultsSummary = ArrangeSummary(calcResult.ProducerFeesTotal),
            ProducerCalculationResults        = ArrangeProducerCalculationResult(runContext, calcResult, producerFeeDetails, materials),
            ProducerCalculationResultsTotal   = ArrangeProducerCalculationResultsTotal(calcResult.ProducerFeesTotal),
        };
    }

    /// <summary>
    /// Arrange the producer fees total using the property
    /// names and ordering required for serialisation.
    /// </summary>
    private static ProducerCalculationResultsSummary ArrangeSummary(FeeDetail total)
    {
        return new ProducerCalculationResultsSummary
        {
            FeeForLaDisposalCostsWithoutBadDebtprovision1 = FormatUtils.FormatCurrency(total.LADisposalCostsSection1.FeeWithoutBadDebt),
            BadDebtProvision1                             = FormatUtils.FormatCurrency(total.LADisposalCostsSection1.BadDebt),
            FeeForLaDisposalCostsWithBadDebtprovision1    = FormatUtils.FormatCurrency(total.LADisposalCostsSection1.ByCountry.Total),

            FeeForCommsCostsByMaterialWithoutBadDebtprovision2a = FormatUtils.FormatCurrency(total.CommsCostsSection2a.FeeWithoutBadDebt),
            BadDebtProvision2a                                  = FormatUtils.FormatCurrency(total.CommsCostsSection2a.BadDebt),
            FeeForCommsCostsByMaterialWitBadDebtprovision2a     = FormatUtils.FormatCurrency(total.CommsCostsSection2a.ByCountry.Total),

            FeeForCommsCostsUkWideWithoutBadDebtprovision2b = FormatUtils.FormatCurrency(total.CommsCostsSection2b.FeeWithoutBadDebt),
            BadDebtProvision2b                              = FormatUtils.FormatCurrency(total.CommsCostsSection2b.BadDebt),
            FeeForCommsCostsUkWideWithBadDebtprovision2b    = FormatUtils.FormatCurrency(total.CommsCostsSection2b.ByCountry.Total),

            FeeForCommsCostsByCountryWithoutBadDebtprovision2c  = FormatUtils.FormatCurrency(total.CommsCostsSection2c.FeeWithoutBadDebt),
            BadDebtProvision2c                                  = FormatUtils.FormatCurrency(total.CommsCostsSection2c.BadDebt),
            FeeForCommsCostsByCountryWideWithBadDebtprovision2c = FormatUtils.FormatCurrency(total.CommsCostsSection2c.ByCountry.Total),

            Total12a2b2cWithBadDebt = FormatUtils.FormatCurrency(total.TotalOnePlus2A2B2CWithBadDebt()),

            SaOperatingCostsWithoutBadDebtProvision3 = FormatUtils.FormatCurrency(total.SaOperatingCostsSection3.FeeWithoutBadDebt),
            BadDebtProvision3                        = FormatUtils.FormatCurrency(total.SaOperatingCostsSection3.BadDebt),
            SaOperatingCostsWithBadDebtProvision3    = FormatUtils.FormatCurrency(total.SaOperatingCostsSection3.ByCountry.Total),

            LaDataPrepCostsWithoutBadDebtProvision4 = FormatUtils.FormatCurrency(total.LaDataPrepSection4.FeeWithoutBadDebt),
            BadDebtProvision4                       = FormatUtils.FormatCurrency(total.LaDataPrepSection4.BadDebt),
            LaDataPrepCostsWithbadDebtProvision4    = FormatUtils.FormatCurrency(total.LaDataPrepSection4.ByCountry.Total),

            OneOffFeeSaSetupCostsWithoutBadDebtProvision5 = FormatUtils.FormatCurrency(total.SaSetupCostsSection5.FeeWithoutBadDebt),
            BadDebtProvision5                             = FormatUtils.FormatCurrency(total.SaSetupCostsSection5.BadDebt),
            OneOffFeeSaSetupCostsWithBadDebtProvision5    = FormatUtils.FormatCurrency(total.SaSetupCostsSection5.ByCountry.Total)
        };
    }

    // IEnumerable - deferred iterator so JsonSerializer writes one producer at a time, consuming
    // producerFeeDetails (a streamed DB read) lazily rather than holding every producer in memory.
    private static IEnumerable<CalcSummaryProducerCalculationResults> ArrangeProducerCalculationResult(
        BillingRunContext runContext,
        CalcResult calcResult,
        IEnumerable<FeeDetail> producerFeeDetails,
        IImmutableList<MaterialDetail> materials)
    {
        var scaledupProducers = calcResult.CalcResultScaledupProducers.ScaledupProducers.Select(p => p.ProducerId).ToImmutableList();

        foreach (var producer in producerFeeDetails)
            yield return CalcSummaryProducerCalculationResults.From(producer, materials, runContext.RequiresModulation, scaledupProducers);
    }

    private static CalcResultProducerCalculationResultsTotal? ArrangeProducerCalculationResultsTotal(FeeDetail total)
    {
        return CalcResultProducerCalculationResultsTotal.From(total);
    }
}
