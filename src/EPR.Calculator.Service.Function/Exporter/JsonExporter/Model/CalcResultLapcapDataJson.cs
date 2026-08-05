using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

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
            EnglandLaDisposalCost         = FormatUtils.FormatCurrency(record.England        , 2, useGrouping: true),
            WalesLaDisposalCost           = FormatUtils.FormatCurrency(record.Wales          , 2, useGrouping: true),
            ScotlandLaDisposalCost        = FormatUtils.FormatCurrency(record.Scotland       , 2, useGrouping: true),
            NorthernIrelandLaDisposalCost = FormatUtils.FormatCurrency(record.NorthernIreland, 2, useGrouping: true),
            OneLaDisposalCostTotal        = FormatUtils.FormatCurrency(record.Total          , 2, useGrouping: true)
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
            TotalEnglandLaDisposalCost         = FormatUtils.FormatCurrency(record.England        , 2, useGrouping: true),
            TotalWalesLaDisposalCost           = FormatUtils.FormatCurrency(record.Wales          , 2, useGrouping: true),
            TotalScotlandLaDisposalCost        = FormatUtils.FormatCurrency(record.Scotland       , 2, useGrouping: true),
            TotalNorthernIrelandLaDisposalCost = FormatUtils.FormatCurrency(record.NorthernIreland, 2, useGrouping: true),
            TotalLaDisposalCost                = FormatUtils.FormatCurrency(record.Total          , 2, useGrouping: true)
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
            EnglandApportionment         = FormatUtils.FormatPercentage(record.England        ),
            WalesApportionment           = FormatUtils.FormatPercentage(record.Wales          ),
            ScotlandApportionment        = FormatUtils.FormatPercentage(record.Scotland       ),
            NorthernIrelandApportionment = FormatUtils.FormatPercentage(record.NorthernIreland),
            TotalApportionment           = FormatUtils.FormatPercentage(100                   )
        };
    }
}
