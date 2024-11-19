using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Mappers
{
    public static class MaterialMapper
    {
        public static List<MaterialDetail> Map(IEnumerable<Material> materialsInDb)
        {
            var result = new List<MaterialDetail>();

            foreach (var material in materialsInDb)
            {
                result.Add(new MaterialDetail
                {
                    Code = material.Code,
                    Name = material.Name,
                    Description = material.Description ?? string.Empty
                });
            }

            return result;
        }
    }
}
