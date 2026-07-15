using System.ComponentModel.DataAnnotations.Schema;

namespace EPR.Calculator.API.Data.DataModels;

public record SelfManagedConsumerWaste
{
    public int Id { get; init; }

    public required int CalculatorRunId { get; init; }

    public required List<ProducerSelfManagedConsumerWaste> ProducerTotals { get; init; }
    
    internal ICollection<MaterialSelfManagedConsumerWasteData> MaterialTotals { get; set; } = [];

    [NotMapped]
    public required IReadOnlyDictionary<string, SelfManagedConsumerWasteData> TotalByMaterial
    {
        get => MaterialTotals.ToDictionary(s => s.MaterialCode, v => v.Smcw);
        init => MaterialTotals = [..value.Select(kvp =>
            new MaterialSelfManagedConsumerWasteData { MaterialCode = kvp.Key, Smcw = kvp.Value }
        )];
    }
}

public record ProducerSelfManagedConsumerWaste
{
    public int ProducerId { get; set; }
    public string? SubsidiaryId { get; set; }
    public required int Level {get; init; }

    internal ICollection<MaterialSelfManagedConsumerWasteData> SmcwMaterialData { get; set; } = [];

    [NotMapped]
    public required IReadOnlyDictionary<string, SelfManagedConsumerWasteData> SmcwByMaterial
    {
        get => SmcwMaterialData.ToDictionary(s => s.MaterialCode, v => v.Smcw);
        init => SmcwMaterialData = [..value.Select(kvp =>
            new MaterialSelfManagedConsumerWasteData { MaterialCode = kvp.Key, Smcw = kvp.Value }
        )];
    }
}

internal record MaterialSelfManagedConsumerWasteData
{
    public required string MaterialCode { get; init; }
    public required SelfManagedConsumerWasteData Smcw { get; init; }
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