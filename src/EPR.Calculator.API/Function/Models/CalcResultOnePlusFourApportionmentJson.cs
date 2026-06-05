using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultOnePlusFourApportionmentJson
    {
        [JsonPropertyName("oneFeeForLADisposalCosts")]
        public CalcResultOnePlusFourApportionmentDetailJson OneFeeForLADisposalCosts { get; set; } = new CalcResultOnePlusFourApportionmentDetailJson();

        [JsonPropertyName("fourLADataPrepCharge")]
        public CalcResultOnePlusFourApportionmentDetailJson FourLADataPrepCharge { get; set; } = new CalcResultOnePlusFourApportionmentDetailJson();

        [JsonPropertyName("totalOfonePlusFour")]
        public CalcResultOnePlusFourApportionmentDetailJson TotalOfonePlusFour { get; set; } = new CalcResultOnePlusFourApportionmentDetailJson();

        [JsonPropertyName("onePlusFourApportionmentPercentages")]
        public CalcResultOnePlusFourApportionmentDetailJson OnePlusFourApportionmentPercentages { get; set; } = new CalcResultOnePlusFourApportionmentDetailJson();

        public static CalcResultOnePlusFourApportionmentJson From(CalcResultOnePlusFourApportionment calcResultOnePlusFourApportionment)
        {
            return new CalcResultOnePlusFourApportionmentJson
            {
                // TODO can we format Totals consistently (e.g. without `,` separator)
                OneFeeForLADisposalCosts = new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England         = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(calcResultOnePlusFourApportionment.LaDisposalCost.England        , 2, ""),
                    Wales           = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(calcResultOnePlusFourApportionment.LaDisposalCost.Wales          , 2, ""),
                    Scotland        = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(calcResultOnePlusFourApportionment.LaDisposalCost.Scotland       , 2, ""),
                    NorthernIreland = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(calcResultOnePlusFourApportionment.LaDisposalCost.NorthernIreland, 2, ""),
                    Total           = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(calcResultOnePlusFourApportionment.LaDisposalCost.Total          , 2, ",")
                },

                FourLADataPrepCharge = new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England         = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(calcResultOnePlusFourApportionment.LADataPrepCharge.England        , 2, ""),
                    Wales           = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(calcResultOnePlusFourApportionment.LADataPrepCharge.Wales          , 2, ""),
                    Scotland        = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(calcResultOnePlusFourApportionment.LADataPrepCharge.Scotland       , 2, ""),
                    NorthernIreland = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(calcResultOnePlusFourApportionment.LADataPrepCharge.NorthernIreland, 2, ""),
                    Total           = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(calcResultOnePlusFourApportionment.LADataPrepCharge.Total          , 2, ",")
                },

                TotalOfonePlusFour = new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England         = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(calcResultOnePlusFourApportionment.TotalOnePlusFour.England        , 2, ""),
                    Wales           = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(calcResultOnePlusFourApportionment.TotalOnePlusFour.Wales          , 2, ""),
                    Scotland        = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(calcResultOnePlusFourApportionment.TotalOnePlusFour.Scotland       , 2, ""),
                    NorthernIreland = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(calcResultOnePlusFourApportionment.TotalOnePlusFour.NorthernIreland, 2, ""),
                    Total           = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(calcResultOnePlusFourApportionment.TotalOnePlusFour.Total          , 2, ",")
                },

                OnePlusFourApportionmentPercentages = new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England         = $"{calcResultOnePlusFourApportionment.OnePlusFourApportionment.England        :0.00000000}%",
                    Wales           = $"{calcResultOnePlusFourApportionment.OnePlusFourApportionment.Wales          :0.00000000}%",
                    Scotland        = $"{calcResultOnePlusFourApportionment.OnePlusFourApportionment.Scotland       :0.00000000}%",
                    NorthernIreland = $"{calcResultOnePlusFourApportionment.OnePlusFourApportionment.NorthernIreland:0.00000000}%",
                    Total           = $"{calcResultOnePlusFourApportionment.OnePlusFourApportionment.Total          :0.00000000}%"
                }
            };
        }
    }
}
