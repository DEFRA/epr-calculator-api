using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewPostInitialStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasBillingFileGenerated",
                table: "calculator_run");

            migrationBuilder.InsertData(
                table: "calculator_run_classification",
                columns: new[] { "id", "created_by", "status" },
                values: new object[,]
                {
                    { 6, "System User", "DELETED" },
                    { 7, "System User", "INITIAL RUN COMPLETED" },
                    { 8, "Test User", "INITIAL RUN" },
                    { 9, "Test User", "INTERIM RE-CALCULATION RUN" },
                    { 10, "Test User", "FINAL RUN" },
                    { 11, "Test User", "FINAL RE-CALCULATION RUN" },
                    { 12, "System User", "INTERIM RE-CALCULATION RUN COMPLETED" },
                    { 13, "System User", "FINAL RE-CALCULATION RUN COMPLETED" },
                    { 14, "System User", "FINAL RUN COMPLETED" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 14);

            migrationBuilder.AddColumn<bool>(
                name: "HasBillingFileGenerated",
                table: "calculator_run",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
