/* ---------------------------------------------------------------------------
   Spike 8 vs main discrepancy - Synapse queries used to confirm the root cause.
   Warehouse: devepdinfas1401.sql.azuresynapse.net / devepdsp1401
   Producers: 165026 (subs 165023, 165027), 167432, 169063 (subs 169064, 169065)
   Financial year 2026-27  ->  @RelativeYear = 2026, registration spy = 2026,
   POM SubmissionPeriodYear = 2025.
   See docs/spike-vs-main-discrepancy-analysis.md for the findings.
   --------------------------------------------------------------------------- */

/* ===========================================================================
   1. Registration (CompanyDetails) file history.
      Result: 165026 has a Granted file (49700de1) and a later Pending file
      (c9287ff5). fn_ProducerObligationDetermination filters to
      Granted/Accepted/Cancelled, so it uses 49700de1 - which has CompanyDetails
      rows for the parent + 165027 ONLY (no 165023).
      167432 / 169063: their only 2026 registration file is 'Cancelled'.
   =========================================================================== */
SELECT
    cd.organisation_id, cd.subsidiary_id, DATALENGTH(cd.subsidiary_id) AS sub_bytes,
    cd.organisation_size, cd.leaver_code, cd.joiner_date, cd.leaver_date,
    cd.FileName, sofs.Regulator_Status, sofs.SubmissionPeriodYear AS spy,
    sofs.SubmissionPeriod AS sp, sofs.ComplianceSchemeId, o.ExternalId,
    COALESCE(sofs.ComplianceSchemeId, o.ExternalId) AS submitter_id,
    sofs.CreatedDateTime, sofs.IsResubmission_identifier AS is_resub
FROM rpd.CompanyDetails cd
INNER JOIN rpd.Organisations o ON o.ReferenceNumber = cd.organisation_id AND o.IsDeleted = 0
INNER JOIN dbo.t_submitted_pom_org_file_status sofs
    ON sofs.FileName = cd.FileName AND sofs.FileType = 'CompanyDetails'
WHERE cd.organisation_id IN (165026, 167432, 169063)
ORDER BY cd.organisation_id, submitter_id, sofs.CreatedDateTime DESC, cd.subsidiary_id;


/* ===========================================================================
   2. POM (rpd.Pom) file history + resolved submitter_id per file.
      Result: for all three producers the registration and POM submitter_id
      (COALESCE(ComplianceSchemeId, ExternalId)) are equal - divergence "submitter
      provenance" is NOT the cause here.
   =========================================================================== */
SELECT DISTINCT
    p.organisation_id, p.subsidiary_id, DATALENGTH(p.subsidiary_id) AS sub_bytes,
    p.organisation_size, p.FileName, p.submission_period, p.to_country,
    sofs.Regulator_Status, sofs.SubmissionPeriodYear AS spy, sofs.SubmissionPeriod AS sp,
    sofs.ComplianceSchemeId, o.ExternalId,
    COALESCE(sofs.ComplianceSchemeId, o.ExternalId) AS submitter_id,
    sofs.CreatedDateTime, sofs.Is_resubmitted_POM_identifier AS is_resub
FROM rpd.Pom p
INNER JOIN rpd.Organisations o ON o.ReferenceNumber = p.organisation_id AND o.IsDeleted = 0
INNER JOIN dbo.t_submitted_pom_org_file_status sofs
    ON sofs.FileType = 'Pom' AND sofs.FileName = p.FileName
WHERE p.organisation_id IN (165026, 167432, 169063)
ORDER BY p.organisation_id, submitter_id, sofs.CreatedDateTime DESC, p.subsidiary_id;


/* ===========================================================================
   3. What fn_ProducerObligationDetermination returns for the three producers
      (== what sp_GetPaycalOrgData returns on main, == what the spike's
      ProducerObligationDeterminer produces - verified identical).
      Result: 165026 parent + 165027 => 'O'.  167432 => 'E'/Not Obligated.
              169063 parent + 169064 + 169065 => 'E'/Not Obligated.
   =========================================================================== */
SELECT ob.organisation_id, ob.subsidiary_id, ob.submitter_id, ob.status_code,
       ob.obligation_status, ob.error_code, ob.num_days_obligated
FROM dbo.fn_ProducerObligationDetermination(NULL) ob
WHERE ob.submission_period_year = 2026
  AND ob.organisation_id IN (165026, 167432, 169063)
ORDER BY ob.organisation_id, ob.subsidiary_id;


/* ===========================================================================
   4. The full sp_GetPaycalPomData body, replayed for the three producers
      (@RelativeYear = 2026, no cut-off). THIS IS THE DECISIVE QUERY.
      Result: returns ONLY {165026/parent, 165026/165027} x {H1,H2}.
        - 165023 is absent  -> its only POM line items are packaging_type
          NH/OW/RU, all removed by the reportable-type WHERE clause.
        - 167432 / 169063 are absent entirely -> no Granted/Accepted registration,
          so Latest_Org_Data_Selection excludes them.
      The spike keeps all of these rows (packaging filter moved to
      ProducerPomAligner.Align; PomEligibilityFilter accepts Cancelled-registration
      orgs), so its ProducerErrorDetector raises errors main never sees.
   =========================================================================== */
DECLARE @RelativeYear INT = 2026;
DECLARE @CutOffDate DATETIME = '9999-12-31';

WITH latest_accepted_registration AS (
    SELECT * FROM (
        SELECT sofs.filename, cd.organisation_id,
            ROW_NUMBER() OVER (PARTITION BY cd.organisation_id,
                COALESCE(sofs.ComplianceSchemeId, o.ExternalId), sofs.SubmissionPeriod
                ORDER BY sofs.CreatedDateTime DESC) AS rn,
            sofs.SubmissionPeriodYear AS Submission_Period_Year
        FROM rpd.CompanyDetails cd
        INNER JOIN rpd.Organisations o ON o.ReferenceNumber = cd.organisation_id AND o.IsDeleted = 0
        INNER JOIN dbo.t_submitted_pom_org_file_status sofs
            ON sofs.filetype = 'CompanyDetails' AND sofs.FileName = cd.FileName
           AND sofs.Regulator_Status IN ('Granted','Accepted')     -- <-- NO 'Cancelled'
           AND sofs.SubmissionPeriodYear > 2024
           AND (sofs.IsResubmission_identifier = 0 OR sofs.CreatedDateTime <= @CutOffDate)
    ) a WHERE rn = 1
),
latest_accepted_pom AS (
    SELECT * FROM (
        SELECT p.organisation_id, sofs.FileName, p.submission_period,
            ROW_NUMBER() OVER (PARTITION BY p.organisation_id,
                COALESCE(sofs.ComplianceSchemeId, o.ExternalId), sofs.SubmissionPeriod
                ORDER BY sofs.CreatedDateTime DESC) AS rn,
            sofs.SubmissionPeriodYear AS Submission_Period_Year,
            COALESCE(sofs.ComplianceSchemeId, o.ExternalId) AS submitter_id
        FROM rpd.Pom p
        INNER JOIN rpd.Organisations o ON o.ReferenceNumber = p.organisation_id AND o.IsDeleted = 0
        INNER JOIN dbo.t_submitted_pom_org_file_status sofs
            ON sofs.filetype = 'Pom' AND sofs.FileName = p.FileName
           AND sofs.Regulator_Status = 'Accepted'
           AND sofs.SubmissionPeriodYear = @RelativeYear - 1
           AND (sofs.Is_resubmitted_POM_identifier = 0 OR sofs.CreatedDateTime <= @CutOffDate)
    ) a WHERE rn = 1
),
opf AS (
    SELECT organisation_id, submitter_id, CAST(Submission_Period_Year AS INT) AS spy,
        MAX(CASE WHEN CAST(Submission_Period_Year AS INT) > 2024 AND RIGHT(submission_period,3) = '-H1' THEN 1
                 WHEN submission_period IN ('2024-P1','2024-P2','2024-P3') THEN 1 ELSE 0 END) AS has_h1,
        MAX(CASE WHEN CAST(Submission_Period_Year AS INT) > 2024 AND RIGHT(submission_period,3) = '-H2' THEN 1
                 WHEN submission_period = '2024-P4' THEN 1 ELSE 0 END) AS has_h2
    FROM latest_accepted_pom
    GROUP BY organisation_id, submitter_id, Submission_Period_Year
),
l2p AS (
    SELECT pom.* FROM latest_accepted_pom pom
    INNER JOIN opf ON pom.organisation_id = opf.organisation_id
                  AND pom.submitter_id = opf.submitter_id
                  AND pom.Submission_Period_Year = opf.spy
    WHERE has_h1 = 1 AND has_h2 = 1
),
lods AS (
    SELECT DISTINCT cd.organisation_id, lar.Submission_Period_Year - 1 AS spym1
    FROM rpd.CompanyDetails cd
    INNER JOIN latest_accepted_registration lar
        ON cd.filename = lar.filename AND cd.Organisation_size = 'L'
       AND lar.organisation_id = cd.organisation_id
       AND cd.organisation_id IS NOT NULL AND cd.organisation_name IS NOT NULL
)
SELECT DISTINCT
    p.organisation_id, NULLIF(TRIM(p.subsidiary_id), '') AS subsidiary_id,
    p.submission_period, l2p.submitter_id
FROM rpd.POM p
INNER JOIN l2p ON TRIM(p.FileName) = TRIM(l2p.FileName) AND l2p.organisation_id = p.organisation_id
INNER JOIN lods ON lods.organisation_id = p.organisation_id AND lods.spym1 = l2p.Submission_Period_Year
WHERE (p.packaging_type IN ('HH','CW','PB') OR (p.packaging_type = 'HDC' AND p.packaging_material = 'GL'))
  AND p.organisation_size = 'L'
  AND (p.to_country IS NULL OR TRIM(p.to_country) = '')
  AND p.organisation_id IS NOT NULL
  AND LEFT(p.submission_period, 4) = (@RelativeYear - 1)
  AND p.organisation_id IN (165026, 167432, 169063)
ORDER BY p.organisation_id, subsidiary_id, p.submission_period;


/* ===========================================================================
   5. Proof that subsidiary 165023's POM is all non-reportable packaging types.
      Result: every row is packaging_type NH / OW / RU - none HH/CW/PB/HDC.
   =========================================================================== */
SELECT p.subsidiary_id, p.submission_period, p.packaging_type, p.packaging_material,
       COUNT(*) AS row_count, SUM(p.packaging_material_weight) AS total_weight
FROM rpd.Pom p
WHERE p.organisation_id = 165026 AND p.subsidiary_id = '165023'
GROUP BY p.subsidiary_id, p.submission_period, p.packaging_type, p.packaging_material
ORDER BY p.submission_period, p.packaging_type, p.packaging_material;


/* ===========================================================================
   6. (API database, NOT Synapse) Was any of the three invoiced in a prior
      completed run this FY?  This is what InvoicedProducerService.GetInvoicedProducers
      reads; the ReplaceOrgPomStagingWithCalculatorRunOrganisation migration does
      not touch these tables. Expected: only 168946 (not in this list) is invoiced;
      165026 is invoiced (shows in Cancelled Producers as run 28); 167432/169063 are not.
   =========================================================================== */
-- SELECT s.producer_id, s.calculator_run_id, r.name AS run_name,
--        r.calculator_run_classification_id, r.relative_year,
--        s.billing_instruction_accept_reject, s.suggested_billing_instruction
-- FROM producer_result_file_suggested_billing_instruction s
-- INNER JOIN calculator_run r ON r.id = s.calculator_run_id
-- WHERE s.producer_id IN (165026, 167432, 169063)
--   AND r.relative_year = 2026
--   AND r.calculator_run_classification_id IN (4, 8, 10, 11)
--   AND s.billing_instruction_accept_reject = 'Accepted'
-- ORDER BY s.producer_id, s.calculator_run_id;
