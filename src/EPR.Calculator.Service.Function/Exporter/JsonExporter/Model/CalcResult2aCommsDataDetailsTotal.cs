using System.Text.Json.Serialization;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public record CalcResult2ACommsDataDetailsTotal
{
    [JsonPropertyName("total")]

    public required string Total { get; init; }

    [JsonPropertyName("englandCommsCostTotal")]

    public required string EnglandCommsCostTotal { get; init; }

    [JsonPropertyName("walesCommsCostTotal")]

    public required string WalesCommsCostTotal { get; init; }

    [JsonPropertyName("scotlandCommsCostTotal")]

    public required string ScotlandCommsCostTotal { get; init; }

    [JsonPropertyName("northernIrelandCommsCostTotal")]

    public required string NorthernIrelandCommsCostTotal { get; init; }

    [JsonPropertyName("totalCommsCostTotal")]

    public required string TotalCommsCostTotal { get; init; }

    [JsonPropertyName("producerHouseholdPackagingWasteTonnageTotal")]

    public required decimal HouseholdPackagingWasteTonnageTotal { get; init; }

    [JsonPropertyName("publicBinTonnage")]

    public required decimal PublicBinTonnageTotal { get; init; }

    [JsonPropertyName("householdDrinksContainersTonnageTotal")]

    public required decimal HouseholdDrinksContainersTonnageTotal { get; init; }

    [JsonPropertyName("lateReportingTonnageTotal")]

    public required decimal LateReportingTonnageTotal { get; init; }

    [JsonPropertyName("totalTonnageTotal")]

    public required decimal TotalTonnageTotal { get; init; }

    public static CalcResult2ACommsDataDetailsTotal From(CalcResultCommsCostCommsCostByMaterial commsCostByMaterial)
    {
        return new CalcResult2ACommsDataDetailsTotal
        {
            Total                                 = "Total",
            EnglandCommsCostTotal                 = FormatUtils.FormatCurrency(commsCostByMaterial.Cost.England),
            WalesCommsCostTotal                   = FormatUtils.FormatCurrency(commsCostByMaterial.Cost.Wales),
            ScotlandCommsCostTotal                = FormatUtils.FormatCurrency(commsCostByMaterial.Cost.Scotland),
            NorthernIrelandCommsCostTotal         = FormatUtils.FormatCurrency(commsCostByMaterial.Cost.NorthernIreland),
            TotalCommsCostTotal                   = FormatUtils.FormatCurrency(commsCostByMaterial.Cost.Total),
            HouseholdPackagingWasteTonnageTotal   = MathUtils.RoundAwayFromZero(commsCostByMaterial.HouseholdPackagingWasteTonnage  , 3),
            PublicBinTonnageTotal                 = MathUtils.RoundAwayFromZero(commsCostByMaterial.PublicBinTonnage                , 3),
            HouseholdDrinksContainersTonnageTotal = MathUtils.RoundAwayFromZero(commsCostByMaterial.HouseholdDrinksContainersTonnage, 3),
            LateReportingTonnageTotal             = MathUtils.RoundAwayFromZero(commsCostByMaterial.LateReportingTonnage            , 3),
            TotalTonnageTotal                     = MathUtils.RoundAwayFromZero(commsCostByMaterial.TotalTonnage                    , 3)
        };
    }
}
