using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHistoricRunData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE calculator_run_organization_data_detail SET obligation_status = 'O' WHERE obligation_status = ''");

            migrationBuilder.Sql(@"
            -- ============================================================
            -- STEP 1: all producer_reported_material rows needing a submission_period split
            -- ============================================================
            DROP TABLE IF EXISTS ##historic_rows;

            SELECT
                prm.id                              AS producer_reported_material_id,
                prm.producer_detail_id,
                prm.material_id,
                prm.packaging_type,
                pd.producer_id                      AS organisation_id,
                pd.subsidiary_id,
                cr.calculator_run_pom_data_master_id,
                cr.calculator_run_organization_data_master_id,
                m.code                              AS material_code
            INTO ##historic_rows
            FROM producer_reported_material prm
            JOIN producer_detail pd ON pd.id = prm.producer_detail_id
            JOIN calculator_run cr ON cr.id = pd.calculator_run_id
            JOIN material m ON m.id = prm.material_id
            WHERE prm.submission_period = ''

            -- ============================================================
            -- STEP 2: obligated producer/submitter
            -- ============================================================
            DROP TABLE IF EXISTS ##obligated_submitters;

            WITH producers AS (
                SELECT DISTINCT
                    producer_detail_id, organisation_id, subsidiary_id, calculator_run_organization_data_master_id
                FROM ##historic_rows
            ),
            candidates AS (
                SELECT DISTINCT
                    p.producer_detail_id,
                    codd.submitter_id
                FROM producers p
                JOIN calculator_run_organization_data_detail codd
                    ON codd.calculator_run_organization_data_master_id = p.calculator_run_organization_data_master_id
                AND codd.organisation_id = p.organisation_id
                AND ISNULL(codd.subsidiary_id, '') = ISNULL(p.subsidiary_id, '')
                WHERE codd.obligation_status = 'O'
            )
            SELECT producer_detail_id, submitter_id
            INTO ##obligated_submitters
            FROM (
                SELECT *, COUNT(*) OVER (PARTITION BY producer_detail_id) AS obligated_submitter_count
                FROM candidates
            ) c
            WHERE obligated_submitter_count = 1;

            -- ============================================================
            -- STEP 3: split resolved rows by submission_period
            -- ============================================================
            DROP TABLE IF EXISTS ##rows_to_insert;

            SELECT
                hr.producer_reported_material_id,
                hr.material_id,
                hr.producer_detail_id,
                hr.packaging_type,
                pomd.submission_period,
                CAST(ROUND(SUM(pomd.packaging_material_weight) / 1000.0, 3) AS decimal(18,3))                                                       AS packaging_tonnage,
                CAST(ROUND(SUM(CASE WHEN pomd.ram_rag_rating = 'R'   THEN pomd.packaging_material_weight ELSE 0 END) / 1000.0, 3) AS decimal(18,3)) AS packaging_tonnage_red,
                CAST(ROUND(SUM(CASE WHEN pomd.ram_rag_rating = 'A'   THEN pomd.packaging_material_weight ELSE 0 END) / 1000.0, 3) AS decimal(18,3)) AS packaging_tonnage_amber,
                CAST(ROUND(SUM(CASE WHEN pomd.ram_rag_rating = 'G'   THEN pomd.packaging_material_weight ELSE 0 END) / 1000.0, 3) AS decimal(18,3)) AS packaging_tonnage_green,
                CAST(ROUND(SUM(CASE WHEN pomd.ram_rag_rating = 'R-M' THEN pomd.packaging_material_weight ELSE 0 END) / 1000.0, 3) AS decimal(18,3)) AS packaging_tonnage_red_medical,
                CAST(ROUND(SUM(CASE WHEN pomd.ram_rag_rating = 'A-M' THEN pomd.packaging_material_weight ELSE 0 END) / 1000.0, 3) AS decimal(18,3)) AS packaging_tonnage_amber_medical,
                CAST(ROUND(SUM(CASE WHEN pomd.ram_rag_rating = 'G-M' THEN pomd.packaging_material_weight ELSE 0 END) / 1000.0, 3) AS decimal(18,3)) AS packaging_tonnage_green_medical
            INTO ##rows_to_insert
            FROM ##historic_rows hr
            JOIN ##obligated_submitters os ON os.producer_detail_id = hr.producer_detail_id
            JOIN calculator_run_pom_data_detail pomd
                ON pomd.calculator_run_pom_data_master_id = hr.calculator_run_pom_data_master_id
            AND pomd.organisation_id = hr.organisation_id
            AND ISNULL(pomd.subsidiary_id, '') = ISNULL(hr.subsidiary_id, '')
            AND pomd.packaging_type = hr.packaging_type
            AND pomd.packaging_material = hr.material_code
            AND ISNULL(pomd.submitter_id, '00000000-0000-0000-0000-000000000000') = ISNULL(os.submitter_id, '00000000-0000-0000-0000-000000000000')
            WHERE pomd.submission_period IS NOT NULL
            GROUP BY
                hr.producer_reported_material_id, hr.material_id, hr.producer_detail_id,
                hr.packaging_type, pomd.submission_period;

            -- ============================================================
            -- STEP 4: write - every historic (summed) row is
            -- removed; only rows the current logic can actually derive get replaced.
            -- ============================================================
            DELETE prm
            FROM producer_reported_material prm
            JOIN ##historic_rows hr ON hr.producer_reported_material_id = prm.id;

            INSERT INTO producer_reported_material
                (material_id, producer_detail_id, packaging_type, packaging_tonnage,
                packaging_tonnage_red, packaging_tonnage_amber, packaging_tonnage_green,
                packaging_tonnage_red_medical, packaging_tonnage_amber_medical, packaging_tonnage_green_medical,
                submission_period)
            SELECT
                material_id, producer_detail_id, packaging_type, packaging_tonnage,
                packaging_tonnage_red, packaging_tonnage_amber, packaging_tonnage_green,
                packaging_tonnage_red_medical, packaging_tonnage_amber_medical, packaging_tonnage_green_medical,
                submission_period
            FROM ##rows_to_insert;
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
