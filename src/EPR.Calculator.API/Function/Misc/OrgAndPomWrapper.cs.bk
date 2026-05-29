using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Misc
{
    public interface IOrgAndPomWrapper
    {
        bool AnyOrganisationData();
        bool AnyPomData();
        Task<IEnumerable<OrganisationData>> GetOrganisationDataAsync();
        Task<IEnumerable<PomData>> GetPomDataAsync();
        Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken);
    }

    public class OrgAndPomWrapper : IOrgAndPomWrapper
    {
        private readonly ApplicationDBContext context;

        public OrgAndPomWrapper(ApplicationDBContext context)
        {
            this.context = context;
        }

        public bool AnyOrganisationData()
        {
            return context.OrganisationData.Any();
        }

        public bool AnyPomData()
        {
            return context.PomData.Any();
        }

        public async Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken)
            => await context.Database.ExecuteSqlAsync(sql, cancellationToken);

        public async Task<IEnumerable<OrganisationData>> GetOrganisationDataAsync()
            => await context.OrganisationData.ToListAsync();

        public async Task<IEnumerable<PomData>> GetPomDataAsync()
            => await context.PomData.ToListAsync();
    }
}
