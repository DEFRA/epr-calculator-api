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
        IEnumerable<FeeDetail>? producerFeeDetails = null)
    {
        var scaledupProducers = calcResult.CalcResultScaledupProducers.ScaledupProducers.Select(p => p.ProducerId).ToImmutableList();

        return new CalculationResultsJson
        {
            ProducerCalculationResultsSummary = ArrangeSummary(calcResult.ProducerFees),
            ProducerCalculationResults        = ArrangeProducerCalculationResult(
                producerFeeDetails ?? calcResult.ProducerFees.Details.Select(d => d.FeeDetail),
                materials, runContext.RequiresModulation, scaledupProducers),
            ProducerCalculationResultsTotal   = ArrangeProducerCalculationResultsTotal(calcResult.ProducerFees),
        };
    }

    /// <summary>
    /// Arrange the ProducerFees data using the property
    /// names and ordering required for serialisation.
    /// </summary>
    private static ProducerCalculationResultsSummary ArrangeSummary(ProducerFees data)
    {
        return new ProducerCalculationResultsSummary
        {
            FeeForLaDisposalCostsWithoutBadDebtprovision1 = FormatUtils.FormatCurrency(data.Total.LADisposalCostsSection1.FeeWithoutBadDebt),
            BadDebtProvision1                             = FormatUtils.FormatCurrency(data.Total.LADisposalCostsSection1.BadDebt),
            FeeForLaDisposalCostsWithBadDebtprovision1    = FormatUtils.FormatCurrency(data.Total.LADisposalCostsSection1.ByCountry.Total),

            FeeForCommsCostsByMaterialWithoutBadDebtprovision2a = FormatUtils.FormatCurrency(data.Total.CommsCostsSection2a.FeeWithoutBadDebt),
            BadDebtProvision2a                                  = FormatUtils.FormatCurrency(data.Total.CommsCostsSection2a.BadDebt),
            FeeForCommsCostsByMaterialWitBadDebtprovision2a     = FormatUtils.FormatCurrency(data.Total.CommsCostsSection2a.ByCountry.Total),

            FeeForCommsCostsUkWideWithoutBadDebtprovision2b = FormatUtils.FormatCurrency(data.Total.CommsCostsSection2b.FeeWithoutBadDebt),
            BadDebtProvision2b                              = FormatUtils.FormatCurrency(data.Total.CommsCostsSection2b.BadDebt),
            FeeForCommsCostsUkWideWithBadDebtprovision2b    = FormatUtils.FormatCurrency(data.Total.CommsCostsSection2b.ByCountry.Total),

            FeeForCommsCostsByCountryWithoutBadDebtprovision2c  = FormatUtils.FormatCurrency(data.Total.CommsCostsSection2c.FeeWithoutBadDebt),
            BadDebtProvision2c                                  = FormatUtils.FormatCurrency(data.Total.CommsCostsSection2c.BadDebt),
            FeeForCommsCostsByCountryWideWithBadDebtprovision2c = FormatUtils.FormatCurrency(data.Total.CommsCostsSection2c.ByCountry.Total),

            Total12a2b2cWithBadDebt = FormatUtils.FormatCurrency(data.Total.TotalOnePlus2A2B2CWithBadDebt()),

            SaOperatingCostsWithoutBadDebtProvision3 = FormatUtils.FormatCurrency(data.Total.SaOperatingCostsSection3.FeeWithoutBadDebt),
            BadDebtProvision3                        = FormatUtils.FormatCurrency(data.Total.SaOperatingCostsSection3.BadDebt),
            SaOperatingCostsWithBadDebtProvision3    = FormatUtils.FormatCurrency(data.Total.SaOperatingCostsSection3.ByCountry.Total),

            LaDataPrepCostsWithoutBadDebtProvision4 = FormatUtils.FormatCurrency(data.Total.LaDataPrepSection4.FeeWithoutBadDebt),
            BadDebtProvision4                       = FormatUtils.FormatCurrency(data.Total.LaDataPrepSection4.BadDebt),
            LaDataPrepCostsWithbadDebtProvision4    = FormatUtils.FormatCurrency(data.Total.LaDataPrepSection4.ByCountry.Total),

            OneOffFeeSaSetupCostsWithoutBadDebtProvision5 = FormatUtils.FormatCurrency(data.Total.SaSetupCostsSection5.FeeWithoutBadDebt),
            BadDebtProvision5                             = FormatUtils.FormatCurrency(data.Total.SaSetupCostsSection5.BadDebt),
            OneOffFeeSaSetupCostsWithBadDebtProvision5    = FormatUtils.FormatCurrency(data.Total.SaSetupCostsSection5.ByCountry.Total)
        };
    }

    // Lazily projected - kept as a deferred iterator so JsonSerializer pulls one producer at a time
    // when the source is a streamed DB read, rather than the whole set being held in memory.
    private static IEnumerable<CalcSummaryProducerCalculationResults> ArrangeProducerCalculationResult(
        IEnumerable<FeeDetail> feeDetails,
        IImmutableList<MaterialDetail> materials,
        bool requiresModulation,
        IImmutableList<int> scaledupProducerIds)
    {
        foreach (var feeDetail in feeDetails)
            yield return CalcSummaryProducerCalculationResults.From(feeDetail, materials, requiresModulation, scaledupProducerIds);
    }

    private static CalcResultProducerCalculationResultsTotal? ArrangeProducerCalculationResultsTotal(ProducerFees producerFees)
    {
        return CalcResultProducerCalculationResultsTotal.From(producerFees);
    }
}
