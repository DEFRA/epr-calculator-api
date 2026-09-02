using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceOrgPomStagingWithCalculatorRunOrganisation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_calculator_run_calculator_run_organization_data_master_calculator_run_organization_data_master_id",
                table: "calculator_run");

            migrationBuilder.DropForeignKey(
                name: "FK_calculator_run_calculator_run_pom_data_master_calculator_run_pom_data_master_id",
                table: "calculator_run");

            migrationBuilder.DropTable(
                name: "calculator_run_organization_data_detail");

            migrationBuilder.DropTable(
                name: "calculator_run_pom_data_detail");

            migrationBuilder.DropTable(
                name: "organisation_data");

            migrationBuilder.DropTable(
                name: "pom_data");

            migrationBuilder.DropTable(
                name: "calculator_run_organization_data_master");

            migrationBuilder.DropTable(
                name: "calculator_run_pom_data_master");

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
                name: "obligation_status",
                table: "producer_detail",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status_code",
                table: "producer_detail",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "submitter_id",
                table: "producer_detail",
                type: "uniqueidentifier",
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
                    submitter_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    organisation_name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    trading_name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    obligation_status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    num_days_obligated = table.Column<int>(type: "int", nullable: true),
                    joiner_date = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    leaver_date = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    status_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    error_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    has_h1 = table.Column<bool>(type: "bit", nullable: false),
                    has_h2 = table.Column<bool>(type: "bit", nullable: false)
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
                name: "IX_index_calculator_run",
                table: "calculator_run",
                columns: new[] { "calculator_run_classification_id", "relative_year", "billing_run_status", "id" })
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "name", "created_by", "created_at", "updated_by", "updated_at", "default_parameter_setting_master_id", "lapcap_data_master_id" });

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_organisation_calculator_run_id",
                table: "calculator_run_organisation",
                column: "calculator_run_id");
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
                name: "obligation_status",
                table: "producer_detail");

            migrationBuilder.DropColumn(
                name: "status_code",
                table: "producer_detail");

            migrationBuilder.DropColumn(
                name: "submitter_id",
                table: "producer_detail");

            migrationBuilder.DropColumn(
                name: "org_pom_data_loaded_at",
                table: "calculator_run");

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

            migrationBuilder.CreateTable(
                name: "calculator_run_organization_data_master",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    effective_from = table.Column<DateTime>(type: "datetime2", nullable: false),
                    effective_to = table.Column<DateTime>(type: "datetime2", nullable: true),
                    relative_year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculator_run_organization_data_master", x => x.id);
                    table.ForeignKey(
                        name: "FK_calculator_run_organization_data_master_calculator_run_relative_years_relative_year",
                        column: x => x.relative_year,
                        principalTable: "calculator_run_relative_years",
                        principalColumn: "relative_year",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "calculator_run_pom_data_master",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    effective_from = table.Column<DateTime>(type: "datetime2", nullable: false),
                    effective_to = table.Column<DateTime>(type: "datetime2", nullable: true),
                    relative_year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculator_run_pom_data_master", x => x.id);
                    table.ForeignKey(
                        name: "FK_calculator_run_pom_data_master_calculator_run_relative_years_relative_year",
                        column: x => x.relative_year,
                        principalTable: "calculator_run_relative_years",
                        principalColumn: "relative_year",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "calculator_run_organization_data_detail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_organization_data_master_id = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_calculator_run_organization_data_detail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_calculator_run_organization_data_detail_calculator_run_organization_data_master_calculator_run_organization_data_master_id",
                        column: x => x.calculator_run_organization_data_master_id,
                        principalTable: "calculator_run_organization_data_master",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "calculator_run_pom_data_detail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_pom_data_master_id = table.Column<int>(type: "int", nullable: false),
                    load_ts = table.Column<DateTime>(type: "datetime2", nullable: false),
                    organisation_id = table.Column<int>(type: "int", nullable: true),
                    packaging_activity = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    packaging_class = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    packaging_material = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
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
                    table.PrimaryKey("PK_calculator_run_pom_data_detail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_calculator_run_pom_data_detail_calculator_run_pom_data_master_calculator_run_pom_data_master_id",
                        column: x => x.calculator_run_pom_data_master_id,
                        principalTable: "calculator_run_pom_data_master",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_organization_data_detail_calculator_run_organization_data_master_id",
                table: "calculator_run_organization_data_detail",
                column: "calculator_run_organization_data_master_id");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_organization_data_master_relative_year",
                table: "calculator_run_organization_data_master",
                column: "relative_year");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_pom_data_detail_calculator_run_pom_data_master_id",
                table: "calculator_run_pom_data_detail",
                column: "calculator_run_pom_data_master_id");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_pom_data_master_relative_year",
                table: "calculator_run_pom_data_master",
                column: "relative_year");

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
