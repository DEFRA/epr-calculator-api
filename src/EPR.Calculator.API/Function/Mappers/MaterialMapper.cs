using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Mappers
{
    public static class MaterialMapper
    {
        public static IImmutableList<MaterialDetail> Map(IEnumerable<Material> materialsInDb)
        {
            var result = new List<MaterialDetail>();

            foreach (var material in materialsInDb)
            {
                result.Add(new MaterialDetail
                {
                    Id = material.Id,
                    Code = material.Code,
                    Name = material.Name,
                    Description = material.Description ?? string.Empty,
                });
            }

            return result.ToImmutableList();
        }
    }
}
