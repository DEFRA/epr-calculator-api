using System.Text.Json.Serialization;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Converter;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalcResultLaDisposalCostDataJson
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("calcResultLaDisposalCostDetails")]
    public required IEnumerable<CalcResultLaDisposalCostDetailsJson> CalcResultLaDisposalCostDetails { get; set; }

    [JsonPropertyName("calcResultLaDisposalCostDataDetailsTotal")]
    public required CalcResultLaDisposalCostDataDetailsTotal CalcResultLaDisposalCostDataDetailsTotal { get; set; }

    public static CalcResultLaDisposalCostDataJson From(
        IReadOnlyDictionary<string, CalcResultLaDisposalCostDataDetail> detailsByMaterial,
        CalcResultLaDisposalCostDataDetail total,
        IImmutableList<MaterialDetail> materials
    )
    {
        return new CalcResultLaDisposalCostDataJson
        {
            Name = CommonConstants.LADisposalCostData,
            CalcResultLaDisposalCostDetails =
                detailsByMaterial.Select(item =>
                {
                    var material = materials.First(m => m.Code == item.Key);
                    return CalcResultLaDisposalCostDetailsJson.From(material, item.Value);
                }).ToList(),
            CalcResultLaDisposalCostDataDetailsTotal = CalcResultLaDisposalCostDataDetailsTotal.From(total)
        };
    }
}

public class CalcResultLaDisposalCostDetailsJson : BaseLaDisposalcostAnd2ACommsData
{
    [JsonPropertyName("materialName")]
    public required string MaterialName { get; init; }

    [JsonPropertyName("englandLaDisposalCost")]
    public required string EnglandLaDisposalCost { get; init; }

    [JsonPropertyName("walesLaDisposalCost")]
    public required string WalesLaDisposalCost { get; init; }

    [JsonPropertyName("scotlandLaDisposalCost")]
    public required string ScotlandLaDisposalCost { get; init; }

    [JsonPropertyName("northernIrelandLaDisposalCost")]
    public required string NorthernIrelandLaDisposalCost { get; init; }

    [JsonPropertyName("totalLaDisposalCost")]
    public required string TotalLaDisposalCost { get; init; }

    [JsonPropertyName("disposalCostPricePerTonne")]
    public required string? DisposalCostPricePerTonne { get; init; }

    public static CalcResultLaDisposalCostDetailsJson From(MaterialDetail material, CalcResultLaDisposalCostDataDetail item) =>
        new ()
        {
            MaterialName                           = material.Name,
            EnglandLaDisposalCost                  = FormatUtils.FormatCurrency(item.Cost.England        , 2, useGrouping: true),
            WalesLaDisposalCost                    = FormatUtils.FormatCurrency(item.Cost.Wales          , 2, useGrouping: true),
            ScotlandLaDisposalCost                 = FormatUtils.FormatCurrency(item.Cost.Scotland       , 2, useGrouping: true),
            NorthernIrelandLaDisposalCost          = FormatUtils.FormatCurrency(item.Cost.NorthernIreland, 2, useGrouping: true),
            TotalLaDisposalCost                    = FormatUtils.FormatCurrency(item.Cost.Total          , 2, useGrouping: true),
            ProducerHouseholdPackagingWasteTonnage = item.HouseholdPackagingWasteTonnage,
            PublicBinTonnage                       = item.PublicBinTonnage,
            HouseholdDrinksContainersTonnage       = item.HouseholdDrinkContainersTonnage,
            LateReportingTonnage                   = item.LateReportingTonnage,
            TotalTonnage                           = item.TotalTonnage,
            DisposalCostPricePerTonne              = FormatUtils.FormatCurrency(item.DisposalCostPricePerTonne == null ? 0 : item.DisposalCostPricePerTonne.Value, 4, useGrouping: true)
        };
}

public class CalcResultLaDisposalCostDataDetailsTotal
{
    [JsonPropertyName("total")]
    public required string Total { get; init; }

    [JsonPropertyName("englandLaDisposalCostTotal")]
    public required string EnglandLaDisposalCostTotal { get; init; }

    [JsonPropertyName("walesLaDisposalCostTotal")]
    public required string WalesLaDisposalCostTotal { get; init; }

    [JsonPropertyName("scotlandLaDisposalCostTotal")]
    public required string ScotlandLaDisposalCostTotal { get; init; }

    [JsonPropertyName("northernIrelandLaDisposalCostTotal")]
    public required string NorthernIrelandLaDisposalCostTotal { get; init; }

    [JsonPropertyName("totalLaDisposalCostTotal")]
    public required string TotalLaDisposalCostTotal { get; init; }

    [JsonPropertyName("producerHouseholdPackagingWasteTonnageTotal")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public required decimal ProducerHouseholdPackagingWasteTonnageTotal { get; init; }

    [JsonPropertyName("publicBinTonnage")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public required decimal PublicBinTonnage { get; init; }

    [JsonPropertyName("householdDrinksContainersTonnageTotal")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public required decimal HouseholdDrinksContainersTonnageTotal { get; init; }

    [JsonPropertyName("lateReportingTonnageTotal")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public required decimal LateReportingTonnageTotal { get; init; }

    [JsonPropertyName("totalTonnageTotal")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public required decimal? TotalTonnageTotal { get; init; }

    public static CalcResultLaDisposalCostDataDetailsTotal From(CalcResultLaDisposalCostDataDetail total) =>
        new ()
        {
            Total                                       = "Total",
            EnglandLaDisposalCostTotal                  = FormatUtils.FormatCurrency(total.Cost.England        , 2, useGrouping: true),
            WalesLaDisposalCostTotal                    = FormatUtils.FormatCurrency(total.Cost.Wales          , 2, useGrouping: true),
            ScotlandLaDisposalCostTotal                 = FormatUtils.FormatCurrency(total.Cost.Scotland       , 2, useGrouping: true),
            NorthernIrelandLaDisposalCostTotal          = FormatUtils.FormatCurrency(total.Cost.NorthernIreland, 2, useGrouping: true),
            TotalLaDisposalCostTotal                    = FormatUtils.FormatCurrency(total.Cost.Total          , 2, useGrouping: true),
            ProducerHouseholdPackagingWasteTonnageTotal = total.HouseholdPackagingWasteTonnage,
            PublicBinTonnage                            = total.PublicBinTonnage,
            HouseholdDrinksContainersTonnageTotal       = total.HouseholdDrinkContainersTonnage,
            LateReportingTonnageTotal                   = total.LateReportingTonnage,
            TotalTonnageTotal                           = total.TotalTonnage
        };
}
