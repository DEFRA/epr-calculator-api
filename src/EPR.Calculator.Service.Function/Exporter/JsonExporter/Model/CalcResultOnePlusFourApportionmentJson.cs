using System.Text.Json.Serialization;
using EPR.Calculator.API.Data.DataModels;
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
                    England         = FormatUtils.FormatCurrency(calcResultOnePlusFourApportionment.LaDisposalCost.England        , 2),
                    Wales           = FormatUtils.FormatCurrency(calcResultOnePlusFourApportionment.LaDisposalCost.Wales          , 2),
                    Scotland        = FormatUtils.FormatCurrency(calcResultOnePlusFourApportionment.LaDisposalCost.Scotland       , 2),
                    NorthernIreland = FormatUtils.FormatCurrency(calcResultOnePlusFourApportionment.LaDisposalCost.NorthernIreland, 2),
                    Total           = FormatUtils.FormatCurrency(calcResultOnePlusFourApportionment.LaDisposalCost.Total          , 2, useGrouping: true)
                },

                FourLADataPrepCharge = new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England         = FormatUtils.FormatCurrency(calcResultOnePlusFourApportionment.LADataPrepCharge.England        , 2),
                    Wales           = FormatUtils.FormatCurrency(calcResultOnePlusFourApportionment.LADataPrepCharge.Wales          , 2),
                    Scotland        = FormatUtils.FormatCurrency(calcResultOnePlusFourApportionment.LADataPrepCharge.Scotland       , 2),
                    NorthernIreland = FormatUtils.FormatCurrency(calcResultOnePlusFourApportionment.LADataPrepCharge.NorthernIreland, 2),
                    Total           = FormatUtils.FormatCurrency(calcResultOnePlusFourApportionment.LADataPrepCharge.Total          , 2, useGrouping: true)
                },

                TotalOfonePlusFour = new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England         = FormatUtils.FormatCurrency(calcResultOnePlusFourApportionment.TotalOnePlusFour.England        , 2),
                    Wales           = FormatUtils.FormatCurrency(calcResultOnePlusFourApportionment.TotalOnePlusFour.Wales          , 2),
                    Scotland        = FormatUtils.FormatCurrency(calcResultOnePlusFourApportionment.TotalOnePlusFour.Scotland       , 2),
                    NorthernIreland = FormatUtils.FormatCurrency(calcResultOnePlusFourApportionment.TotalOnePlusFour.NorthernIreland, 2),
                    Total           = FormatUtils.FormatCurrency(calcResultOnePlusFourApportionment.TotalOnePlusFour.Total          , 2, useGrouping: true)
                },

                OnePlusFourApportionmentPercentages = new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England         = FormatUtils.FormatPercentage(calcResultOnePlusFourApportionment.OnePlusFourApportionment.England        ),
                    Wales           = FormatUtils.FormatPercentage(calcResultOnePlusFourApportionment.OnePlusFourApportionment.Wales          ),
                    Scotland        = FormatUtils.FormatPercentage(calcResultOnePlusFourApportionment.OnePlusFourApportionment.Scotland       ),
                    NorthernIreland = FormatUtils.FormatPercentage(calcResultOnePlusFourApportionment.OnePlusFourApportionment.NorthernIreland),
                    Total           = FormatUtils.FormatPercentage(calcResultOnePlusFourApportionment.OnePlusFourApportionment.Total          )
                }
            };
        }
    }
}
