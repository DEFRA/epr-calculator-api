CREATE PROCEDURE [dbo].[sp_GetPaycalPomData]
  @RelativeYear INT
, @CutOffDate DATETIME
AS
BEGIN
  SET NOCOUNT ON;
  SET @CutOffDate = ISNULL(@CutOffDate, '9999-12-31');

  DECLARE @start_dt DATETIME;
  DECLARE @batch_id INT;

  SELECT @batch_id = ISNULL(MAX(batch_id), 0) + 1
  FROM [dbo].[batch_log]

  SET @start_dt = GETDATE();

  BEGIN
    ----Find latest POM file with data submitted for a given organisation--
    -- Selection only - no eligibility decision (whether both H1 and H2 were submitted, whether a
    -- corresponding registration exists). Those are now made in C# (see IPomEligibilityFilter), which
    -- has both this run's organisations and POMs already in memory to check against.
    WITH latest_accepted_pom AS (
      SELECT * FROM (
        SELECT
          p.organisation_id
        , sofs.FileName
        , p.submission_period
        , sofs.submissionperiod as submission_period_desc
        --ST005 Updated logic to determine the latest accepted file submission with data for a given organisation
        , row_number() over(
            partition by p.organisation_id, coalesce(sofs.ComplianceSchemeId, o.ExternalId), sofs.SubmissionPeriod
            order by sofs.CreatedDateTime desc
        ) as latest_producer_accepted_record_per_SP
        , sofs.SubmissionPeriodYear as Submission_Period_Year
        , coalesce(sofs.ComplianceSchemeId, o.ExternalId) as submitter_id
        FROM rpd.Pom p
        INNER JOIN rpd.Organisations o
          on  o.ReferenceNumber = p.organisation_id
          --Excluding soft deleted organisations
          AND o.IsDeleted = 0
        INNER JOIN dbo.t_submitted_pom_org_file_status sofs
          ON  sofs.filetype         =  'Pom'
          AND sofs.FileName         =  p.FileName
          AND sofs.Regulator_Status =  'Accepted'
          AND sofs.SubmissionPeriodYear = @RelativeYear - 1
          AND (sofs.Is_resubmitted_POM_identifier = 0 OR sofs.CreatedDateTime <= @CutOffDate)
      ) a
      WHERE latest_producer_accepted_record_per_SP = 1
    )

      -----------------------------
      -----Main Selection of Data--
      -----------------------------
    SELECT
      p.organisation_id
    , NULLIF(trim(p.subsidiary_id), '') as subsidiary_id
    , p.submission_period
    , p.packaging_activity
    , p.packaging_type
    , p.packaging_class
    , p.packaging_material
    , p.packaging_material_weight
    , p.ram_rag_rating
    , p.packaging_material_subtype
    , lap.submission_period_desc
    , lap.submitter_id
    FROM rpd.POM p
    INNER JOIN latest_accepted_pom lap
      ON  trim(p.FileName)    = trim(lap.FileName)
      AND lap.organisation_id = p.organisation_id
    -- packaging_type/packaging_material selection (which packaging types count as reportable
    -- material) moved to C# - see ProducerPomAligner.Align. Everything below is still "accepted
    -- data only" selection.
    WHERE p.organisation_size = 'L'
      AND (p.to_country IS NULL OR trim(p.to_country) = '')
      AND p.organisation_id IS NOT NULL
      AND LEFT(p.submission_period,4) = (@RelativeYear - 1)

  END

  INSERT INTO [dbo].[batch_log]
    ([ID], [ProcessName], [SubProcessName], [Count], [start_time_stamp], [end_time_stamp], [Comments], [batch_id])
  SELECT
    (SELECT ISNULL(MAX(id), 1) + 1 FROM [dbo].[batch_log])
  , 'dbo.sp_GetPaycalPomData'
  , ''
  , NULL
  , @start_dt
  , GETDATE()
  , ''
  , @batch_id;

END
