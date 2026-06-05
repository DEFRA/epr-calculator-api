using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using System.Globalization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Converter;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultLapcapDataJson
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("calcResultLapcapDataDetails")]
        public required IEnumerable<CalcResultLapcapDataDetailJson> CalcResultLapcapDataDetails { get; set; }

        [JsonPropertyName("calcResultLapcapDataTotal")]
        public CalcResultLapcapDataDetailTotalJson? CalcResultLapcapDataTotal { get; set; }

        [JsonPropertyName("oneCountryApportionmentPercentages")]
        public CalcResultLapcapDataDetailApportionmentJson? OneCountryApportionmentPercentages { get; set; }

        public static CalcResultLapcapDataJson From(
            CalcResultLapcapData data,
            IImmutableList<MaterialDetail> materials
        )
        {
            return new CalcResultLapcapDataJson
            {
                Name = "LAPCAP Data",
                CalcResultLapcapDataDetails        = data.ByMaterial.Select(detail => {
                    var material = materials.First(m => m.Code == detail.Key);
                    return CalcResultLapcapDataDetailJson.From(material, detail.Value);
                }),
                CalcResultLapcapDataTotal          = CalcResultLapcapDataDetailTotalJson.From(data.Total),
                OneCountryApportionmentPercentages = CalcResultLapcapDataDetailApportionmentJson.From(data.CountryApportionment)
            };
        }
    }

    public class CalcResultLapcapDataDetailJson
    {
        [JsonPropertyName("materialName")]
        public required string MaterialName { get; set; }

        [JsonPropertyName("englandLaDisposalCost")]
        public required string EnglandLaDisposalCost { get; set; }

        [JsonPropertyName("walesLaDisposalCost")]
        public required string WalesLaDisposalCost { get; set; }

        [JsonPropertyName("scotlandLaDisposalCost")]
        public required string ScotlandLaDisposalCost { get; set; }

        [JsonPropertyName("northernIrelandLaDisposalCost")]
        public required string NorthernIrelandLaDisposalCost { get; set; }

        [JsonPropertyName("oneLaDisposalCostTotal")]
        public required string OneLaDisposalCostTotal { get; set; }

        public static CalcResultLapcapDataDetailJson From(MaterialDetail materialDetail, ByCountryCost record)
        {
            return new CalcResultLapcapDataDetailJson
            {
                MaterialName                  = materialDetail.Name,
                EnglandLaDisposalCost         = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.England        , 2, ","),
                WalesLaDisposalCost           = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.Wales          , 2, ","),
                ScotlandLaDisposalCost        = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.Scotland       , 2, ","),
                NorthernIrelandLaDisposalCost = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.NorthernIreland, 2, ","),
                OneLaDisposalCostTotal        = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.Total          , 2, ",")
            };
        }
    }

    public class CalcResultLapcapDataDetailTotalJson
    {
        [JsonPropertyName("totalEnglandLaDisposalCost")]
        public required string TotalEnglandLaDisposalCost { get; set; }

        [JsonPropertyName("totalWalesLaDisposalCost")]
        public required string TotalWalesLaDisposalCost { get; set; }

        [JsonPropertyName("totalScotlandLaDisposalCost")]
        public required string TotalScotlandLaDisposalCost { get; set; }

        [JsonPropertyName("totalNorthernIrelandLaDisposalCost")]
        public required string TotalNorthernIrelandLaDisposalCost { get; set; }

        [JsonPropertyName("totalLaDisposalCost")]
        public required string TotalLaDisposalCost { get; set; }

        public static CalcResultLapcapDataDetailTotalJson From(ByCountryCost record)
        {
            return new CalcResultLapcapDataDetailTotalJson
            {
                TotalEnglandLaDisposalCost         = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.England        , 2, ","),
                TotalWalesLaDisposalCost           = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.Wales          , 2, ","),
                TotalScotlandLaDisposalCost        = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.Scotland       , 2, ","),
                TotalNorthernIrelandLaDisposalCost = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.NorthernIreland, 2, ","),
                TotalLaDisposalCost                = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.Total          , 2, ",")
            };
        }
    }

    public class CalcResultLapcapDataDetailApportionmentJson
    {
        [JsonPropertyName("englandApportionment")]
        public required string EnglandApportionment { get; set; }

        [JsonPropertyName("walesApportionment")]
        public required string WalesApportionment { get; set; }

        [JsonPropertyName("scotlandApportionment")]
        public required string ScotlandApportionment { get; set; }

        [JsonPropertyName("northernIrelandApportionment")]
        public required string NorthernIrelandApportionment { get; set; }

        [JsonPropertyName("totalApportionment")]
        public required string TotalApportionment { get; set; }

        public static CalcResultLapcapDataDetailApportionmentJson From(ByCountryApportionment record)
        {
            return new CalcResultLapcapDataDetailApportionmentJson
            {
                EnglandApportionment         = $"{record.England        :0.00000000}%",
                WalesApportionment           = $"{record.Wales          :0.00000000}%",
                ScotlandApportionment        = $"{record.Scotland       :0.00000000}%",
                NorthernIrelandApportionment = $"{record.NorthernIreland:0.00000000}%",
                TotalApportionment           = $"{100                   :0.00000000}%"
            };
        }
    }
}
