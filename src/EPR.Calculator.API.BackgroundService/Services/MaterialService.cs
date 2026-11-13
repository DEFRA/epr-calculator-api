using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
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
        // AsNoTracking() not needed: projects to MaterialDetail.
        return await dbContext
            .Material
            .OrderBy(m => m.DisplayOrder)
            .Select(m => new MaterialDetail
            {
                Id = m.Id,
                Code = m.Code,
                Name = m.Name
            })
            .ToImmutableListAsync();
    }
}
