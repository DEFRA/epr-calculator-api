using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTestRunClassificationMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
               table: "calculator_run_classification",
               columns: new[]
               {
                    "status",
                    "created_by",
               },
               values: new object[,]
               {
                    {
                        "TEST",
                        "Test user",
                    },
               });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "calculator_run_classification",
                keyColumn: "status",
                keyValue: "TEST");
        }
    }
}
