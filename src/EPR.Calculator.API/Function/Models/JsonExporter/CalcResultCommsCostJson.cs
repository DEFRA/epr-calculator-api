using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Constants;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
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
                England         = $"{dataRow.England        :0.00000000}%",
                Wales           = $"{dataRow.Wales          :0.00000000}%",
                Scotland        = $"{dataRow.Scotland       :0.00000000}%",
                NorthernIreland = $"{dataRow.NorthernIreland:0.00000000}%",
                Total           = $"{dataRow.Total          :0.00000000}%"
            };
        }
    }
}
