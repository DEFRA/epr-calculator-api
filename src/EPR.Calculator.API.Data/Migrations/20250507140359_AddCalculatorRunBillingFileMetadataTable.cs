using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCalculatorRunBillingFileMetadataTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calculator_run_billing_file_metadata",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    billing_csv_filename = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    billing_json_filename = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    billing_file_created_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    billing_file_created_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    billing_file_authorised_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    billing_file_authorised_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculator_run_billing_file_metadata", x => x.id);
                    table.ForeignKey(
                        name: "FK_calculator_run_billing_file_metadata_calculator_run_calculator_run_id",
                        column: x => x.calculator_run_id,
                        principalTable: "calculator_run",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_billing_file_metadata_calculator_run_id",
                table: "calculator_run_billing_file_metadata",
                column: "calculator_run_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calculator_run_billing_file_metadata");
        }
    }
}
