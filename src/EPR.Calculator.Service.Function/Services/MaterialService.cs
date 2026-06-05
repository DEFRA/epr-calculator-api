using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Services;

public interface IMaterialService
{
    public Task<IImmutableList<MaterialDetail>> GetMaterials();
}

public class MaterialService(ApplicationDBContext dbContext)
    : IMaterialService
{
    public async Task<IImmutableList<MaterialDetail>> GetMaterials()
    {
        return await dbContext
            .Material
            .Select(m => new MaterialDetail
            {
                Id = m.Id,
                Code = m.Code,
                Name = m.Name
            })
            .ToImmutableListAsync();
    }
}
