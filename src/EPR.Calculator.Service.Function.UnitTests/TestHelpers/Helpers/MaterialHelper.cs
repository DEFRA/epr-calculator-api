using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Helpers;

public static class MaterialHelper
{
    public static IImmutableList<Material> GetMaterials() =>
    [
        new()  { Id = 1, Code = "AL", Name = "Aluminium"       },
        new()  { Id = 2, Code = "FC", Name = "Fibre composite" },
        new()  { Id = 3, Code = "GL", Name = "Glass"           },
        new()  { Id = 4, Code = "PC", Name = "Paper or card"   },
        new()  { Id = 5, Code = "PL", Name = "Plastic"         },
        new()  { Id = 6, Code = "ST", Name = "Steel"           },
        new()  { Id = 7, Code = "WD", Name = "Wood"            },
        new()  { Id = 8, Code = "OT", Name = "Other materials" }
    ];

    public static ImmutableList<MaterialDetail> ToDetails(this IEnumerable<Material> materials)
    {
        return materials
            .Select(m => new MaterialDetail { Id = m.Id, Name = m.Name, Code = m.Code })
            .ToImmutableList();
    }
}
