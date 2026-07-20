using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class BillingRunStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            UpHandleDodgyBillingMetadata(migrationBuilder);

            migrationBuilder.DropIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run");

            migrationBuilder.AddColumn<DateTime>(
                name: "billing_run_started_at",
                table: "calculator_run",
                type: "datetime2",
                nullable: true);

            // nullable=true to allow for existing runs to be migrated
            migrationBuilder.AddColumn<string>(
                name: "billing_run_status",
                table: "calculator_run",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            UpSetBillingRunStatus(migrationBuilder);

            // nullable=false
            migrationBuilder.AlterColumn<string>(
                name: "billing_run_status",
                table: "calculator_run",
                type: "varchar(50)",
                nullable: false,
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "is_billing_file_generating",
                table: "calculator_run");

            migrationBuilder.CreateIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run",
                columns: new[] { "calculator_run_classification_id", "relative_year", "billing_run_status", "id" })
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "name", "created_by", "created_at", "updated_by", "updated_at", "calculator_run_organization_data_master_id", "calculator_run_pom_data_master_id", "default_parameter_setting_master_id", "lapcap_data_master_id" });
        }

        private static void UpHandleDodgyBillingMetadata(MigrationBuilder migrationBuilder)
        {
            // Delete metadata rows with null filenames
            // Only present on DEV/TST
            migrationBuilder.Sql("""
                DELETE FROM
                	dbo.calculator_run_billing_file_metadata
                WHERE
                	billing_csv_filename IS NULL OR billing_json_filename IS NULL
                """);

            // nullable=false
            migrationBuilder.AlterColumn<string>(
                name: "billing_json_filename",
                table: "calculator_run_billing_file_metadata",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400,
                oldNullable: true);

            // nullable=false
            migrationBuilder.AlterColumn<string>(
                name: "billing_csv_filename",
                table: "calculator_run_billing_file_metadata",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400,
                oldNullable: true);

            // Delete multiple metadata rows (only 1 supported currently)
            // Only present on DEV/TST/PRE1
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
        }

        private static void UpSetBillingRunStatus(MigrationBuilder migrationBuilder)
        {
            // Runs with billing file metadata:
            // Set billing_run_status=Completed
            migrationBuilder.Sql("""
                UPDATE
                    run
                SET
                    run.billing_run_status = 'Completed',
                    run.billing_run_started_at = DATEADD(MINUTE, -15, metadata.billing_file_created_date)
                FROM
                    dbo.calculator_run AS run
                INNER JOIN
                    dbo.calculator_run_billing_file_metadata AS metadata
                    ON metadata.calculator_run_id = run.id
                WHERE
                    run.billing_run_status IS NULL
                """);

            // Remaining runs with is_billing_file_generating=true:
            // Set billing_run_status=Errored
            // Wrapped in EXEC() so SQL Server defers column name resolution to runtime.
            // is_billing_file_generating is dropped later in this migration; without EXEC()
            // the IF NOT EXISTS guard in the generated script does not protect against
            // Msg 207 (invalid column name) because SQL Server validates DML column
            // references at compile time, before evaluating the IF condition.
            migrationBuilder.Sql("""
                EXEC(N'UPDATE run
                SET run.billing_run_status = ''Errored''
                FROM dbo.calculator_run AS run
                WHERE run.billing_run_status IS NULL
                AND run.is_billing_file_generating = 1')
                """);

            // Remaining runs with IN_THE_QUEUE/RUNNING/UNCLASSIFIED/TEST_RUN/ERROR/DELETED run status:
            // Set billing_run_status=None
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

            // All remaining runs:
            // Set billing_run_status=Unknown (at this point, this should be a no-op, just accounting for edge cases)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run");

            migrationBuilder.AddColumn<bool>(
                name: "is_billing_file_generating",
                table: "calculator_run",
                type: "bit",
                nullable: true);

            DownSetIsBillingFileGenerating(migrationBuilder);

            migrationBuilder.DropColumn(
                name: "billing_run_started_at",
                table: "calculator_run");

            migrationBuilder.DropColumn(
                name: "billing_run_status",
                table: "calculator_run");

            migrationBuilder.CreateIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run",
                columns: new[] { "calculator_run_classification_id", "relative_year", "is_billing_file_generating", "id" })
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "name", "created_by", "created_at", "updated_by", "updated_at", "calculator_run_organization_data_master_id", "calculator_run_pom_data_master_id", "default_parameter_setting_master_id", "lapcap_data_master_id" });
        }

        private static void DownSetIsBillingFileGenerating(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.Sql("""
                UPDATE
                    run
                SET
                    run.is_billing_file_generating = 1
                FROM
                    dbo.calculator_run AS run
                WHERE
                    run.is_billing_file_generating IS NULL AND
                    run.billing_run_status not in ('Unknown', 'None')
                """);
        }
    }
}
