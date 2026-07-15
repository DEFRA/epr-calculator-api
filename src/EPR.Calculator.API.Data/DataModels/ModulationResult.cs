using System.ComponentModel.DataAnnotations.Schema;

namespace EPR.Calculator.API.Data.DataModels;

public record ModulationResult
{
    public int Id { get; init; }
    public required int CalculatorRunId { get; init; }
    public required decimal GreenFactor { get; init; }
    public required decimal RedFactor { get; init; }

    internal ICollection<MaterialModulation> MaterialModulations { get; set; } = [];

    [NotMapped]
    public required IReadOnlyDictionary<MaterialDetail, ModulationDetail> ModulationByMaterial
    {
        get => MaterialModulations.ToDictionary(s => s.MaterialDetail, v => v.ModulationDetail);
        init => MaterialModulations = [..value.Select(kvp =>
            new MaterialModulation{ MaterialDetail = kvp.Key, ModulationDetail = kvp.Value }
        )];
    }
}

internal record MaterialModulation
{
    public required MaterialDetail MaterialDetail { get; init; }
    public required ModulationDetail ModulationDetail { get; init; }
}

public record ModulationDetail
{ 
    public required decimal RedMaterialDisposalCost { get; init; }
    public required decimal AmberMaterialDisposalCost { get; init; }
    public required decimal GreenMaterialDisposalCost { get; init; }
    public required decimal RedMaterialTonnages { get; init; }
    public required decimal AmberMaterialTonnages { get; init; }
    public required decimal GreenMaterialTonnages { get; init; }
    public required decimal TotalRedMaterialAtAmberDisposalCost { get; init; }
    public required decimal TotalGreenMaterialAtAmberDisposalCost { get; init; }
}