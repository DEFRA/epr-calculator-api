using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AmendErrorReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_error_report_error_type_error_type_id",
                table: "error_report");

            migrationBuilder.DropTable(
                name: "error_type");

            migrationBuilder.DropIndex(
                name: "IX_error_report_error_type_id",
                table: "error_report");

            migrationBuilder.DropColumn(
                name: "error_type_id",
                table: "error_report");

            migrationBuilder.AddColumn<string>(
                name: "error_code",
                table: "error_report",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "error_code",
                table: "error_report");

            migrationBuilder.AddColumn<int>(
                name: "error_type_id",
                table: "error_report",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "error_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_error_type", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "error_type",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Missing Registration Data" },
                    { 2, "Conflicting Obligations (Leaver Codes)" },
                    { 3, "Conflicting Obligations (Blank)" },
                    { 4, "No longer trading" },
                    { 5, "Not Obligated" },
                    { 6, "Compliance Scheme Leaver" },
                    { 7, "Compliance Scheme to Direct Producer" },
                    { 8, "Invalid Leaver Code" },
                    { 9, "Date input issue" },
                    { 10, "Invalid Organisation ID" },
                    { 11, "Missing POM data" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_error_report_error_type_id",
                table: "error_report",
                column: "error_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_error_type_name",
                table: "error_type",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_error_report_error_type_error_type_id",
                table: "error_report",
                column: "error_type_id",
                principalTable: "error_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
