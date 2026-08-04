using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Utils;
using EPR.Calculator.API.Data.Utils;

namespace EPR.Calculator.API.BackgroundService.Services;

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
