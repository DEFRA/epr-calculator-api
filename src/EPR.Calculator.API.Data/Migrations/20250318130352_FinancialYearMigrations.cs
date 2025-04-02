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
                columns: new[] { "financial_Year", "description" },
                values: new object[] { "2023-24", null });
            migrationBuilder.InsertData(
                table: "calculator_run_financial_years",
                columns: new[] { "financial_Year", "description" },
                values: new object[] { "2024-25", null });
            migrationBuilder.InsertData(
                table: "calculator_run_financial_years",
                columns: new[] { "financial_Year", "description" },
                values: new object[] { "2025-26", null });

            migrationBuilder.InsertData(
                table: "calculator_run_financial_years",
                columns: new[] { "financial_Year", "description" },
                values: new object[] { "2026-27", null });

            migrationBuilder.InsertData(
                table: "calculator_run_financial_years",
                columns: new[] { "financial_Year", "description" },
                values: new object[] { "2027-28", null });

            migrationBuilder.InsertData(
                table: "calculator_run_financial_years",
                columns: new[] { "financial_Year", "description" },
                values: new object[] { "2028-29", null });

            migrationBuilder.InsertData(
                table: "calculator_run_financial_years",
                columns: new[] { "financial_Year", "description" },
                values: new object[] { "2029-30", null });

            migrationBuilder.InsertData(
                table: "calculator_run_financial_years",
                columns: new[] { "financial_Year", "description" },
                values: new object[] { "2030-31", null });
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
