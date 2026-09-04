using System.Diagnostics;
using System.Runtime.CompilerServices;
using EPR.CommonDataService.DataApi.CommonDataApi.Entities;
using EPR.CommonDataService.DataApi.CommonDataApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EPR.CommonDataService.DataApi.CommonDataApi;

public interface IStreamPomsRequestHandler
{
    IAsyncEnumerable<PayCalPom> Handle(int relativeYear, CancellationToken cancellationToken = default);
}

public sealed class StreamPomsRequestHandler(IDbContextFactory<SynapseContext> dbContextFactory)
    : IStreamPomsRequestHandler
{
    public async IAsyncEnumerable<PayCalPom> Handle(int relativeYear,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = DataApiTelemetry.StartActivity(typeof(StreamPomsRequestHandler), nameof(Handle));

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Previously sourced from the stored procedure dbo.sp_GetPaycalPomData. This is selection only
        // ("accepted data only"): every accepted POM file is returned (no dedup, no cut-off filtering) -
        // which file "wins" per org/submitter/period, honouring the cut-off date, is decided in C# (see
        // IAcceptedFileSelector). The eligibility decision (whether both H1 and H2 were submitted,
        // whether a matching registration exists) is made in C# (see IPomEligibilityFilter), and the
        // reportable packaging_type/packaging_material selection is made in C# (see ProducerPomAligner.Align).
        var poms = dbContext
            .PayCalPoms
            .FromSqlInterpolated($"""
                -- candidate_pom_files: accepted POM files submitted for a given organisation/submitter/period.
                -- DISTINCT collapses rpd.Pom's per-line-item rows down to one row per file (every selected
                -- column here is a per-file attribute, not a per-line-item one).
                WITH candidate_pom_files AS (
                    SELECT DISTINCT
                      p.organisation_id
                    , sofs.FileName
                    , p.submission_period
                    , sofs.submissionperiod AS submission_period_desc
                    , sofs.SubmissionPeriodYear AS Submission_Period_Year
                    , COALESCE(sofs.ComplianceSchemeId, o.ExternalId) AS submitter_id
                    , sofs.CreatedDateTime
                    , sofs.Is_resubmitted_POM_identifier
                    FROM rpd.Pom p
                    INNER JOIN rpd.Organisations o
                      ON  o.ReferenceNumber = p.organisation_id
                      -- Excluding soft deleted organisations
                      AND o.IsDeleted = 0
                    INNER JOIN dbo.t_submitted_pom_org_file_status sofs
                      ON  sofs.filetype         = 'Pom'
                      AND sofs.FileName         = p.FileName
                      AND sofs.Regulator_Status = 'Accepted'
                      AND sofs.SubmissionPeriodYear = {relativeYear} - 1
                )
                -- Main selection of data
                SELECT
                  p.organisation_id
                , NULLIF(TRIM(p.subsidiary_id), '') AS subsidiary_id
                , p.submission_period
                , p.packaging_activity
                , p.packaging_type
                , p.packaging_class
                , p.packaging_material
                , p.packaging_material_weight
                , p.ram_rag_rating
                , p.packaging_material_subtype
                , cpf.submission_period_desc
                , cpf.submitter_id
                , cpf.FileName AS file_name
                , cpf.CreatedDateTime AS created_date_time
                , CAST(cpf.Is_resubmitted_POM_identifier AS bit) AS is_resubmission
                FROM rpd.POM p
                INNER JOIN candidate_pom_files cpf
                  ON  TRIM(p.FileName)    = TRIM(cpf.FileName)
                  AND cpf.organisation_id = p.organisation_id
                WHERE p.organisation_size = 'L'
                  AND (p.to_country IS NULL OR TRIM(p.to_country) = '')
                  AND p.organisation_id IS NOT NULL
                  AND LEFT(p.submission_period, 4) = ({relativeYear} - 1)
                """)
            .AsNoTracking()
            .WithTimeout(TimeSpan.FromMinutes(10)) // Necessary due to poor db performance
            .AsAsyncEnumerable();

        await foreach (var pom in poms.WithCancellation(cancellationToken))
        {
            yield return pom;
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
    }
}
