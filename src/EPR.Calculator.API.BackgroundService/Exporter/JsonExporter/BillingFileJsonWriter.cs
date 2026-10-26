using System.Text.Encodings.Web;
using System.Text.Json;
using EPR.Calculator.API.BackgroundService.Converter;
using EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.JsonExporter;

public interface IBillingFileJsonWriter
{
    Task WriteTo(Stream stream, BillingRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken);
}

public class BillingFileJsonWriter(IMaterialService materialService)
    : IBillingFileJsonWriter
{
    private const int DecimalPrecision = 3;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new DecimalPrecisionConverter(DecimalPrecision) }
    };

    [ActivityTrace]
    public async Task WriteTo(Stream stream, BillingRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken)
    {
        var materials = (await materialService.GetMaterials())
                            .Select(m => m.Code switch
                            {
                                "PC" => m with { Name = "Paper or card" },
                                "FC" => m with { Name = "Fibre composite" },
                                "OT" => m with { Name = "Other materials" },
                                _ => m
                            }).ToImmutableList(); //Maintain previous capitalisation

        if (runContext.RequiresModulation)
        {
            var content = new BillingFileJson2026
            {
                Schema                     = "https://epr.gov.uk/schemas/billing/2026/v1",
                RunId                      = calcResult.CalcResultDetail.RunId,
                FinancialYear              = calcResult.CalcResultDetail.RelativeYear.ToFinancialYear(),
                BadDebtProvisionPercentage = $"{calcResult.CalcResultParameterOtherCost.BadDebtValue:0.00}",
                ModulationResults          = CalcResultModulationResults.From(calcResult.CalcResultModulation!),
                Materials                  = MaterialPrices.FromAll(materials, calcResult, applyModulation: true).ToList(),
                Producers                  = BuildProducerGroups(calcResult.ProducerFees, materials, applyModulation: true),
            };
            await JsonSerializer.SerializeAsync(stream, content, JsonSerializerOptions, cancellationToken);
            return;
        }

        var content2025 = new BillingFileJson2025
        {
            Schema                     = "https://epr.gov.uk/schemas/billing/2025/v1",
            RunId                      = calcResult.CalcResultDetail.RunId,
            FinancialYear              = calcResult.CalcResultDetail.RelativeYear.ToFinancialYear(),
            BadDebtProvisionPercentage = $"{calcResult.CalcResultParameterOtherCost.BadDebtValue:0.00}",
            Materials                  = MaterialPrices.FromAll(materials, calcResult, applyModulation: false).ToList(),
            Producers                  = calcResult.ProducerFees.Details
                                             .Select(p => ProducerResult.From(p.FeeDetail, materials, applyModulation: false))
                                             .ToList(),
        };
        await JsonSerializer.SerializeAsync(stream, content2025, JsonSerializerOptions, cancellationToken);
    }

    /// <summary>
    /// Groups the flat per-row producer summary (one Level-1 row per group, plus one Level-2 row
    /// per member when the group is composite) into one <see cref="ProducerGroupResult"/> per
    /// producerID. A single-organisation group has no Level-2 rows, so its Level-1 row also serves
    /// as its sole member.
    /// </summary>
    private static List<ProducerGroupResult> BuildProducerGroups(
        ProducerFees producerFees,
        IImmutableList<MaterialDetail> materials,
        bool applyModulation) =>
        producerFees.Details
            .Select(p => p.FeeDetail)
            .GroupBy(p => p.ProducerId)
            .Select(group =>
            {
                var rows          = group.ToList();
                var aggregateRow  = rows.Single(r => r.Level == "1");
                var memberRows    = rows.Where(r => r.Level == "2").ToList();

                return ProducerGroupResult.From(aggregateRow, memberRows.Count > 0 ? memberRows : [aggregateRow], materials, applyModulation);
            })
            .ToList();
}
