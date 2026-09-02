using EPR.CommonDataService.DataApi.CommonDataApi.Entities;
using EPR.CommonDataService.DataApi.CommonDataApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EPR.CommonDataService.DataApi.CommonDataApi;

public interface IStreamPomsRequestHandler
{
    IAsyncEnumerable<PayCalPom> Handle(int relativeYear, DateTimeOffset? cutOffDate);
}

public sealed class StreamPomsRequestHandler(SynapseContext dbContext)
    : IStreamPomsRequestHandler
{
    public IAsyncEnumerable<PayCalPom> Handle(int relativeYear, DateTimeOffset? cutOffDate)
    {
        return dbContext
            .PayCalPoms
            .FromSqlInterpolated($"EXEC [dbo].[sp_GetPaycalPomData] @RelativeYear={relativeYear}, @CutOffDate={cutOffDate}")
            .AsNoTracking()
            .WithTimeout(TimeSpan.FromMinutes(10)) // Necessary due to poor db performance
            .AsAsyncEnumerable();
    }
}
