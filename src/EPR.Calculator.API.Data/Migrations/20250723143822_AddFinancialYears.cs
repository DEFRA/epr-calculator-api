using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialYears : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
              table: "calculator_run_financial_years",
              column: "financial_Year",
              value: "2026-27");

            migrationBuilder.InsertData(
               table: "calculator_run_financial_years",
               column: "financial_Year",
               value: "2027-28");

            migrationBuilder.InsertData(
               table: "calculator_run_financial_years",
               column: "financial_Year",
               value: "2028-29");

            migrationBuilder.InsertData(
               table: "calculator_run_financial_years",
               column: "financial_Year",
               value: "2029-30");

            migrationBuilder.InsertData(
               table: "calculator_run_financial_years",
               column: "financial_Year",
               value: "2030-31");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
               table: "calculator_run_financial_years",
               keyColumn: "financial_Year",
               keyValue: "2026-27");

            migrationBuilder.DeleteData(
               table: "calculator_run_financial_years",
               keyColumn: "financial_Year",
               keyValue: "2027-28");

            migrationBuilder.DeleteData(
               table: "calculator_run_financial_years",
               keyColumn: "financial_Year",
               keyValue: "2028-29");

            migrationBuilder.DeleteData(
               table: "calculator_run_financial_years",
               keyColumn: "financial_Year",
               keyValue: "2029-30");

            migrationBuilder.DeleteData(
               table: "calculator_run_financial_years",
               keyColumn: "financial_Year",
               keyValue: "2030-31");
        }
    }
}
