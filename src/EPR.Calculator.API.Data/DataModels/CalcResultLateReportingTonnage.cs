using System.ComponentModel.DataAnnotations.Schema;

namespace EPR.Calculator.API.Data.DataModels;

public record CalcResultLateReportingTonnageEntry
{
    public int Id { get; set; }
    public required int CalculatorRunId { get; set; }
    public required CalcResultLateReportingTonnage LateReportingTonnage { get; set; }

}

public record CalcResultLateReportingTonnage
{
    internal ICollection<MaterialLateReportingTonnageData> MaterialTonnages { get; set; } = [];

    [NotMapped]
    public required IReadOnlyDictionary<string, CalcResultLateReportingTonnageDetail> ByMaterial
    {
        get => MaterialTonnages.ToDictionary(x => x.MaterialCode, x => x.Detail);
        init => MaterialTonnages = [..value.Select(kvp =>
            new MaterialLateReportingTonnageData { MaterialCode = kvp.Key, Detail = kvp.Value })];
    }

    public required CalcResultLateReportingTonnageDetail Total { get; init; }
}

internal record MaterialLateReportingTonnageData
{
    public required string MaterialCode { get; set; }
    public required CalcResultLateReportingTonnageDetail Detail { get; set; }
}

public record CalcResultLateReportingTonnageDetail
{
    required public decimal Total { get; init; }

    required public decimal Red { get; init; }

    required public decimal Amber { get; init; }

    required public decimal Green { get; init; }
}
