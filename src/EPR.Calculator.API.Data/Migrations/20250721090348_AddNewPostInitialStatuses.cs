using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewPostInitialStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "calculator_run_classification",
                columns: new[] { "id", "created_by", "status" },
                values: new object[,]
                {
                    { 12, "System User", "INTERIM RE-CALCULATION RUN COMPLETED" },
                    { 13, "System User", "FINAL RE-CALCULATION RUN COMPLETED" },
                    { 14, "System User", "FINAL RUN COMPLETED" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
