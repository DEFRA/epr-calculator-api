using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;
using EPR.Calculator.Service.Function.Builder.CommsCost;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
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
                EnglandCommsCostByCountry         = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.England        , 2, ","),
                WalesCommsCostByCountry           = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.Wales          , 2, ","),
                ScotlandCommsCostByCountry        = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.Scotland       , 2, ","),
                NorthernIrelandCommsCostByCountry = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.NorthernIreland, 2, ","),
                TotalCommsCostByCountry           = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.Total          , 2, ",")
            };
        }
    }
}
