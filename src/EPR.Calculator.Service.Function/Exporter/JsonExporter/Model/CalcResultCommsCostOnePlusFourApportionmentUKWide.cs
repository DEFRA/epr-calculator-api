using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalcResultCommsCostOnePlusFourApportionmentUKWide
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("englandCommsCostUKWide")]
    public required string EnglandCommsCostUKWide { get; set; }

    [JsonPropertyName("walesCommsCostUKWide")]
    public required string WalesCommsCostUKWide { get; set; }

    [JsonPropertyName("scotlandCommsCostUKWide")]
    public required string ScotlandCommsCostUKWide { get; set; }

    [JsonPropertyName("northernIrelandCommsCostUKWide")]
    public required string NorthernIrelandCommsCostUKWide { get; set; }

    [JsonPropertyName("totalCommsCostUKWide")]
    public required string TotalCommsCostUKWide { get; set; }

    public static CalcResultCommsCostOnePlusFourApportionmentUKWide From(ByCountryCost record) =>
        new ()
        {
            Name                           = CalcResultCommsCostBuilder.TwoBCommsCostUkWide,
            EnglandCommsCostUKWide         = FormatUtils.FormatCurrency(record.England        , 2, useGrouping: true),
            WalesCommsCostUKWide           = FormatUtils.FormatCurrency(record.Wales          , 2, useGrouping: true),
            ScotlandCommsCostUKWide        = FormatUtils.FormatCurrency(record.Scotland       , 2, useGrouping: true),
            NorthernIrelandCommsCostUKWide = FormatUtils.FormatCurrency(record.NorthernIreland, 2, useGrouping: true),
            TotalCommsCostUKWide           = FormatUtils.FormatCurrency(record.Total          , 2, useGrouping: true)
         };
}
