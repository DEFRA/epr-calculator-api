using System.Text.Json.Serialization;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Converter;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalcResultLateReportingTonnageJson
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("calcResultLateReportingTonnageDetails")]
    public List<CalcResultLateReportingTonnageDetailsJson> CalcResultLateReportingTonnageDetails { get; set; } = null!;

    [JsonPropertyName("calcResultLateReportingTonnageTotal")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal CalcResultLateReportingTonnageTotal { get; set; }

    public static CalcResultLateReportingTonnageJson From(CalcResultLateReportingTonnage calcResultLateReportingTonnage, IImmutableList<MaterialDetail> materials) =>
        new ()
        {
            Name = "Late Reporting Tonnage",
            CalcResultLateReportingTonnageDetails = calcResultLateReportingTonnage.ByMaterial
                .Select(kv => new CalcResultLateReportingTonnageDetailsJson
                {
                    MaterialName = materials.First(m => m.Code == kv.Key).Name,
                    TotalLateReportingTonnage = kv.Value.Total
                })
                .ToList(),
            CalcResultLateReportingTonnageTotal = calcResultLateReportingTonnage.Total.Total
        };
}

public class CalcResultLateReportingTonnageDetailsJson
{
    [JsonPropertyName("materialName")]
    public string MaterialName { get; set; } = null!;


    [JsonPropertyName("totalLateReportingTonnage")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal TotalLateReportingTonnage { get; set; }
}
