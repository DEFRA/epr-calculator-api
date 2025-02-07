using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPomDataDetailSubmissionPeriodIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_pom_data_detail_submission_period",
                table: "calculator_run_pom_data_detail",
                column: "submission_period");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_calculator_run_pom_data_detail_submission_period",
                table: "calculator_run_pom_data_detail");
        }
    }
}
