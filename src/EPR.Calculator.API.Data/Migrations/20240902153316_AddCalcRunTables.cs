using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCalcRunTables : Migration
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
                    { 1, new DateTime(2024, 9, 2, 16, 33, 15, 335, DateTimeKind.Local).AddTicks(8091), "Test User", "IN THE QUEUE" },
                    { 2, new DateTime(2024, 9, 2, 16, 33, 15, 335, DateTimeKind.Local).AddTicks(8097), "Test User", "RUNNING" },
                    { 3, new DateTime(2024, 9, 2, 16, 33, 15, 335, DateTimeKind.Local).AddTicks(8102), "Test User", "UNCLASSIFIED" },
                    { 4, new DateTime(2024, 9, 2, 16, 33, 15, 335, DateTimeKind.Local).AddTicks(8106), "Test User", "PLAY" },
                    { 5, new DateTime(2024, 9, 2, 16, 33, 15, 335, DateTimeKind.Local).AddTicks(8110), "Test User", "ERROR" }
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
