using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeleteFinancialYearsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "calculator_run_financial_years",
                columns: new[] { "financial_Year", "description" },
                values: new object[,]
                {
                    { "2026-27", null },
                    { "2026-27", null },
                    { "2026-27", null },
                    { "2026-27", null },
                    { "2026-27", null },
                });
        }
    }
}
