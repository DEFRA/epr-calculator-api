using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Helpers;

public static class MaterialHelpers
{
    public static ImmutableList<MaterialDetail> ToDetails(this IEnumerable<Material> materials)
    {
        return materials
            .Select(m => new MaterialDetail { Id = m.Id, Name = m.Name, Code = m.Code })
            .ToImmutableList();
    }
}
