using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialYearTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "projection_year",
                table: "lapcap_data_master",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "parameter_year",
                table: "default_parameter_setting_master",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "financial_year",
                table: "calculator_run",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.CreateTable(
                name: "calculator_run_financial_years",
                columns: table => new
                {
                    financial_Year = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculator_run_financial_years", x => x.financial_Year);
                });

            migrationBuilder.CreateIndex(
                name: "IX_lapcap_data_master_projection_year",
                table: "lapcap_data_master",
                column: "projection_year");

            migrationBuilder.CreateIndex(
                name: "IX_default_parameter_setting_master_parameter_year",
                table: "default_parameter_setting_master",
                column: "parameter_year");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_financial_year",
                table: "calculator_run",
                column: "financial_year");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calculator_run_financial_years");

            migrationBuilder.DropIndex(
                name: "IX_lapcap_data_master_projection_year",
                table: "lapcap_data_master");

            migrationBuilder.DropIndex(
                name: "IX_default_parameter_setting_master_parameter_year",
                table: "default_parameter_setting_master");

            migrationBuilder.DropIndex(
                name: "IX_calculator_run_financial_year",
                table: "calculator_run");

            migrationBuilder.AlterColumn<string>(
                name: "projection_year",
                table: "lapcap_data_master",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "parameter_year",
                table: "default_parameter_setting_master",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "financial_year",
                table: "calculator_run",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
