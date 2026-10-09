using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EPR.Calculator.API.Data.Migrations;

/// <summary>
///     Splits the legacy <c>calculator_run_classification</c> lookup into separate concerns on <c>calculator_run</c>:
///     <list type="bullet">
///         <item><c>classification</c> - the (user designated) run classification, persisted as a string.</item>
///         <item><c>calculation_run_status</c> - the calculation run lifecycle, persisted as a string.</item>
///         <item>
///             <c>is_billing_file_shared</c>, <c>billing_file_shared_by</c> and <c>billing_file_shared_at</c> - replace the
///             'completed' classifications, and the 'authorised' columns of <c>calculator_run_billing_file_metadata</c>.
///         </item>
///     </list>
///     It also renames the <c>Running</c> billing run status to <c>Started</c>.
/// </summary>
/// <remarks>
///     Raw SQL is wrapped in EXEC() so that SQL Server resolves column names when it runs, rather than when the batch
///     containing it is compiled (see the <c>20260603161538_BillingRunStatus</c> migration for details).
///     Values are written literally because this migration must keep working as the enums evolve.
/// </remarks>
public partial class SplitRunClassification : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_calculator_run_calculator_run_classification_calculator_run_classification_id",
            table: "calculator_run");

        migrationBuilder.DropTable(
            name: "calculator_run_classification");

        // nullable=true to allow for existing runs to be migrated
        migrationBuilder.AddColumn<string>(
            name: "calculation_run_status",
            table: "calculator_run",
            type: "varchar(50)",
            unicode: false,
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "is_billing_file_shared",
            table: "calculator_run",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "billing_file_shared_by",
            table: "calculator_run",
            type: "nvarchar(400)",
            maxLength: 400,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "billing_file_shared_at",
            table: "calculator_run",
            type: "datetime2",
            nullable: true);

        // Converts the existing ids to their string representation (e.g. 12 => '12'), which are then remapped below.
        // IX_index_calculator_run is dropped and recreated around the type change.
        migrationBuilder.RenameColumn(
            name: "calculator_run_classification_id",
            table: "calculator_run",
            newName: "classification");

        migrationBuilder.AlterColumn<string>(
            name: "classification",
            table: "calculator_run",
            type: "varchar(50)",
            unicode: false,
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int");

        UpSplitClassification(migrationBuilder);
        UpMoveBillingFileSharedByAndAt(migrationBuilder);

        // nullable=false
        migrationBuilder.AlterColumn<string>(
            name: "calculation_run_status",
            table: "calculator_run",
            type: "varchar(50)",
            unicode: false,
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(50)",
            oldUnicode: false,
            oldMaxLength: 50,
            oldNullable: true);

        migrationBuilder.DropColumn(
            name: "billing_file_authorised_by",
            table: "calculator_run_billing_file_metadata");

        migrationBuilder.DropColumn(
            name: "billing_file_authorised_date",
            table: "calculator_run_billing_file_metadata");

        // BillingRunStatus.Running has been renamed to BillingRunStatus.Started
        migrationBuilder.Sql("""
            EXEC(N'
            UPDATE
                dbo.calculator_run
            SET
                billing_run_status = ''Started''
            WHERE
                billing_run_status = ''Running''
            ')
            """);
    }

    private static void UpSplitClassification(MigrationBuilder migrationBuilder)
    {
        // Each legacy classification id maps onto a combination of the new columns
        migrationBuilder.Sql("""
            EXEC(N'
            UPDATE
                run
            SET
                run.classification = map.classification,
                run.calculation_run_status = map.calculation_run_status,
                run.is_billing_file_shared = map.is_billing_file_shared
            FROM
                dbo.calculator_run AS run
            INNER JOIN
                (VALUES
                    (''2'',  ''None'',          ''Started'',   0), -- RUNNING
                    (''3'',  ''None'',          ''Completed'', 0), -- UNCLASSIFIED
                    (''4'',  ''Test'',          ''Completed'', 0), -- TEST RUN
                    (''5'',  ''None'',          ''Errored'',   0), -- ERROR
                    (''6'',  ''Deleted'',       ''Completed'', 0), -- DELETED
                    (''7'',  ''Initial'',       ''Completed'', 1), -- INITIAL RUN COMPLETED
                    (''8'',  ''Initial'',       ''Completed'', 0), -- INITIAL RUN
                    (''9'',  ''Recalculation'', ''Completed'', 0), -- RECALCULATION RUN
                    (''12'', ''Recalculation'', ''Completed'', 1)  -- RECALCULATION RUN COMPLETED
                ) AS map (legacy_id, classification, calculation_run_status, is_billing_file_shared)
                ON map.legacy_id = run.classification
            ')
            """);

        // All remaining runs:
        // Set classification/calculation_run_status=Unknown (this should be a no-op, the dropped foreign key prevented
        // any other ids; just accounting for edge cases)
        migrationBuilder.Sql("""
            EXEC(N'
            UPDATE
                dbo.calculator_run
            SET
                classification = ''Unknown'',
                calculation_run_status = ''Unknown''
            WHERE
                calculation_run_status IS NULL
            ')
            """);
    }

    private static void UpMoveBillingFileSharedByAndAt(MigrationBuilder migrationBuilder)
    {
        // The 'authorised' values were set on the billing file metadata when the billing file was sent to FSS, i.e. when
        // the run was given a 'completed' classification. Only copied for those runs, so that billing_file_shared_by/at
        // are only ever set when is_billing_file_shared=1.
        migrationBuilder.Sql("""
            EXEC(N'
            UPDATE
                run
            SET
                run.billing_file_shared_by = metadata.billing_file_authorised_by,
                run.billing_file_shared_at = metadata.billing_file_authorised_date
            FROM
                dbo.calculator_run AS run
            CROSS APPLY
                (
                    SELECT TOP (1)
                        m.billing_file_authorised_by,
                        m.billing_file_authorised_date
                    FROM
                        dbo.calculator_run_billing_file_metadata AS m
                    WHERE
                        m.calculator_run_id = run.id
                        AND m.billing_file_authorised_date IS NOT NULL
                    ORDER BY
                        m.billing_file_authorised_date DESC,
                        m.id DESC
                ) AS metadata
            WHERE
                run.is_billing_file_shared = 1
            ')
            """);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Runs are mapped back onto the legacy classification ids. This is lossy for runs created after <c>Up</c>:
    ///     queued runs (calculation_run_status=None) become 'RUNNING', as that's how runs were previously created.
    /// </remarks>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // BillingRunStatus.Started was previously BillingRunStatus.Running
        migrationBuilder.Sql("""
            EXEC(N'
            UPDATE
                dbo.calculator_run
            SET
                billing_run_status = ''Running''
            WHERE
                billing_run_status = ''Started''
            ')
            """);

        migrationBuilder.AddColumn<string>(
            name: "billing_file_authorised_by",
            table: "calculator_run_billing_file_metadata",
            type: "nvarchar(400)",
            maxLength: 400,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "billing_file_authorised_date",
            table: "calculator_run_billing_file_metadata",
            type: "datetime2",
            nullable: true);

        DownMoveBillingFileSharedByAndAt(migrationBuilder);
        DownMergeClassification(migrationBuilder);

        migrationBuilder.DropColumn(
            name: "billing_file_shared_at",
            table: "calculator_run");

        migrationBuilder.DropColumn(
            name: "billing_file_shared_by",
            table: "calculator_run");

        migrationBuilder.DropColumn(
            name: "calculation_run_status",
            table: "calculator_run");

        migrationBuilder.DropColumn(
            name: "is_billing_file_shared",
            table: "calculator_run");

        // Converts the legacy ids set above back to int.
        // IX_index_calculator_run is dropped and recreated around the type change.
        migrationBuilder.RenameColumn(
            name: "classification",
            table: "calculator_run",
            newName: "calculator_run_classification_id");

        migrationBuilder.AlterColumn<int>(
            name: "calculator_run_classification_id",
            table: "calculator_run",
            type: "int",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(50)",
            oldUnicode: false,
            oldMaxLength: 50);

        migrationBuilder.CreateTable(
            name: "calculator_run_classification",
            columns: table => new
            {
                id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                created_by = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                status = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_calculator_run_classification", x => x.id);
            });

        migrationBuilder.InsertData(
            table: "calculator_run_classification",
            columns: new[] { "id", "created_by", "status" },
            values: new object[,]
            {
                { 2, "System User", "RUNNING" },
                { 3, "System User", "UNCLASSIFIED" },
                { 4, "System User", "TEST RUN" },
                { 5, "System User", "ERROR" },
                { 6, "System User", "DELETED" },
                { 7, "System User", "INITIAL RUN COMPLETED" },
                { 8, "System User", "INITIAL RUN" },
                { 9, "System User", "RECALCULATION RUN" },
                { 12, "System User", "RECALCULATION RUN COMPLETED" }
            });

        migrationBuilder.AddForeignKey(
            name: "FK_calculator_run_calculator_run_classification_calculator_run_classification_id",
            table: "calculator_run",
            column: "calculator_run_classification_id",
            principalTable: "calculator_run_classification",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }

    private static void DownMoveBillingFileSharedByAndAt(MigrationBuilder migrationBuilder)
    {
        // Moved back onto the latest billing file metadata, i.e. the billing file that would have been shared
        migrationBuilder.Sql("""
            EXEC(N'
            UPDATE
                metadata
            SET
                metadata.billing_file_authorised_by = run.billing_file_shared_by,
                metadata.billing_file_authorised_date = run.billing_file_shared_at
            FROM
                dbo.calculator_run_billing_file_metadata AS metadata
            INNER JOIN
                dbo.calculator_run AS run
                ON run.id = metadata.calculator_run_id
            WHERE
                run.is_billing_file_shared = 1
                AND metadata.id = (
                    SELECT TOP (1)
                        latest.id
                    FROM
                        dbo.calculator_run_billing_file_metadata AS latest
                    WHERE
                        latest.calculator_run_id = run.id
                    ORDER BY
                        latest.billing_file_created_date DESC,
                        latest.id DESC
                )
            ')
            """);
    }

    private static void DownMergeClassification(MigrationBuilder migrationBuilder)
    {
        // Official classifications take precedence, then Test/Deleted; any remaining (unclassified) runs are mapped by
        // their calculation run status, with anything unexpected treated as errored.
        migrationBuilder.Sql("""
            EXEC(N'
            UPDATE
                dbo.calculator_run
            SET
                classification =
                    CASE
                        WHEN classification = ''Initial'' AND is_billing_file_shared = 1       THEN ''7''  -- INITIAL RUN COMPLETED
                        WHEN classification = ''Initial''                                      THEN ''8''  -- INITIAL RUN
                        WHEN classification = ''Recalculation'' AND is_billing_file_shared = 1 THEN ''12'' -- RECALCULATION RUN COMPLETED
                        WHEN classification = ''Recalculation''                                THEN ''9''  -- RECALCULATION RUN
                        WHEN classification = ''Test''                                         THEN ''4''  -- TEST RUN
                        WHEN classification = ''Deleted''                                      THEN ''6''  -- DELETED
                        WHEN calculation_run_status IN (''None'', ''Started'')                 THEN ''2''  -- RUNNING
                        WHEN calculation_run_status = ''Completed''                            THEN ''3''  -- UNCLASSIFIED
                        ELSE ''5''                                                                         -- ERROR
                    END
            ')
            """);
    }
}
