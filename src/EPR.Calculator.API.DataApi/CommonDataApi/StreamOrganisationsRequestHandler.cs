using System.Diagnostics;
using System.Runtime.CompilerServices;
using EPR.CommonDataService.DataApi.CommonDataApi.Entities;
using EPR.CommonDataService.DataApi.CommonDataApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EPR.CommonDataService.DataApi.CommonDataApi;

public interface IStreamOrganisationsRequestHandler
{
    IAsyncEnumerable<PayCalOrganisation> Handle(int relativeYear, DateTimeOffset? cutOffDate,
        CancellationToken cancellationToken = default);
}

public sealed class StreamOrganisationsRequestHandler(IDbContextFactory<SynapseContext> dbContextFactory)
    : IStreamOrganisationsRequestHandler
{
    public async IAsyncEnumerable<PayCalOrganisation> Handle(int relativeYear, DateTimeOffset? cutOffDate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = DataApiTelemetry.StartActivity(typeof(StreamOrganisationsRequestHandler), nameof(Handle));

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Previously sourced from the stored procedure dbo.sp_GetPaycalOrgData. This is selection only:
        // the obligation decision (leaver code, status, days obligated) is made in C# from the raw
        // columns below (see IProducerObligationDeterminer), and has_h1/has_h2 are derived in C# from
        // the POM stream (see IOrganisationPeriodFlagsCalculator). A NULL cutOffDate means "no cut-off"
        // (include everything).
        var organisations = dbContext
            .PayCalOrganisations
            .FromSqlInterpolated($"""
                WITH
                -- latest_accepted_registration_files: join source tables, deduplicate, keep the most
                -- recent file per org/submitter/year.
                larf_base AS (
                    SELECT DISTINCT
                        cd.FileName,
                        cd.organisation_id,
                        sofs.SubmissionPeriodYear                       AS submission_period_year,
                        COALESCE(sofs.ComplianceSchemeId, o.ExternalId) AS submitter_id,
                        sofs.CreatedDateTime,
                        sofs.Regulator_Status
                    FROM rpd.CompanyDetails cd
                    INNER JOIN rpd.Organisations o
                        ON o.ReferenceNumber = cd.organisation_id
                    INNER JOIN dbo.t_submitted_pom_org_file_status sofs
                        ON sofs.FileName = cd.FileName
                       AND sofs.FileType = 'CompanyDetails'
                       AND sofs.Regulator_Status IN ('Granted', 'Accepted', 'Cancelled')
                    WHERE o.IsDeleted = 0
                      AND (sofs.IsResubmission_identifier = 0
                           OR {cutOffDate} IS NULL
                           OR sofs.CreatedDateTime <= {cutOffDate})
                ),
                latest_accepted_registration_files AS (
                    SELECT FileName, organisation_id, submission_period_year, submitter_id, CreatedDateTime, Regulator_Status
                    FROM (
                        SELECT *,
                            ROW_NUMBER() OVER (
                                PARTITION BY organisation_id, submitter_id, submission_period_year
                                ORDER BY CreatedDateTime DESC
                            ) AS rn
                        FROM larf_base
                    ) t
                    WHERE rn = 1
                ),
                -- latest_accepted_registrations: join back to CompanyDetails, filter to large orgs.
                latest_accepted_registrations AS (
                    SELECT
                        larf.organisation_id,
                        cd.subsidiary_id,
                        larf.submitter_id,
                        cd.organisation_name,
                        cd.trading_name,
                        cd.leaver_code AS status_code,
                        cd.joiner_date,
                        cd.leaver_date,
                        larf.submission_period_year,
                        larf.Regulator_Status AS regulator_status
                    FROM latest_accepted_registration_files larf
                    INNER JOIN rpd.CompanyDetails cd
                        ON  cd.organisation_id = larf.organisation_id
                        AND cd.FileName = larf.FileName
                    WHERE cd.organisation_size = 'L'
                      AND cd.organisation_id IS NOT NULL
                      AND cd.organisation_name IS NOT NULL
                )
                SELECT
                    ob.organisation_id
                  , ob.subsidiary_id
                  , ob.submitter_id
                  , ob.organisation_name
                  , ob.trading_name
                  , ob.status_code
                  , ob.leaver_date
                  , ob.joiner_date
                  , ob.regulator_status
                  , ob.submission_period_year
                FROM latest_accepted_registrations ob
                WHERE ob.submission_period_year = {relativeYear}
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
