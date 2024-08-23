using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCalculatorRunTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calculator_run_classification",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    status = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculator_run_classification", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calculator_run",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_classification_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    financial_year = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculator_run", x => x.id);
                    table.ForeignKey(
                        name: "FK_calculator_run_calculator_run_classification_calculator_run_classification_id",
                        column: x => x.calculator_run_classification_id,
                        principalTable: "calculator_run_classification",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "calculator_run_classification",
                columns: new[] { "id", "created_at", "created_by", "status" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 8, 23, 12, 19, 7, 621, DateTimeKind.Local).AddTicks(6280), "Test User", "IN THE QUEUE" },
                    { 2, new DateTime(2024, 8, 23, 12, 19, 7, 621, DateTimeKind.Local).AddTicks(6289), "Test User", "RUNNING" },
                    { 3, new DateTime(2024, 8, 23, 12, 19, 7, 621, DateTimeKind.Local).AddTicks(6297), "Test User", "UNCLASSIFIED" },
                    { 4, new DateTime(2024, 8, 23, 12, 19, 7, 621, DateTimeKind.Local).AddTicks(6305), "Test User", "PLAY" },
                    { 5, new DateTime(2024, 8, 23, 12, 19, 7, 621, DateTimeKind.Local).AddTicks(6312), "Test User", "ERROR" }
                });

            migrationBuilder.InsertData(
                table: "calculator_run",
                columns: new[] { "id", "calculator_run_classification_id", "created_at", "created_by", "financial_year", "name", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 5, 28, 10, 1, 0, 0, DateTimeKind.Unspecified), "Test User", "2024-25", "Default settings check", null, null },
                    { 2, 2, new DateTime(2025, 5, 21, 12, 9, 0, 0, DateTimeKind.Unspecified), "Test User", "2024-25", "Alteration check", null, null },
                    { 3, 3, new DateTime(2025, 5, 11, 9, 14, 0, 0, DateTimeKind.Unspecified), "Test User", "2024-25", "Test 10", null, null },
                    { 4, 4, new DateTime(2025, 5, 13, 11, 18, 0, 0, DateTimeKind.Unspecified), "Test User", "2024-25", "June check", null, null },
                    { 5, 4, new DateTime(2025, 5, 10, 8, 13, 0, 0, DateTimeKind.Unspecified), "Test User", "2024-25", "Pre June check", null, null },
                    { 6, 4, new DateTime(2025, 5, 8, 10, 0, 0, 0, DateTimeKind.Unspecified), "Test User", "2024-25", "Local Authority data check 5", null, null },
                    { 7, 4, new DateTime(2025, 5, 7, 11, 20, 0, 0, DateTimeKind.Unspecified), "Test User", "2024-25", "Local Authority data check 4", null, null },
                    { 8, 4, new DateTime(2025, 6, 24, 14, 29, 0, 0, DateTimeKind.Unspecified), "Test User", "2024-25", "Local Authority data check 3", null, null },
                    { 9, 4, new DateTime(2025, 6, 27, 16, 39, 12, 0, DateTimeKind.Unspecified), "Test User", "2024-25", "Local Authority data check 2", null, null },
                    { 10, 4, new DateTime(2025, 6, 14, 17, 6, 26, 0, DateTimeKind.Unspecified), "Test User", "2024-25", "Local Authority data check", null, null },
                    { 11, 5, new DateTime(2025, 5, 1, 9, 12, 0, 0, DateTimeKind.Unspecified), "Test User", "2024-25", "Fee adjustment check", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_calculator_run_classification_id",
                table: "calculator_run",
                column: "calculator_run_classification_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calculator_run");

            migrationBuilder.DropTable(
                name: "calculator_run_classification");
        }
    }
}
