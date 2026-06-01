using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IMaterialService
    {
        public Task<IImmutableList<MaterialDetail>> GetMaterials();
    }

    public class MaterialService : IMaterialService
    {
        private readonly ApplicationDBContext context;

        public MaterialService(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<IImmutableList<MaterialDetail>> GetMaterials()
        {
            var materials = await context.Material.ToListAsync();
            return MaterialMapper.Map(materials).ToImmutableList();
        }
    }
}
