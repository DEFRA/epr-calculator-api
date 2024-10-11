using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class CalcRunTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<int>(
                name: "default_parameter_setting_master_id",
                table: "calculator_run",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "lapcap_data_master_id",
                table: "calculator_run",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "calculator_run_organization_data_master",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calendar_year = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    effective_from = table.Column<DateTime>(type: "datetime2", nullable: false),
                    effective_to = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculator_run_organization_data_master", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calculator_run_pom_data_master",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calendar_year = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    effective_from = table.Column<DateTime>(type: "datetime2", nullable: false),
                    effective_to = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculator_run_pom_data_master", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organization_data",
                columns: table => new
                {
                    organisation_id = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    subsidiary_id = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    organisation_name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    load_ts = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_data", x => new { x.organisation_id, x.subsidiary_id });
                });

            migrationBuilder.CreateTable(
                name: "pom_data",
                columns: table => new
                {
                    organisation_id = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    subsidiary_id = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    submission_period = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    packaging_activity = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    packaging_type = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    packaging_class = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    packaging_material = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    packaging_material_weight = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    load_ts = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pom_data", x => new { x.organisation_id, x.subsidiary_id });
                });

            migrationBuilder.CreateTable(
                name: "calculator_run_organization_data_detail",
                columns: table => new
                {
                    organisation_id = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    subsidiary_id = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    organisation_name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    load_ts = table.Column<DateTime>(type: "datetime2", nullable: false),
                    calculator_run_organization_data_master_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculator_run_organization_data_detail", x => new { x.organisation_id, x.subsidiary_id });
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
                    organisation_id = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    subsidiary_id = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    submission_period = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    packaging_activity = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    packaging_type = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    packaging_class = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    packaging_material = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    packaging_material_weight = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    load_ts = table.Column<DateTime>(type: "datetime2", nullable: false),
                    calculator_run_pom_data_master_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculator_run_pom_data_detail", x => new { x.organisation_id, x.subsidiary_id });
                    table.ForeignKey(
                        name: "FK_calculator_run_pom_data_detail_calculator_run_pom_data_master_calculator_run_pom_data_master_id",
                        column: x => x.calculator_run_pom_data_master_id,
                        principalTable: "calculator_run_pom_data_master",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 10, 4, 14, 46, 49, 179, DateTimeKind.Local).AddTicks(461));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_at",
                value: new DateTime(2024, 10, 4, 14, 46, 49, 179, DateTimeKind.Local).AddTicks(464));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_at",
                value: new DateTime(2024, 10, 4, 14, 46, 49, 179, DateTimeKind.Local).AddTicks(466));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_at",
                value: new DateTime(2024, 10, 4, 14, 46, 49, 179, DateTimeKind.Local).AddTicks(469));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_at",
                value: new DateTime(2024, 10, 4, 14, 46, 49, 179, DateTimeKind.Local).AddTicks(471));

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_calculator_run_organization_data_master_id",
                table: "calculator_run",
                column: "calculator_run_organization_data_master_id");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_calculator_run_pom_data_master_id",
                table: "calculator_run",
                column: "calculator_run_pom_data_master_id");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_default_parameter_setting_master_id",
                table: "calculator_run",
                column: "default_parameter_setting_master_id");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_lapcap_data_master_id",
                table: "calculator_run",
                column: "lapcap_data_master_id");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_organization_data_detail_calculator_run_organization_data_master_id",
                table: "calculator_run_organization_data_detail",
                column: "calculator_run_organization_data_master_id");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_pom_data_detail_calculator_run_pom_data_master_id",
                table: "calculator_run_pom_data_detail",
                column: "calculator_run_pom_data_master_id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_calculator_run_default_parameter_setting_master_default_parameter_setting_master_id",
                table: "calculator_run",
                column: "default_parameter_setting_master_id",
                principalTable: "default_parameter_setting_master",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_calculator_run_lapcap_data_master_lapcap_data_master_id",
                table: "calculator_run",
                column: "lapcap_data_master_id",
                principalTable: "lapcap_data_master",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_calculator_run_calculator_run_organization_data_master_calculator_run_organization_data_master_id",
                table: "calculator_run");

            migrationBuilder.DropForeignKey(
                name: "FK_calculator_run_calculator_run_pom_data_master_calculator_run_pom_data_master_id",
                table: "calculator_run");

            migrationBuilder.DropForeignKey(
                name: "FK_calculator_run_default_parameter_setting_master_default_parameter_setting_master_id",
                table: "calculator_run");

            migrationBuilder.DropForeignKey(
                name: "FK_calculator_run_lapcap_data_master_lapcap_data_master_id",
                table: "calculator_run");

            migrationBuilder.DropTable(
                name: "calculator_run_organization_data_detail");

            migrationBuilder.DropTable(
                name: "calculator_run_pom_data_detail");

            migrationBuilder.DropTable(
                name: "organization_data");

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
                name: "IX_calculator_run_default_parameter_setting_master_id",
                table: "calculator_run");

            migrationBuilder.DropIndex(
                name: "IX_calculator_run_lapcap_data_master_id",
                table: "calculator_run");

            migrationBuilder.DropColumn(
                name: "calculator_run_organization_data_master_id",
                table: "calculator_run");

            migrationBuilder.DropColumn(
                name: "calculator_run_pom_data_master_id",
                table: "calculator_run");

            migrationBuilder.DropColumn(
                name: "default_parameter_setting_master_id",
                table: "calculator_run");

            migrationBuilder.DropColumn(
                name: "lapcap_data_master_id",
                table: "calculator_run");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 9, 24, 20, 44, 8, 278, DateTimeKind.Local).AddTicks(6481));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_at",
                value: new DateTime(2024, 9, 24, 20, 44, 8, 278, DateTimeKind.Local).AddTicks(6486));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_at",
                value: new DateTime(2024, 9, 24, 20, 44, 8, 278, DateTimeKind.Local).AddTicks(6491));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_at",
                value: new DateTime(2024, 9, 24, 20, 44, 8, 278, DateTimeKind.Local).AddTicks(6495));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_at",
                value: new DateTime(2024, 9, 24, 20, 44, 8, 278, DateTimeKind.Local).AddTicks(6500));
        }
    }
}
