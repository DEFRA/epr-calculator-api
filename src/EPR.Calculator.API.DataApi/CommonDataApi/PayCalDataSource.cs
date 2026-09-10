using System.Runtime.CompilerServices;
using EPR.CommonDataService.DataApi.CommonDataApi.Entities;
using EPR.CommonDataService.DataApi.CommonDataApi.LoadTables;
using Microsoft.EntityFrameworkCore;

namespace EPR.CommonDataService.DataApi.CommonDataApi;

/// <summary>
///     Where <see cref="ProducerDataService" /> reads its raw organisation and POM rows from. Two
///     implementations: straight off the RPD source, or off the local <c>data_api_load_*</c> tables
///     that <see cref="ILoadTableRefresher" /> filled from that source once.
/// </summary>
public interface IPayCalDataSource
{
    IAsyncEnumerable<PayCalOrganisation> StreamOrganisations(int relativeYear, CancellationToken cancellationToken = default);

    IAsyncEnumerable<PayCalPom> StreamPoms(int relativeYear, CancellationToken cancellationToken = default);
}

/// <summary>Reads the RPD source directly via the streaming request handlers.</summary>
public sealed class SynapseDataSource(
    IStreamOrganisationsRequestHandler organisationsHandler,
    IStreamPomsRequestHandler pomsHandler) : IPayCalDataSource
{
    public IAsyncEnumerable<PayCalOrganisation> StreamOrganisations(int relativeYear, CancellationToken cancellationToken = default) =>
        organisationsHandler.Handle(relativeYear, cancellationToken);

    public IAsyncEnumerable<PayCalPom> StreamPoms(int relativeYear, CancellationToken cancellationToken = default) =>
        pomsHandler.Handle(relativeYear, cancellationToken);
}

/// <summary>
///     Reads back the rows staged in <c>data_api_load_organisations</c> / <c>data_api_load_poms</c>.
///     <see cref="ProducerDataService" /> streams POMs twice; a local table scan is cheap where a
///     second RPD query is not.
/// </summary>
public sealed class LoadTableDataSource(IDbContextFactory<DataApiLoadContext> dbContextFactory) : IPayCalDataSource
{
    public async IAsyncEnumerable<PayCalOrganisation> StreamOrganisations(
        int relativeYear, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var rows = dbContext.LoadOrganisations
            .AsNoTracking()
            .OrderBy(o => o.Id)
            .AsAsyncEnumerable();

        await foreach (var o in rows.WithCancellation(cancellationToken))
        {
            yield return new PayCalOrganisation
            {
                OrganisationId = o.OrganisationId,
                SubsidiaryId = o.SubsidiaryId,
                SubmitterId = o.SubmitterId,
                OrganisationName = o.OrganisationName,
                TradingName = o.TradingName,
                StatusCode = o.StatusCode,
                LeaverDate = o.LeaverDate,
                JoinerDate = o.JoinerDate,
                RegulatorStatus = o.RegulatorStatus,
                ObligationStatus = o.ObligationStatus,
                NumDaysObligated = o.NumDaysObligated,
                ErrorCode = o.ErrorCode,
                SubmissionPeriodYear = o.SubmissionPeriodYear,
                HasH1 = o.HasH1,
                HasH2 = o.HasH2,
                FileName = o.FileName,
                IsResubmission = o.IsResubmission,
                CreatedDateTime = o.CreatedDateTime
            };
        }
    }

    public async IAsyncEnumerable<PayCalPom> StreamPoms(
        int relativeYear, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var rows = dbContext.LoadPoms
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .AsAsyncEnumerable();

        await foreach (var p in rows.WithCancellation(cancellationToken))
        {
            yield return new PayCalPom
            {
                OrganisationId = p.OrganisationId,
                SubsidiaryId = p.SubsidiaryId,
                SubmitterId = p.SubmitterId,
                SubmissionPeriod = p.SubmissionPeriod,
                SubmissionPeriodDescription = p.SubmissionPeriodDescription,
                PackagingActivity = p.PackagingActivity,
                PackagingType = p.PackagingType,
                PackagingClass = p.PackagingClass,
                PackagingMaterial = p.PackagingMaterial,
                PackagingMaterialSubtype = p.PackagingMaterialSubtype,
                PackagingMaterialWeight = p.PackagingMaterialWeight,
                RamRagRating = p.RamRagRating,
                FileName = p.FileName,
                IsResubmission = p.IsResubmission,
                CreatedDateTime = p.CreatedDateTime
            };
        }
    }
}
