using EPR.CommonDataService.DataApi.CommonDataApi.Entities;
using EPR.CommonDataService.DataApi.CommonDataApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EPR.CommonDataService.DataApi.CommonDataApi;

public interface IStreamOrganisationsRequestHandler
{
    IAsyncEnumerable<PayCalOrganisation> Handle(int relativeYear, DateTimeOffset? cutOffDate);
}

public sealed class StreamOrganisationsRequestHandler(SynapseContext dbContext)
    : IStreamOrganisationsRequestHandler
{
    public IAsyncEnumerable<PayCalOrganisation> Handle(int relativeYear, DateTimeOffset? cutOffDate)
    {
        return dbContext
            .PayCalOrganisations
            .FromSqlInterpolated($"EXEC [dbo].[sp_GetPaycalOrgData] @RelativeYear={relativeYear}, @CutOffDate={cutOffDate}")
            .AsNoTracking()
            .WithTimeout(TimeSpan.FromMinutes(10)) // Necessary due to poor db performance
            .AsAsyncEnumerable();
    }
}
