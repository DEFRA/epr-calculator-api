CREATE PROCEDURE [dbo].[sp_GetPaycalOrgData]
  @RelativeYear INT
, @CutOffDate DATETIME
AS
BEGIN
  SET NOCOUNT ON;
  SET @CutOffDate = ISNULL(@CutOffDate, '9999-12-31');   -- NULL = no cut-off (include everything)

  DECLARE @start_dt DATETIME;
  DECLARE @batch_id INT;

  SELECT @batch_id = ISNULL(MAX(batch_id), 0) + 1
  FROM [dbo].[batch_log]

  SET @start_dt = GETDATE();

  BEGIN

    WITH
    -- latest_accepted_registration_files: join source tables, deduplicate, keep most recent per org/submitter/year.
    -- Selection only - no obligation decision (leaver code, status, days obligated). That decision is now
    -- made in C# (see IProducerObligationDeterminer) from the raw columns selected below - @RelativeYear
    -- is still applied at the end, same as before.
    larf_base AS (
      SELECT DISTINCT
          cd.FileName,
          cd.organisation_id,
          sofs.SubmissionPeriodYear                                AS submission_period_year,
          COALESCE(sofs.ComplianceSchemeId, o.ExternalId)          AS submitter_id,
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
        AND (sofs.IsResubmission_identifier = 0 OR @CutOffDate IS NULL OR sofs.CreatedDateTime <= @CutOffDate)
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
    -- latest_accepted_registrations: join back to CompanyDetails, filter large orgs
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
    ),
    latest_accepted_pom AS (
      SELECT
        a.organisation_id
      , a.subsidiary_id
      , a.submission_period
      , a.submission_period_year
      , a.submitter_id
      FROM (
        SELECT
          p.organisation_id
        , NULLIF(p.subsidiary_id, '') AS subsidiary_id
        , p.submission_period
        , ROW_NUMBER() OVER (
            PARTITION BY p.organisation_id, p.subsidiary_id, COALESCE(sofs.ComplianceSchemeId, o.ExternalId), sofs.SubmissionPeriod
            ORDER BY sofs.CreatedDateTime DESC
          ) AS latest_producer_accepted_record_per_SP
        , sofs.SubmissionPeriodYear AS submission_period_year
        , COALESCE(sofs.ComplianceSchemeId, o.ExternalId) AS submitter_id
        FROM rpd.Pom p
        INNER JOIN rpd.Organisations o
          ON  o.ReferenceNumber         = p.organisation_id
          AND o.IsDeleted               = 0
        INNER JOIN dbo.t_submitted_pom_org_file_status sofs
          ON  sofs.filetype             =  'Pom'
          AND sofs.FileName             =  p.FileName
          AND sofs.Regulator_Status     =  'Accepted'
          AND sofs.SubmissionPeriodYear =  @RelativeYear - 1
          AND (sofs.Is_resubmitted_POM_identifier = 0 OR sofs.CreatedDateTime <= @CutOffDate)
      ) a
      WHERE a.latest_producer_accepted_record_per_SP = 1
    ),
    organisation_period_flags AS (
      SELECT
        organisation_id
      , subsidiary_id
      , submitter_id
      , submission_period_year
      , MAX(CASE
              WHEN CAST(submission_period_year AS INT) > 2024 AND RIGHT(submission_period, 3) = '-H1' THEN 1
              WHEN submission_period in ('2024-P1', '2024-P2', '2024-P3') THEN 1
              ELSE 0
            END) AS has_h1
      , MAX(CASE
              WHEN CAST(submission_period_year AS INT) > 2024 AND RIGHT(submission_period, 3) = '-H2' THEN 1
              WHEN submission_period = '2024-P4' THEN 1
              ELSE 0
            END) AS has_h2
      FROM latest_accepted_pom
      GROUP BY
        organisation_id
      , subsidiary_id
      , submitter_id
      , submission_period_year
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
      , CAST(COALESCE(opf.has_h1, 0) AS BIT) AS has_h1
      , CAST(COALESCE(opf.has_h2, 0) AS BIT) AS has_h2
    FROM latest_accepted_registrations ob
    LEFT JOIN organisation_period_flags opf
      ON  opf.organisation_id            = ob.organisation_id
      AND ISNULL(opf.subsidiary_id, '')  = ISNULL(ob.subsidiary_id, '')
      AND ISNULL(opf.submitter_id, '')   = ISNULL(ob.submitter_id, '')
      AND opf.submission_period_year + 1 = ob.submission_period_year
    WHERE ob.submission_period_year = @RelativeYear;

  END

  INSERT INTO [dbo].[batch_log]
    ([ID], [ProcessName], [SubProcessName], [Count], [start_time_stamp], [end_time_stamp], [Comments], [batch_id])
  SELECT
    (SELECT ISNULL(MAX(id), 1) + 1 FROM [dbo].[batch_log])
  , 'dbo.sp_GetPaycalOrgData'
  , ''
  , NULL
  , @start_dt
  , GETDATE()
  , ''
  , @batch_id;

END
