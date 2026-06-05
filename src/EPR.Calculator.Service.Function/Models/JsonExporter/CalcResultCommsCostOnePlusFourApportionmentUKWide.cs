using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
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

        public static CalcResultCommsCostOnePlusFourApportionmentUKWide? From(ByCountryCost? record)
        {
            if (record == null)
            {
                return null;
            }

            return new CalcResultCommsCostOnePlusFourApportionmentUKWide
            {
                Name                           = CalcResultCommsCostBuilder.TwoBCommsCostUkWide,
                EnglandCommsCostUKWide         = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.England        , 2, ","),
                WalesCommsCostUKWide           = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.Wales          , 2, ","),
                ScotlandCommsCostUKWide        = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.Scotland       , 2, ","),
                NorthernIrelandCommsCostUKWide = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.NorthernIreland, 2, ","),
                TotalCommsCostUKWide           = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.Total          , 2, ",")
             };
         }
    }
}
