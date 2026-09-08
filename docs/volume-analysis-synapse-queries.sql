/* ---------------------------------------------------------------------------
   Volume analysis for ECV-730 - row counts at each DataApi filter, measured
   against the warehouse. Same joins as spike-vs-main-synapse-queries.sql
   queries 1 & 2, just aggregated and not scoped to specific producers.

   Four independent SELECTs - run them one at a time. Set @RelativeYear first
   (POM year = @RelativeYear - 1).
   --------------------------------------------------------------------------- */

DECLARE @RelativeYear INT = 2026;   -- registration year; POM year is one less


/* ===========================================================================
   A. POM raw stream + reportable-packaging split.
      raw_stream_rows  = what StreamPomsRequestHandler hands the service.
      reportable_*     = ReportablePackaging step (E) - the rest is dropped.
   =========================================================================== */
SELECT
    COUNT(*)                                                                       AS raw_stream_rows,
    SUM(CASE WHEN p.packaging_type IN ('HH','CW','PB')                THEN 1 ELSE 0 END) AS reportable_hh_cw_pb,
    SUM(CASE WHEN p.packaging_type = 'HDC' AND p.packaging_material = 'GL' THEN 1 ELSE 0 END) AS reportable_hdc_glass,
    COUNT(DISTINCT p.organisation_id)                                              AS distinct_orgs
FROM rpd.Pom p
INNER JOIN rpd.Organisations o
    ON o.ReferenceNumber = p.organisation_id AND o.IsDeleted = 0
INNER JOIN dbo.t_submitted_pom_org_file_status sofs
    ON sofs.filetype             = 'Pom'
   AND sofs.FileName             = p.FileName
   AND sofs.Regulator_Status     = 'Accepted'
   AND sofs.SubmissionPeriodYear = @RelativeYear - 1
WHERE p.organisation_size = 'L'
  AND (p.to_country IS NULL OR TRIM(p.to_country) = '')
  AND p.organisation_id IS NOT NULL
  AND LEFT(p.submission_period, 4) = (@RelativeYear - 1);


/* ===========================================================================
   B. POM - bite of the size and export filters (drop the size/country lines
      from the WHERE clause, count them as conditional sums instead).
   =========================================================================== */
SELECT
    COUNT(*)                                                                         AS accepted_line_items_any_size,
    SUM(CASE WHEN p.organisation_size = 'L' THEN 1 ELSE 0 END)                        AS size_L,
    SUM(CASE WHEN p.organisation_size = 'L'
             AND (p.to_country IS NULL OR TRIM(p.to_country) = '') THEN 1 ELSE 0 END) AS size_L_non_export
FROM rpd.Pom p
INNER JOIN rpd.Organisations o
    ON o.ReferenceNumber = p.organisation_id AND o.IsDeleted = 0
INNER JOIN dbo.t_submitted_pom_org_file_status sofs
    ON sofs.filetype             = 'Pom'
   AND sofs.FileName             = p.FileName
   AND sofs.Regulator_Status     = 'Accepted'
   AND sofs.SubmissionPeriodYear = @RelativeYear - 1
WHERE p.organisation_id IS NOT NULL
  AND LEFT(p.submission_period, 4) = (@RelativeYear - 1);


/* ===========================================================================
   C. ORG raw stream + bite of the size filter.
      reg_rows_size_L        = what StreamOrganisationsRequestHandler hands the
                               service (before file selection / dedup).
      distinct_orgs_L        = ballpark for the Organisations output (~11k).
   =========================================================================== */
SELECT
    COUNT(*)                                                                       AS reg_rows_any_size,
    SUM(CASE WHEN cd.organisation_size = 'L' THEN 1 ELSE 0 END)                     AS reg_rows_size_L,
    COUNT(DISTINCT cd.organisation_id)                                             AS distinct_orgs_any_size,
    COUNT(DISTINCT CASE WHEN cd.organisation_size = 'L' THEN cd.organisation_id END) AS distinct_orgs_L
FROM rpd.CompanyDetails cd
INNER JOIN rpd.Organisations o
    ON o.ReferenceNumber = cd.organisation_id AND o.IsDeleted = 0
INNER JOIN dbo.t_submitted_pom_org_file_status sofs
    ON sofs.FileName         = cd.FileName
   AND sofs.FileType         = 'CompanyDetails'
   AND sofs.Regulator_Status IN ('Granted','Accepted','Cancelled')
WHERE cd.organisation_id   IS NOT NULL
  AND cd.organisation_name IS NOT NULL
  AND sofs.SubmissionPeriodYear = @RelativeYear;


/* ===========================================================================
   D. Resubmission rate - how much AcceptedFileSelector (step A) has to dedup.
      groups            = distinct (org, submitter, period).
      groups_multifile  = those with more than one accepted file.
   =========================================================================== */
SELECT
    COUNT(*)                                          AS groups,
    SUM(CASE WHEN file_count > 1 THEN 1 ELSE 0 END)   AS groups_multifile,
    SUM(file_count)                                   AS total_files,
    MAX(file_count)                                   AS max_files_in_a_group
FROM (
    SELECT p.organisation_id,
           COALESCE(sofs.ComplianceSchemeId, o.ExternalId) AS submitter_id,
           p.submission_period,
           COUNT(DISTINCT p.FileName) AS file_count
    FROM rpd.Pom p
    INNER JOIN rpd.Organisations o
        ON o.ReferenceNumber = p.organisation_id AND o.IsDeleted = 0
    INNER JOIN dbo.t_submitted_pom_org_file_status sofs
        ON sofs.filetype             = 'Pom'
       AND sofs.FileName             = p.FileName
       AND sofs.Regulator_Status     = 'Accepted'
       AND sofs.SubmissionPeriodYear = @RelativeYear - 1
    WHERE p.organisation_size = 'L'
      AND (p.to_country IS NULL OR TRIM(p.to_country) = '')
      AND p.organisation_id IS NOT NULL
      AND LEFT(p.submission_period, 4) = (@RelativeYear - 1)
    GROUP BY p.organisation_id,
             COALESCE(sofs.ComplianceSchemeId, o.ExternalId),
             p.submission_period
) g;
