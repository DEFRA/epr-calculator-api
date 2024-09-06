using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class LapcapData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lapcap_data_master",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    year = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    effective_from = table.Column<DateTime>(type: "datetime2", nullable: false),
                    effective_to = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lapcap_data_master", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lapcap_data_template_master",
                columns: table => new
                {
                    unique_ref = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    country = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    material = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    total_cost_from = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    total_cost_to = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lapcap_data_template_master", x => x.unique_ref);
                });

            migrationBuilder.CreateTable(
                name: "lapcap_data_detail",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    lapcap_data_master_id = table.Column<int>(type: "int", nullable: false),
                    lapcap_data_template_master_unique_ref = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    total_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lapcap_data_detail", x => x.id);
                    table.ForeignKey(
                        name: "FK_lapcap_data_detail_lapcap_data_master_lapcap_data_master_id",
                        column: x => x.lapcap_data_master_id,
                        principalTable: "lapcap_data_master",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lapcap_data_detail_lapcap_data_template_master_lapcap_data_template_master_unique_ref",
                        column: x => x.lapcap_data_template_master_unique_ref,
                        principalTable: "lapcap_data_template_master",
                        principalColumn: "unique_ref",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 9, 5, 12, 5, 3, 773, DateTimeKind.Local).AddTicks(2558));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_at",
                value: new DateTime(2024, 9, 5, 12, 5, 3, 773, DateTimeKind.Local).AddTicks(2561));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_at",
                value: new DateTime(2024, 9, 5, 12, 5, 3, 773, DateTimeKind.Local).AddTicks(2563));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_at",
                value: new DateTime(2024, 9, 5, 12, 5, 3, 773, DateTimeKind.Local).AddTicks(2565));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_at",
                value: new DateTime(2024, 9, 5, 12, 5, 3, 773, DateTimeKind.Local).AddTicks(2567));

            migrationBuilder.CreateIndex(
                name: "IX_lapcap_data_detail_lapcap_data_master_id",
                table: "lapcap_data_detail",
                column: "lapcap_data_master_id");

            migrationBuilder.CreateIndex(
                name: "IX_lapcap_data_detail_lapcap_data_template_master_unique_ref",
                table: "lapcap_data_detail",
                column: "lapcap_data_template_master_unique_ref",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lapcap_data_detail");

            migrationBuilder.DropTable(
                name: "lapcap_data_master");

            migrationBuilder.DropTable(
                name: "lapcap_data_template_master");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 9, 2, 16, 33, 15, 335, DateTimeKind.Local).AddTicks(8091));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_at",
                value: new DateTime(2024, 9, 2, 16, 33, 15, 335, DateTimeKind.Local).AddTicks(8097));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_at",
                value: new DateTime(2024, 9, 2, 16, 33, 15, 335, DateTimeKind.Local).AddTicks(8102));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_at",
                value: new DateTime(2024, 9, 2, 16, 33, 15, 335, DateTimeKind.Local).AddTicks(8106));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_at",
                value: new DateTime(2024, 9, 2, 16, 33, 15, 335, DateTimeKind.Local).AddTicks(8110));
        }
    }
}
