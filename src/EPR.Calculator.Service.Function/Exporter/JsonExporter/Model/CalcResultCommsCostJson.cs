using System.Text.Json.Serialization;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalcResultCommsCostJson
{
    [JsonPropertyName(CommonConstants.OnePlusFourCommsCostApportionmentPercentages)]
    public required OnePlusFourCommsCostApportionmentPercentages OnePlusFourCommsCostApportionmentPercentages { get; set; }

    public static CalcResultCommsCostJson From(CalcResultCommsCost calcResultCommsCost)
    {
        return new CalcResultCommsCostJson {
            OnePlusFourCommsCostApportionmentPercentages = OnePlusFourCommsCostApportionmentPercentages.From(calcResultCommsCost.OnePlusFourApportionment)
        };
    }
}

public class OnePlusFourCommsCostApportionmentPercentages
{
    [JsonPropertyName("england")]
    public string? England { get; set; }

    [JsonPropertyName("wales")]
    public string? Wales { get; set; }

    [JsonPropertyName("scotland")]
    public string? Scotland { get; set; }

    [JsonPropertyName("northernIreland")]
    public string? NorthernIreland { get; set; }

    [JsonPropertyName("total")]
    public string? Total { get; set; }

    public static OnePlusFourCommsCostApportionmentPercentages From(ByCountryApportionment dataRow)
    {
        return new OnePlusFourCommsCostApportionmentPercentages
        {
            England         = FormatUtils.FormatPercentage(dataRow.England        ),
            Wales           = FormatUtils.FormatPercentage(dataRow.Wales          ),
            Scotland        = FormatUtils.FormatPercentage(dataRow.Scotland       ),
            NorthernIreland = FormatUtils.FormatPercentage(dataRow.NorthernIreland),
            Total           = FormatUtils.FormatPercentage(dataRow.Total          )
        };
    }
}
