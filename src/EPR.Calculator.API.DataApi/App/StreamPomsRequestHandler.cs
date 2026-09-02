using System.Diagnostics.CodeAnalysis;
using EPR.CommonDataService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EPR.CommonDataService.DataApi.App;

public interface IStreamPomsRequestHandler
{
    IAsyncEnumerable<PomResponse> Handle(int relativeYear, DateTimeOffset? cutOffDate);
}

public sealed class StreamPomsRequestHandler(SynapseContext dbContext)
    : IStreamPomsRequestHandler
{
    public async IAsyncEnumerable<PomResponse> Handle(int relativeYear, DateTimeOffset? cutOffDate)
    {
        var poms = dbContext
            .PayCalPoms
            .FromSqlInterpolated($"EXEC [dbo].[sp_GetPaycalPomData] @RelativeYear={relativeYear}, @CutOffDate={cutOffDate}")
            .AsNoTracking()
            .WithTimeout(TimeSpan.FromMinutes(10)) // Necessary due to poor db performance
            .AsAsyncEnumerable();

        await foreach (var pom in poms)
            yield return new PomResponse
            {
                SubmissionPeriod = pom.SubmissionPeriod!,
                SubmissionPeriodDescription = pom.SubmissionPeriodDescription,
                OrganisationId = pom.OrganisationId!.Value,
                SubsidiaryId = pom.SubsidiaryId,
                PackagingType = pom.PackagingType,
                PackagingMaterial = pom.PackagingMaterial,
                PackagingMaterialSubtype = pom.PackagingMaterialSubtype,
                PackagingMaterialWeight = pom.PackagingMaterialWeight,
                PackagingClass = pom.PackagingClass,
                PackagingActivity = pom.PackagingActivity,
                RamRagRating = pom.RamRagRating,
                SubmitterId = pom.SubmitterId
            };
    }
}
