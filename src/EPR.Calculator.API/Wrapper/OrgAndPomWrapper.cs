using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Wrapper
{
    public class OrgAndPomWrapper : IOrgAndPomWrapper
    {
        private readonly ApplicationDBContext context;
        public OrgAndPomWrapper(ApplicationDBContext context) { this.context = context; }

        public bool AnyOrganisationData()
        {
            return this.context.OrganisationData.Any();
        }

        public bool AnyPomData()
        {
            return this.context.PomData.Any();
        }

        public async Task<int> ExecuteSqlAsync(FormattableString sql) => await this.context.Database.ExecuteSqlAsync(sql);

        public async Task<IEnumerable<OrganisationData>> GetOrganisationDataAsync()
            => await this.context.OrganisationData.ToListAsync();

        public async Task<IEnumerable<PomData>> GetPomDataAsync()
            => await this.context.PomData.ToListAsync();
    }
}
