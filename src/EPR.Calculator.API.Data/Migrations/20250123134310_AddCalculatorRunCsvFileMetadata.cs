using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCalculatorRunCsvFileMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calculator_run_csvfile_metadata",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    filename = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    blob_uri = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculator_run_csvfile_metadata", x => x.id);
                    table.ForeignKey(
                        name: "FK_calculator_run_csvfile_metadata_calculator_run_calculator_run_id",
                        column: x => x.calculator_run_id,
                        principalTable: "calculator_run",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_csvfile_metadata_calculator_run_id",
                table: "calculator_run_csvfile_metadata",
                column: "calculator_run_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calculator_run_csvfile_metadata");
        }
    }
}
