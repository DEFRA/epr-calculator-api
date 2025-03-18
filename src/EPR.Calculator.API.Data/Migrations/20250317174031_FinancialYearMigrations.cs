using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinancialYearMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "calculator_run_financial_years",
                columns: new[] { "id", "description", "financial_Year" },
                values: new object[] { 2, null, "2025-26" });

            migrationBuilder.InsertData(
                table: "calculator_run_financial_years",
                columns: new[] { "id", "description", "financial_Year" },
                values: new object[] { 3, null, "2026-27" });

            migrationBuilder.InsertData(
                table: "calculator_run_financial_years",
                columns: new[] { "id", "description", "financial_Year" },
                values: new object[] { 4, null, "2027-28" });

            migrationBuilder.InsertData(
                table: "calculator_run_financial_years",
                columns: new[] { "id", "description", "financial_Year" },
                values: new object[] { 5, null, "2028-29" });

            migrationBuilder.InsertData(
                table: "calculator_run_financial_years",
                columns: new[] { "id", "description", "financial_Year" },
                values: new object[] { 6, null, "2029-30" });

            migrationBuilder.InsertData(
                table: "calculator_run_financial_years",
                columns: new[] { "id", "description", "financial_Year" },
                values: new object[] { 7, null, "2030-31" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "calculator_run_financial_years",
                keyColumn: "financial_Year",
                keyValue: "2025-26");

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
