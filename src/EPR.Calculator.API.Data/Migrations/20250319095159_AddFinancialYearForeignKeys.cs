using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialYearForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_calculator_run_calculator_run_financial_years_financial_year",
                table: "calculator_run",
                column: "financial_year",
                principalTable: "calculator_run_financial_years",
                principalColumn: "financial_Year",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_default_parameter_setting_master_calculator_run_financial_years_parameter_year",
                table: "default_parameter_setting_master",
                column: "parameter_year",
                principalTable: "calculator_run_financial_years",
                principalColumn: "financial_Year",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_lapcap_data_master_calculator_run_financial_years_projection_year",
                table: "lapcap_data_master",
                column: "projection_year",
                principalTable: "calculator_run_financial_years",
                principalColumn: "financial_Year",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_calculator_run_calculator_run_financial_years_financial_year",
                table: "calculator_run");

            migrationBuilder.DropForeignKey(
                name: "FK_default_parameter_setting_master_calculator_run_financial_years_parameter_year",
                table: "default_parameter_setting_master");

            migrationBuilder.DropForeignKey(
                name: "FK_lapcap_data_master_calculator_run_financial_years_projection_year",
                table: "lapcap_data_master");
        }
    }
}
