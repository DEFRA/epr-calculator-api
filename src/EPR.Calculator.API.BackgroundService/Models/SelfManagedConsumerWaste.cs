using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Models;

public record SelfManagedConsumerWaste
{
    public required int CalculatorRunId { get; init; }
    public required IReadOnlyDictionary<string, SelfManagedConsumerWasteData> TotalByMaterial { get; init; }
    public IReadOnlyList<ProducerSelfManagedConsumerWaste> ProducerTotals { get; init; } = [];
}

public record ProducerSelfManagedConsumerWaste
{
    public int ProducerId { get; init; }
    public string? SubsidiaryId { get; init; }
    public required int Level { get; init; }
    public required IReadOnlyDictionary<string, SelfManagedConsumerWasteData> SmcwByMaterial { get; init; }
}

public record SelfManagedConsumerWasteData
{
    public required decimal SmcwTonnage { get; init; }
    public required RamTonnageGroup ActionedSmcwTonnage { get; init; }
    public required decimal? ResidualSmcwTonnage { get; init; }
    public required RamTonnageGroup NetTonnage { get; init; }

    public static SelfManagedConsumerWasteData Zero => new()
    {
        SmcwTonnage         = 0,
        ActionedSmcwTonnage = RamTonnageGroup.Zero,
        ResidualSmcwTonnage = 0,
        NetTonnage          = RamTonnageGroup.Zero
    };

    public static SelfManagedConsumerWasteData operator +(
        SelfManagedConsumerWasteData a,
        SelfManagedConsumerWasteData b)
    {
        return new SelfManagedConsumerWasteData
        {
            SmcwTonnage =
                a.SmcwTonnage + b.SmcwTonnage,

            ActionedSmcwTonnage = new RamTonnageGroup {
                Total = (a.ActionedSmcwTonnage.Total ?? 0) + (b.ActionedSmcwTonnage.Total ?? 0),
                Red = (a.ActionedSmcwTonnage.Red   ?? 0) + (b.ActionedSmcwTonnage.Red   ?? 0),
                Amber = (a.ActionedSmcwTonnage.Amber ?? 0) + (b.ActionedSmcwTonnage.Amber ?? 0),
                Green = (a.ActionedSmcwTonnage.Green ?? 0) + (b.ActionedSmcwTonnage.Green ?? 0)
            },

            ResidualSmcwTonnage =
                (a.ResidualSmcwTonnage ?? 0) +
                (b.ResidualSmcwTonnage ?? 0),

            NetTonnage =  new RamTonnageGroup {
                Total = (a.NetTonnage.Total ?? 0) + (b.NetTonnage.Total ?? 0),
                Red = (a.NetTonnage.Red   ?? 0) + (b.NetTonnage.Red   ?? 0),
                Amber = (a.NetTonnage.Amber ?? 0) + (b.NetTonnage.Amber ?? 0),
                Green = (a.NetTonnage.Green ?? 0) + (b.NetTonnage.Green ?? 0)
            }
        };
    }
}

public static class SelfManagedConsumerWasteDataExtensions
{
    public static SelfManagedConsumerWasteData Sum(this IEnumerable<SelfManagedConsumerWasteData?> source)
    {
        return source.Aggregate(
            SelfManagedConsumerWasteData.Zero,
            (acc, x) => acc + (x ?? SelfManagedConsumerWasteData.Zero)
        );
    }
}