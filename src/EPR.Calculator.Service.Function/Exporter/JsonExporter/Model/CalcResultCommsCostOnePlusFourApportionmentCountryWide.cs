using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalcResultCommsCostOnePlusFourApportionmentCountryWide
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("englandCommsCostByCountry")]
    public required string EnglandCommsCostByCountry { get; set; }

    [JsonPropertyName("walesCommsCostByCountry")]
    public required string WalesCommsCostByCountry { get; set; }

    [JsonPropertyName("scotlandCommsCostByCountry")]
    public required string ScotlandCommsCostByCountry { get; set; }

    [JsonPropertyName("northernIrelandCommsCostByCountry")]
    public required string NorthernIrelandCommsCostByCountry { get; set; }

    [JsonPropertyName("totalCommsCostByCountry")]
    public required string TotalCommsCostByCountry { get; set; }

    public static CalcResultCommsCostOnePlusFourApportionmentCountryWide? From(ByCountryCost? record)
    {
        if (record == null)
        {
            return null;
        }

        return new CalcResultCommsCostOnePlusFourApportionmentCountryWide
        {
            Name                              = CalcResultCommsCostBuilder.TwoCCommsCostByCountry,
            EnglandCommsCostByCountry         = FormatUtils.FormatCurrency(record.England        , 2, useGrouping: true),
            WalesCommsCostByCountry           = FormatUtils.FormatCurrency(record.Wales          , 2, useGrouping: true),
            ScotlandCommsCostByCountry        = FormatUtils.FormatCurrency(record.Scotland       , 2, useGrouping: true),
            NorthernIrelandCommsCostByCountry = FormatUtils.FormatCurrency(record.NorthernIreland, 2, useGrouping: true),
            TotalCommsCostByCountry           = FormatUtils.FormatCurrency(record.Total          , 2, useGrouping: true)
        };
    }
}
