using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class BillingRunUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_calculator_run_billing_file_metadata_calculator_run_id",
                table: "calculator_run_billing_file_metadata");

            migrationBuilder.DropIndex(
                name: "IX_calculator_run_relative_year",
                table: "calculator_run");

            migrationBuilder.DropIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run");

            migrationBuilder.AddColumn<DateTime>(
                name: "billing_run_started_at",
                table: "calculator_run",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "billing_run_status",
                table: "calculator_run",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // Delete dodgy data, only present on DEV/TST
            migrationBuilder.Sql("""
                DELETE FROM
                	dbo.calculator_run_billing_file_metadata
                WHERE
                	billing_csv_filename IS NULL OR billing_json_filename IS NULL
                """);

            // Delete more dodgy data, only present on DEV/TST/PRE1
            migrationBuilder.Sql("""
                WITH ranked AS (
                    SELECT
                        *,
                        ROW_NUMBER() OVER (
                            PARTITION BY calculator_run_id
                            ORDER BY id DESC
                        ) AS rn
                    FROM dbo.calculator_run_billing_file_metadata
                )
                DELETE FROM ranked
                WHERE rn > 1
                """);

            // Runs with billing files => Completed
            migrationBuilder.Sql("""
                UPDATE
                    run
                SET
                    run.billing_run_status = 'Completed',
                    run.billing_run_started_at = DATEADD(HOUR, -1, metadata.billing_file_created_date)
                FROM
                    dbo.calculator_run AS run
                INNER JOIN
                    dbo.calculator_run_billing_file_metadata AS metadata
                    ON metadata.calculator_run_id = run.id
                WHERE
                    run.billing_run_status IS NULL
                """);

            // Remaining is_billing_file_generating runs => Errored
            migrationBuilder.Sql("""
                UPDATE
                    run
                SET
                    run.billing_run_status = 'Errored',
                    run.billing_run_started_at = DATEADD(HOUR, -1, run.updated_at)
                FROM
                    dbo.calculator_run AS run
                WHERE
                    run.billing_run_status IS NULL AND
                    run.is_billing_file_generating = 1
                """);

            // Remaining IN_THE_QUEUE/RUNNING/UNCLASSIFIED/TEST_RUN/ERROR/DELETED runs => None
            migrationBuilder.Sql("""
                UPDATE
                    run
                SET
                    run.billing_run_status = 'None'
                FROM
                    dbo.calculator_run AS run
                WHERE
                    run.billing_run_status IS NULL AND
                    run.calculator_run_classification_id in (1,2,3,4,5,6)
                """);

            // Remaining runs => Unknown
            // At this point, this should be a no-op. Just accounting for any edge cases due to dodgy data.
            migrationBuilder.Sql("""
                UPDATE
                    run
                SET
                    run.billing_run_status = 'Unknown'
                FROM
                    dbo.calculator_run AS run
                WHERE
                    run.billing_run_status IS NULL
                """);

            migrationBuilder.AlterColumn<string>(
                name: "billing_run_status",
                table: "calculator_run",
                maxLength: 50,
                nullable: false,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "billing_json_filename",
                table: "calculator_run_billing_file_metadata",
                maxLength: 400,
                nullable: false,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "billing_csv_filename",
                table: "calculator_run_billing_file_metadata",
                maxLength: 400,
                nullable: false,
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "is_billing_file_generating",
                table: "calculator_run");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_billing_file_metadata_calculator_run_id",
                table: "calculator_run_billing_file_metadata",
                column: "calculator_run_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_calculator_run_classification_id",
                table: "calculator_run",
                column: "calculator_run_classification_id");

            migrationBuilder.CreateIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run",
                columns: new[] { "relative_year", "calculator_run_classification_id", "billing_run_status", "id" })
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "name", "created_by", "created_at", "updated_by", "updated_at", "calculator_run_organization_data_master_id", "calculator_run_pom_data_master_id", "default_parameter_setting_master_id", "lapcap_data_master_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run");

            migrationBuilder.DropIndex(
                name: "IX_calculator_run_calculator_run_classification_id",
                table: "calculator_run");

            migrationBuilder.DropIndex(
                name: "IX_calculator_run_billing_file_metadata_calculator_run_id",
                table: "calculator_run_billing_file_metadata");

            migrationBuilder.AddColumn<bool>(
                name: "is_billing_file_generating",
                table: "calculator_run",
                type: "bit",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE
                    run
                SET
                    run.is_billing_file_generating = 1
                FROM
                    dbo.calculator_run AS run
                WHERE
                    run.is_billing_file_generating IS NULL AND
                    run.billing_run_status = 'Errored'
                """);

            migrationBuilder.Sql("""
                UPDATE
                    run
                SET
                    run.is_billing_file_generating = 0
                FROM
                    dbo.calculator_run AS run
                WHERE
                    run.is_billing_file_generating IS NULL AND
                    run.billing_run_status = 'Completed'
                """);

            migrationBuilder.DropColumn(
                name: "billing_run_started_at",
                table: "calculator_run");

            migrationBuilder.DropColumn(
                name: "billing_run_status",
                table: "calculator_run");

            migrationBuilder.AlterColumn<string>(
                name: "billing_json_filename",
                table: "calculator_run_billing_file_metadata",
                maxLength: 400,
                nullable: true,
                oldNullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "billing_csv_filename",
                table: "calculator_run_billing_file_metadata",
                maxLength: 400,
                nullable: true,
                oldNullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_billing_file_metadata_calculator_run_id",
                table: "calculator_run_billing_file_metadata",
                column: "calculator_run_id");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_relative_year",
                table: "calculator_run",
                column: "relative_year");

            migrationBuilder.CreateIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run",
                columns: new[] { "calculator_run_classification_id", "relative_year", "is_billing_file_generating", "id" })
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "name", "created_by", "created_at", "updated_by", "updated_at", "calculator_run_organization_data_master_id", "calculator_run_pom_data_master_id", "default_parameter_setting_master_id", "lapcap_data_master_id" });
        }
    }
}
