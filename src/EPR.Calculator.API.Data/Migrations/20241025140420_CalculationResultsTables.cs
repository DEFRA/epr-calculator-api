using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class CalculationResultsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cost_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cost_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "country",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_country", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "material",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_material", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "producer_detail",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    producer_id = table.Column<int>(type: "int", nullable: false),
                    subsidiary_id = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    producer_name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producer_detail", x => x.id);
                    table.ForeignKey(
                        name: "FK_producer_detail_calculator_run_calculator_run_id",
                        column: x => x.calculator_run_id,
                        principalTable: "calculator_run",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "country_apportionment",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    apportionment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    country_id = table.Column<int>(type: "int", nullable: false),
                    cost_type_id = table.Column<int>(type: "int", nullable: false),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_country_apportionment", x => x.id);
                    table.ForeignKey(
                        name: "FK_country_apportionment_calculator_run_calculator_run_id",
                        column: x => x.calculator_run_id,
                        principalTable: "calculator_run",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_country_apportionment_cost_type_cost_type_id",
                        column: x => x.cost_type_id,
                        principalTable: "cost_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_country_apportionment_country_country_id",
                        column: x => x.country_id,
                        principalTable: "country",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "producer_reported_material",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    material_id = table.Column<int>(type: "int", nullable: false),
                    producer_detail_id = table.Column<int>(type: "int", nullable: false),
                    packaging_type = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    packaging_tonnage = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producer_reported_material", x => x.id);
                    table.ForeignKey(
                        name: "FK_producer_reported_material_material_material_id",
                        column: x => x.material_id,
                        principalTable: "material",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_producer_reported_material_producer_detail_producer_detail_id",
                        column: x => x.producer_detail_id,
                        principalTable: "producer_detail",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 10, 25, 15, 4, 19, 929, DateTimeKind.Local).AddTicks(1118));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_at",
                value: new DateTime(2024, 10, 25, 15, 4, 19, 929, DateTimeKind.Local).AddTicks(1125));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_at",
                value: new DateTime(2024, 10, 25, 15, 4, 19, 929, DateTimeKind.Local).AddTicks(1130));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_at",
                value: new DateTime(2024, 10, 25, 15, 4, 19, 929, DateTimeKind.Local).AddTicks(1135));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_at",
                value: new DateTime(2024, 10, 25, 15, 4, 19, 929, DateTimeKind.Local).AddTicks(1140));

            migrationBuilder.CreateIndex(
                name: "IX_country_apportionment_calculator_run_id",
                table: "country_apportionment",
                column: "calculator_run_id");

            migrationBuilder.CreateIndex(
                name: "IX_country_apportionment_cost_type_id",
                table: "country_apportionment",
                column: "cost_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_country_apportionment_country_id",
                table: "country_apportionment",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_producer_detail_calculator_run_id",
                table: "producer_detail",
                column: "calculator_run_id");

            migrationBuilder.CreateIndex(
                name: "IX_producer_reported_material_material_id",
                table: "producer_reported_material",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_producer_reported_material_producer_detail_id",
                table: "producer_reported_material",
                column: "producer_detail_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "country_apportionment");

            migrationBuilder.DropTable(
                name: "producer_reported_material");

            migrationBuilder.DropTable(
                name: "cost_type");

            migrationBuilder.DropTable(
                name: "country");

            migrationBuilder.DropTable(
                name: "material");

            migrationBuilder.DropTable(
                name: "producer_detail");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 10, 24, 14, 43, 19, 961, DateTimeKind.Local).AddTicks(7112));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_at",
                value: new DateTime(2024, 10, 24, 14, 43, 19, 961, DateTimeKind.Local).AddTicks(7118));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_at",
                value: new DateTime(2024, 10, 24, 14, 43, 19, 961, DateTimeKind.Local).AddTicks(7123));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_at",
                value: new DateTime(2024, 10, 24, 14, 43, 19, 961, DateTimeKind.Local).AddTicks(7128));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_at",
                value: new DateTime(2024, 10, 24, 14, 43, 19, 961, DateTimeKind.Local).AddTicks(7132));
        }
    }
}
