using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <summary>
    ///     Replaces the per-run organisation/POM staging tables with calculator_run_organisation and
    ///     the DataApi load tables. The tables that carry historical run data are kept as a <c>_bk</c>
    ///     copy (renamed, not copied, so their data, keys and identity survive untouched), and
    ///     <see cref="Down" /> restores them by renaming back rather than recreating empty structures.
    ///     organisation_data/pom_data are a disposable latest-load cache, not per-run history, so they
    ///     are dropped outright and <see cref="Down" /> only recreates their (empty) structure.
    /// </summary>
    public partial class DataApiSchemaChanges : Migration
    {
        // Renamed: the table is fully retired by this migration, so sp_rename can move it aside intact
        // (data, keys and identity untouched) and Down() can rename it straight back. calculator_run
        // keeps being used under its own name both later in this migration and forever after, so it
        // can't be renamed aside the same way - it is copied instead, and restored in Down() by adding
        // the dropped columns back and updating them from the copy, not by renaming.
        private static readonly (string Table, bool Renamed)[] BackedUpTables =
        [
            ("calculator_run_organization_data_detail", true),
            ("calculator_run_pom_data_detail", true),
            ("calculator_run_organization_data_master", true),
            ("calculator_run_pom_data_master", true),
            ("calculator_run", false)
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "joiner_date",
                table: "producer_detail",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "leaver_date",
                table: "producer_detail",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "num_days_obligated",
                table: "producer_detail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status_code",
                table: "producer_detail",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "org_pom_data_loaded_at",
                table: "calculator_run",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "calculator_run_organisation",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    organisation_id = table.Column<int>(type: "int", nullable: false),
                    subsidiary_id = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    organisation_name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    trading_name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    num_days_obligated = table.Column<int>(type: "int", nullable: true),
                    joiner_date = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    leaver_date = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    status_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    error_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_error = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculator_run_organisation", x => x.id);
                    table.ForeignKey(
                        name: "FK_calculator_run_organisation_calculator_run_calculator_run_id",
                        column: x => x.calculator_run_id,
                        principalTable: "calculator_run",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_organisation_calculator_run_id",
                table: "calculator_run_organisation",
                column: "calculator_run_id");

            // Carry historical run data forward. Each run had its own master row, so created_at is the
            // run's org/POM load time.
            migrationBuilder.Sql(@"
                UPDATE r
                SET org_pom_data_loaded_at = m.created_at
                FROM calculator_run r
                INNER JOIN calculator_run_organization_data_master m
                    ON m.id = r.calculator_run_organization_data_master_id;");

            // is_error is derived from the old obligation status: an "E" organisation was one excluded
            // from calculation by a hard error, which is what is_error now records.
            migrationBuilder.Sql(@"
                INSERT INTO calculator_run_organisation
                    (calculator_run_id, organisation_id, subsidiary_id, organisation_name,
                     trading_name, num_days_obligated, joiner_date, leaver_date,
                     status_code, error_code, is_error)
                SELECT
                    r.id, d.organisation_id, d.subsidiary_id, d.organisation_name,
                    d.trading_name, d.num_days_obligated, d.joiner_date, d.leaver_date,
                    d.status_code, d.error_code,
                    CASE WHEN d.obligation_status = 'E' THEN 1 ELSE 0 END
                FROM calculator_run_organization_data_detail d
                INNER JOIN calculator_run r
                    ON r.calculator_run_organization_data_master_id = d.calculator_run_organization_data_master_id;");

            // The obligation columns moved onto producer_detail; backfill historical runs from the
            // matching organisation-detail row so consumers (e.g. partial-obligation export) still work.
            migrationBuilder.Sql(@"
                UPDATE pd
                SET num_days_obligated = d.num_days_obligated,
                    status_code = d.status_code,
                    joiner_date = d.joiner_date,
                    leaver_date = d.leaver_date
                FROM producer_detail pd
                INNER JOIN calculator_run r
                    ON r.id = pd.calculator_run_id
                INNER JOIN calculator_run_organization_data_detail d
                    ON d.calculator_run_organization_data_master_id = r.calculator_run_organization_data_master_id
                   AND d.organisation_id = pd.producer_id
                   AND ISNULL(d.subsidiary_id, '') = ISNULL(pd.subsidiary_id, '');");

            // See the comment on BackedUpTables for renamed vs. copied.
            foreach (var (table, renamed) in BackedUpTables)
                migrationBuilder.Sql(renamed
                    ? $"EXEC sp_rename N'{table}', N'{table}_bk';"
                    : $"SELECT * INTO {table}_bk FROM {table};");

            migrationBuilder.DropForeignKey(
                name: "FK_calculator_run_calculator_run_organization_data_master_calculator_run_organization_data_master_id",
                table: "calculator_run");

            migrationBuilder.DropForeignKey(
                name: "FK_calculator_run_calculator_run_pom_data_master_calculator_run_pom_data_master_id",
                table: "calculator_run");

            migrationBuilder.DropIndex(
                name: "IX_calculator_run_calculator_run_organization_data_master_id",
                table: "calculator_run");

            migrationBuilder.DropIndex(
                name: "IX_calculator_run_calculator_run_pom_data_master_id",
                table: "calculator_run");

            migrationBuilder.DropIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run");

            migrationBuilder.DropColumn(
                name: "calculator_run_organization_data_master_id",
                table: "calculator_run");

            migrationBuilder.DropColumn(
                name: "calculator_run_pom_data_master_id",
                table: "calculator_run");

            migrationBuilder.CreateIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run",
                columns: new[] { "calculator_run_classification_id", "relative_year", "billing_run_status", "id" })
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "name", "created_by", "created_at", "updated_by", "updated_at", "default_parameter_setting_master_id", "lapcap_data_master_id" });

            // A disposable cache of the latest-loaded organisation/POM data, superseded by the DataApi
            // load tables below - not per-run history, so no backup is needed.
            migrationBuilder.DropTable(
                name: "organisation_data");

            migrationBuilder.DropTable(
                name: "pom_data");

            // DataApi staging tables. Owned by EPR.Calculator.API.DataApi's DataApiLoadContext (which
            // excludes them from its own migrations); created here because that is the only migration
            // pipeline the app database has. Raw SQL so they stay out of ApplicationDBContext's model
            // snapshot - EF must never try to manage or drop them.
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[data_api_load_organisations]', N'U') IS NULL
                CREATE TABLE [data_api_load_organisations] (
                    [id] int NOT NULL IDENTITY,
                    [organisation_id] int NOT NULL,
                    [subsidiary_id] nvarchar(400) NULL,
                    [submitter_id] nvarchar(400) NULL,
                    [organisation_name] nvarchar(400) NOT NULL,
                    [trading_name] nvarchar(400) NULL,
                    [status_code] nvarchar(400) NULL,
                    [leaver_date] nvarchar(50) NULL,
                    [joiner_date] nvarchar(50) NULL,
                    [regulator_status] nvarchar(50) NOT NULL,
                    [obligation_status] nvarchar(10) NULL,
                    [num_days_obligated] smallint NULL,
                    [error_code] nvarchar(max) NULL,
                    [submission_period_year] int NOT NULL,
                    [has_h1] bit NOT NULL,
                    [has_h2] bit NOT NULL,
                    [file_name] nvarchar(400) NULL,
                    [is_resubmission] bit NOT NULL,
                    [created_date_time] datetime2 NULL,
                    [load_ts] datetime2 NOT NULL,
                    CONSTRAINT [PK_data_api_load_organisations] PRIMARY KEY ([id])
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[data_api_load_poms]', N'U') IS NULL
                CREATE TABLE [data_api_load_poms] (
                    [id] bigint NOT NULL IDENTITY,
                    [organisation_id] int NOT NULL,
                    [subsidiary_id] nvarchar(400) NULL,
                    [submitter_id] nvarchar(400) NULL,
                    [submission_period] nvarchar(400) NOT NULL,
                    [submission_period_desc] nvarchar(400) NULL,
                    [packaging_activity] nvarchar(400) NULL,
                    [packaging_type] nvarchar(400) NULL,
                    [packaging_class] nvarchar(400) NULL,
                    [packaging_material] nvarchar(400) NULL,
                    [packaging_material_subtype] nvarchar(400) NULL,
                    [packaging_material_weight] float NULL,
                    [ram_rag_rating] nvarchar(50) NULL,
                    [file_name] nvarchar(400) NULL,
                    [is_resubmission] bit NOT NULL,
                    [created_date_time] datetime2 NULL,
                    [load_ts] datetime2 NOT NULL,
                    CONSTRAINT [PK_data_api_load_poms] PRIMARY KEY ([id])
                );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calculator_run_organisation");

            migrationBuilder.DropIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run");

            migrationBuilder.DropColumn(
                name: "joiner_date",
                table: "producer_detail");

            migrationBuilder.DropColumn(
                name: "leaver_date",
                table: "producer_detail");

            migrationBuilder.DropColumn(
                name: "num_days_obligated",
                table: "producer_detail");

            migrationBuilder.DropColumn(
                name: "status_code",
                table: "producer_detail");

            migrationBuilder.DropColumn(
                name: "org_pom_data_loaded_at",
                table: "calculator_run");

            // Restore the renamed tables, with their data, by renaming them back. calculator_run was
            // copied rather than renamed (see the comment on BackedUpTables), so it is restored further
            // down instead, by adding its dropped columns back and updating them from calculator_run_bk.
            for (var i = BackedUpTables.Length - 1; i >= 0; i--)
                if (BackedUpTables[i].Renamed)
                    migrationBuilder.Sql($"EXEC sp_rename N'{BackedUpTables[i].Table}_bk', N'{BackedUpTables[i].Table}';");

            // organisation_data/pom_data were dropped outright in Up (see the comment there) - recreate
            // their structure only; the pre-upgrade data was a disposable cache, not history to restore.
            migrationBuilder.CreateTable(
                name: "organisation_data",
                columns: table => new
                {
                    num_days_obligated = table.Column<int>(type: "int", nullable: true),
                    error_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    has_h1 = table.Column<bool>(type: "bit", nullable: false),
                    has_h2 = table.Column<bool>(type: "bit", nullable: false),
                    joiner_date = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    leaver_date = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    load_ts = table.Column<DateTime>(type: "datetime2", nullable: false),
                    obligation_status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    organisation_id = table.Column<int>(type: "int", nullable: false),
                    organisation_name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    status_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    submitter_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    subsidiary_id = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    trading_name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "pom_data",
                columns: table => new
                {
                    load_ts = table.Column<DateTime>(type: "datetime2", nullable: false),
                    organisation_id = table.Column<int>(type: "int", nullable: true),
                    packaging_activity = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    packaging_class = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    packaging_material = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    packaging_material_subtype = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    packaging_material_weight = table.Column<double>(type: "float", nullable: true),
                    packaging_type = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ram_rag_rating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    submission_period = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    submission_period_desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    submitter_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    subsidiary_id = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.AddColumn<int>(
                name: "calculator_run_organization_data_master_id",
                table: "calculator_run",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "calculator_run_pom_data_master_id",
                table: "calculator_run",
                type: "int",
                nullable: true);

            // Runs created after the upgrade have no master rows and stay NULL.
            migrationBuilder.Sql(@"
                UPDATE r
                SET calculator_run_organization_data_master_id = b.calculator_run_organization_data_master_id,
                    calculator_run_pom_data_master_id = b.calculator_run_pom_data_master_id
                FROM calculator_run r
                INNER JOIN calculator_run_bk b
                    ON b.id = r.id;");

            migrationBuilder.DropTable(
                name: "calculator_run_bk");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_calculator_run_organization_data_master_id",
                table: "calculator_run",
                column: "calculator_run_organization_data_master_id");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_calculator_run_pom_data_master_id",
                table: "calculator_run",
                column: "calculator_run_pom_data_master_id");

            migrationBuilder.CreateIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run",
                columns: new[] { "calculator_run_classification_id", "relative_year", "billing_run_status", "id" })
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "name", "created_by", "created_at", "updated_by", "updated_at", "calculator_run_organization_data_master_id", "calculator_run_pom_data_master_id", "default_parameter_setting_master_id", "lapcap_data_master_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_calculator_run_calculator_run_organization_data_master_calculator_run_organization_data_master_id",
                table: "calculator_run",
                column: "calculator_run_organization_data_master_id",
                principalTable: "calculator_run_organization_data_master",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_calculator_run_calculator_run_pom_data_master_calculator_run_pom_data_master_id",
                table: "calculator_run",
                column: "calculator_run_pom_data_master_id",
                principalTable: "calculator_run_pom_data_master",
                principalColumn: "id");
        }
    }
}
