using System.Diagnostics;
using System.Runtime.CompilerServices;
using EPR.CommonDataService.DataApi.CommonDataApi.Entities;
using EPR.CommonDataService.DataApi.CommonDataApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EPR.CommonDataService.DataApi.CommonDataApi;

public interface IStreamOrganisationsRequestHandler
{
    IAsyncEnumerable<PayCalOrganisation> Handle(int relativeYear, CancellationToken cancellationToken = default);
}

public sealed class StreamOrganisationsRequestHandler(IDbContextFactory<SynapseContext> dbContextFactory)
    : IStreamOrganisationsRequestHandler
{
    public async IAsyncEnumerable<PayCalOrganisation> Handle(int relativeYear,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = DataApiTelemetry.StartActivity(typeof(StreamOrganisationsRequestHandler), nameof(Handle));

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Previously sourced from the stored procedure dbo.sp_GetPaycalOrgData. This is selection only:
        // every accepted-status registration file is returned (no dedup, no cut-off filtering) - which
        // file "wins" per org/submitter/year, honouring the cut-off date, is decided in C# (see
        // IAcceptedFileSelector). The obligation decision (leaver code, status, days obligated) is made
        // in C# from the raw columns below (see IProducerObligationDeterminer), and has_h1/has_h2 are
        // derived in C# from the POM stream (see IOrganisationPeriodFlagsCalculator).
        var organisations = dbContext
            .PayCalOrganisations
            .FromSqlInterpolated($"""
                -- candidate_registration_files: join source tables, deduplicate to one row per file.
                WITH candidate_registration_files AS (
                    SELECT DISTINCT
                        cd.FileName,
                        cd.organisation_id,
                        sofs.SubmissionPeriodYear                       AS submission_period_year,
                        COALESCE(sofs.ComplianceSchemeId, o.ExternalId) AS submitter_id,
                        sofs.CreatedDateTime,
                        sofs.Regulator_Status,
                        sofs.IsResubmission_identifier
                    FROM rpd.CompanyDetails cd
                    INNER JOIN rpd.Organisations o
                        ON o.ReferenceNumber = cd.organisation_id
                    INNER JOIN dbo.t_submitted_pom_org_file_status sofs
                        ON sofs.FileName = cd.FileName
                       AND sofs.FileType = 'CompanyDetails'
                       AND sofs.Regulator_Status IN ('Granted', 'Accepted', 'Cancelled')
                    WHERE o.IsDeleted = 0
                )
                -- Main selection of data: join back to CompanyDetails, filter to large orgs.
                SELECT
                    crf.FileName AS file_name,
                    crf.organisation_id,
                    cd.subsidiary_id,
                    crf.submitter_id,
                    cd.organisation_name,
                    cd.trading_name,
                    cd.leaver_code AS status_code,
                    cd.joiner_date,
                    cd.leaver_date,
                    crf.submission_period_year,
                    crf.Regulator_Status AS regulator_status,
                    crf.CreatedDateTime AS created_date_time,
                    crf.IsResubmission_identifier AS is_resubmission
                FROM candidate_registration_files crf
                INNER JOIN rpd.CompanyDetails cd
                    ON  cd.organisation_id = crf.organisation_id
                    AND cd.FileName = crf.FileName
                WHERE cd.organisation_size = 'L'
                  AND cd.organisation_id IS NOT NULL
                  AND cd.organisation_name IS NOT NULL
                  AND crf.submission_period_year = {relativeYear}
                """)
            .AsNoTracking()
            .WithTimeout(TimeSpan.FromMinutes(10)) // Necessary due to poor db performance
            .AsAsyncEnumerable();

        await foreach (var organisation in organisations.WithCancellation(cancellationToken))
        {
            yield return organisation;
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
    }
}
